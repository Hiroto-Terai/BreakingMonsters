using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1ステージ（3〜5階層）の流れを管理する：階層づくり → カウントダウン → プレイ → 強化を選ぶ → 次の階層 → クリア/ゲームオーバー
public class StageRunController : MonoBehaviour
{
  public static StageRunController Instance { get; private set; }

  public GameObject enemySlimePrefab;
  public GameObject playerSlimePrefab;

  enum Phase { Setup, Countdown, Playing, Choosing, Ended }

  const float LifeSpacingX = 0.95f;
  // バーが壁にめり込まない範囲：壁の内側の端 - バーの半分の幅
  const float FieldHalfWidth = 2.42f;
  const float PaddleHalfWidth = 0.61f;
  const float ExtraBallOffsetX = 0.4f;
  const float ChoiceDelaySeconds = 0.6f;
  const int BossBurstCount = 4;
  const int ModalSortingOrder = 100;

  Phase phase;
  int floorIndex;
  int floorCount;
  int combo;
  FloorData floor;
  Boss boss;
  Shield shield;

  Transform monstersRoot;
  Transform livesRoot;
  BallManager mainBall;
  readonly List<BallManager> extraBalls = new List<BallManager>();
  Vector3 ballStartPosition;
  Vector3 ballBaseScale;
  CountDown countDown;
  EnemyBarManager enemyBar;
  PlayerManager player;
  Vector3 playerBaseScale;
  StageHud hud;
  Canvas modalCanvas;
  OptionManager sceneChanger;
  UpgradeChoicePanel choicePanel;
  readonly System.Random random = new System.Random();

  public bool IsPlaying => phase == Phase.Playing;

  void Awake()
  {
    Instance = this;
  }

  void OnDestroy()
  {
    if (Instance == this)
    {
      Instance = null;
    }
  }

  // ボールや敵の位置は各オブジェクトの Start で画面に合わせて決まるので、それが終わる1フレーム後に始める
  IEnumerator Start()
  {
    yield return null;
    RunState.Begin(RunState.SelectedStage);
    floorCount = StageDatabase.GetFloorCount(RunState.Stage);

    monstersRoot = GameObject.Find("EnemySlimes").transform;
    livesRoot = GameObject.Find("PlayerSlimes").transform;
    mainBall = GameObject.Find("Ball").GetComponent<BallManager>();
    ballStartPosition = mainBall.transform.position;
    ballBaseScale = mainBall.transform.localScale;
    countDown = FindFirstObjectByType<CountDown>();
    enemyBar = FindFirstObjectByType<EnemyBarManager>();
    player = FindFirstObjectByType<PlayerManager>();
    playerBaseScale = player.transform.localScale;
    sceneChanger = FindSceneChanger();

    hud = new StageHud(FindFirstObjectByType<OptionButtonPosition>().GetComponent<RectTransform>());
    modalCanvas = PixelUI.CreateOverlayCanvas("ModalCanvas", ModalSortingOrder);
    hud.SetCoins(0);
    StartFloor(0);
  }

  // 広告を挟んだシーン切り替えは、広告の設定を持っている OptionManager に任せる
  static OptionManager FindSceneChanger()
  {
    foreach (var option in FindObjectsByType<OptionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
    {
      if (option.GoogleInterStitialAds != null)
      {
        return option;
      }
    }
    return null;
  }

  void StartFloor(int index)
  {
    floorIndex = index;
    floor = StageDatabase.GetFloor(RunState.Stage, floorIndex);
    combo = 0;
    ClearField();
    SpawnMonsters();
    SpawnLives();
    SetUpEnemyBar();
    SetUpPaddle();
    SetUpBalls();
    if (RunState.HasFloorShield)
    {
      shield = Shield.Create();
    }
    hud.SetStage(RunState.Stage, floorIndex, floorCount);
    if (floor.IsBossFloor)
    {
      boss = Boss.Create(floor);
      hud.ShowBossHp(boss.Hp, boss.MaxHp);
    }
    else
    {
      hud.HideBossHp();
    }
    countDown.Restart();
    phase = Phase.Countdown;
  }

  void ClearField()
  {
    foreach (Transform child in monstersRoot)
    {
      Destroy(child.gameObject);
    }
    foreach (Transform child in livesRoot)
    {
      Destroy(child.gameObject);
    }
    foreach (var bullet in FindObjectsByType<BossBullet>(FindObjectsSortMode.None))
    {
      Destroy(bullet.gameObject);
    }
    if (shield != null)
    {
      Destroy(shield.gameObject);
    }
    foreach (var ball in extraBalls)
    {
      Destroy(ball.gameObject);
    }
    extraBalls.Clear();
  }

  void SpawnMonsters()
  {
    foreach (var spawn in floor.Monsters)
    {
      // 画面の縦横比が変わっても同じ並びになるよう、敵の親オブジェクトの位置を基準にする
      var monster = Instantiate(enemySlimePrefab, monstersRoot);
      monster.transform.position = new Vector3(spawn.Offset.x, monstersRoot.position.y + spawn.Offset.y, 0f);
      monster.GetComponent<EnemyManager>().Configure(spawn.Hp, false);
    }
  }

  // 味方スライム（残りライフ）を横に並べる
  void SpawnLives()
  {
    for (int i = 0; i < RunState.Lives; i++)
    {
      var life = Instantiate(playerSlimePrefab, livesRoot);
      float x = (i - (RunState.Lives - 1) / 2f) * LifeSpacingX;
      life.transform.position = new Vector3(x, livesRoot.position.y, 0f);
      life.GetComponent<EnemyManager>().Configure(1, true);
    }
  }

  void SetUpEnemyBar()
  {
    enemyBar.gameObject.SetActive(floor.HasEnemyBar);
    enemyBar.speed = floor.EnemyBarSpeed;
    enemyBar.limitX = floor.EnemyBarLimitX;
  }

  void SetUpPaddle()
  {
    float scale = RunState.PaddleWidthScale;
    player.transform.localScale = new Vector3(playerBaseScale.x * scale, playerBaseScale.y, playerBaseScale.z);
    player.LimitX = FieldHalfWidth - PaddleHalfWidth * scale;
    Vector3 position = player.transform.position;
    position.x = 0f;
    player.transform.position = position;
  }

  void SetUpBalls()
  {
    ConfigureBall(mainBall, ballStartPosition);
    for (int i = 1; i < RunState.BallCount; i++)
    {
      float side = i % 2 == 1 ? -1f : 1f;
      var position = ballStartPosition + new Vector3(side * ExtraBallOffsetX * ((i + 1) / 2), 0f, 0f);
      var ball = Instantiate(mainBall.gameObject, position, Quaternion.identity).GetComponent<BallManager>();
      ball.name = "ExtraBall";
      ConfigureBall(ball, position);
      extraBalls.Add(ball);
    }
  }

  void ConfigureBall(BallManager ball, Vector3 position)
  {
    ball.Stop();
    ball.transform.position = position;
    ball.transform.localScale = ballBaseScale * RunState.BallSizeScale;
    ball.maxSpeed = floor.BallMaxSpeed * RunState.BallMaxSpeedScale;
    ball.minSpeed = Mathf.Min(floor.BallMinSpeed, ball.maxSpeed);
    ball.speed = ball.minSpeed / Mathf.Sqrt(2f);
    ball.IsFading = floor.IsBallFading;
  }

  void LaunchBalls()
  {
    mainBall.Launch(new Vector2(1f, 1f));
    for (int i = 0; i < extraBalls.Count; i++)
    {
      extraBalls[i].Launch(new Vector2(i % 2 == 0 ? -1f : 0.5f, 1f));
    }
  }

  void StopBalls()
  {
    mainBall.Stop();
    foreach (var ball in extraBalls)
    {
      ball.Stop();
    }
  }

  void Update()
  {
    if (phase == Phase.Countdown && countDown.IsFinished)
    {
      LaunchBalls();
      phase = Phase.Playing;
    }
    if (phase == Phase.Playing && IsFloorCleared())
    {
      OnFloorCleared();
    }
  }

  // 敵が全滅し、ボス階層ならボスも倒した
  bool IsFloorCleared()
  {
    if (floor.IsBossFloor)
    {
      return boss == null;
    }
    return monstersRoot.childCount == 0;
  }

  public void OnPaddleHit()
  {
    combo = 0;
  }

  // ボールが当たったときのダメージ（かいしんの一撃は2倍）
  public int RollBallDamage()
  {
    int damage = RunState.BallDamage;
    if (Random.value < RunState.CriticalChance)
    {
      damage *= 2;
      hud.Popup("CRITICAL!", mainBall.transform.position, PixelArt.Yellow);
    }
    if (boss != null)
    {
      // ボスのHPは当たった後に更新されるので、次のフレームで表示を合わせる
      StartCoroutine(RefreshBossHpNextFrame());
    }
    return damage;
  }

  IEnumerator RefreshBossHpNextFrame()
  {
    yield return null;
    if (boss != null)
    {
      hud.ShowBossHp(boss.Hp, boss.MaxHp);
    }
  }

  public void OnMonsterDefeated(Vector3 position)
  {
    combo++;
    HitEffects.Burst(position, PixelArt.LightBlue);
    if (combo >= 2)
    {
      hud.Popup(combo + " COMBO!", position, PixelArt.Yellow);
    }
  }

  public void OnBossDefeated(Vector3 position)
  {
    hud.HideBossHp();
    for (int i = 0; i < BossBurstCount; i++)
    {
      HitEffects.Burst(position + (Vector3)(Random.insideUnitCircle * 0.8f), i % 2 == 0 ? PixelArt.Yellow : PixelArt.LightBlue);
    }
    HitEffects.Shake();
    hud.Popup("BOSS DOWN!", position, PixelArt.Yellow);
  }

  public void OnLifeLost(Vector3 position)
  {
    if (phase != Phase.Playing)
    {
      return;
    }
    combo = 0;
    RunState.LoseLife();
    HitEffects.Burst(position, PixelArt.Red);
    HitEffects.Shake();
    if (OptionManager.isSePlaying)
    {
      HitEffects.Vibrate();
    }
    if (RunState.Lives > 0)
    {
      return;
    }
    if (RunState.TryRevive())
    {
      hud.Popup("ふっかつ!", position, PixelArt.Green);
      StartCoroutine(RespawnLivesNextFrame());
      return;
    }
    EndStage(false);
  }

  // やられたスライムが消えてから並べ直す
  IEnumerator RespawnLivesNextFrame()
  {
    yield return null;
    SpawnLives();
  }

  void OnFloorCleared()
  {
    phase = Phase.Choosing;
    StopBalls();
    foreach (var bullet in FindObjectsByType<BossBullet>(FindObjectsSortMode.None))
    {
      Destroy(bullet.gameObject);
    }
    int coins = RunState.AddCoins(StageDatabase.GetFloorReward(RunState.Stage));
    hud.SetCoins(RunState.CoinsEarned);
    hud.Popup("+" + coins + " COIN", player.transform.position + Vector3.up, PixelArt.Yellow);

    if (floorIndex + 1 >= floorCount)
    {
      RunState.AddCoins(StageDatabase.GetClearBonus(RunState.Stage));
      hud.SetCoins(RunState.CoinsEarned);
      EndStage(true);
      return;
    }
    StartCoroutine(ShowChoicesAfterDelay(coins));
  }

  IEnumerator ShowChoicesAfterDelay(int coins)
  {
    yield return new WaitForSecondsRealtime(ChoiceDelaySeconds);
    ShowChoices("+" + coins + " COIN  つよさを1つえらぼう");
  }

  void ShowChoices(string subtitle)
  {
    // 選んでいる間は時間を止める
    Time.timeScale = 0f;
    var choices = Upgrades.Roll(random);
    choicePanel = new UpgradeChoicePanel(modalCanvas.transform, "FLOOR " + (floorIndex + 1) + " CLEAR!", subtitle, choices, RunState.RerollsLeft, OnUpgradePicked, OnReroll);
  }

  void OnUpgradePicked(UpgradeDefinition upgrade)
  {
    choicePanel.Close();
    RunState.AddUpgrade(upgrade.Id);
    Time.timeScale = 1f;
    StartFloor(floorIndex + 1);
  }

  void OnReroll()
  {
    if (!RunState.TryUseReroll())
    {
      return;
    }
    choicePanel.Close();
    ShowChoices("えらびなおし のこり " + RunState.RerollsLeft);
  }

  void EndStage(bool isClear)
  {
    phase = Phase.Ended;
    StopBalls();
    int stars = isClear ? StarsForLives() : 0;
    if (isClear)
    {
      Progress.MarkCleared(RunState.Stage, stars);
    }
    // 負けても、それまでにクリアした階層のコインは持ち帰る
    Progress.AddCoins(RunState.CoinsEarned);

    var result = new StageResultPanel.Result
    {
      IsClear = isClear,
      Stage = RunState.Stage,
      Stars = stars,
      FloorsCleared = isClear ? floorCount : floorIndex,
      FloorCount = floorCount,
      CoinsEarned = RunState.CoinsEarned,
    };
    new StageResultPanel(modalCanvas.transform, result, GoToNextStage, RetryStage, GoToMap);
  }

  // 残ったスライムの割合で星を決める（全員無事なら3つ）
  static int StarsForLives()
  {
    if (RunState.Lives >= RunState.MaxLives) return 3;
    if (RunState.Lives * 2 >= RunState.MaxLives) return 2;
    return 1;
  }

  void GoToNextStage()
  {
    RunState.SelectedStage = RunState.Stage + 1;
    sceneChanger.GoToScene("StagePlay");
  }

  void RetryStage()
  {
    RunState.SelectedStage = RunState.Stage;
    sceneChanger.GoToScene("StagePlay");
  }

  void GoToMap()
  {
    sceneChanger.GoToScene("Menu");
  }
}
