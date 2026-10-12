using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1ステージ（3〜5階層）の流れを管理する：階層づくり → カウントダウン → プレイ → 強化を選ぶ → 次の階層 → クリア/ゲームオーバー
public class StageRunController : MonoBehaviour
{
  public static StageRunController Instance { get; private set; }

  public GameObject enemySlimePrefab;
  public GameObject playerSlimePrefab;

  enum Phase { Setup, Countdown, Playing, Serving, Choosing, Ended }

  const float LifeSpacingX = 0.95f;
  // バーが壁にめり込まない範囲：壁の内側の端 - バーの半分の幅
  const float FieldHalfWidth = 2.42f;
  const float PaddleHalfWidth = 0.61f;
  const float ExtraBallOffsetX = 0.4f;
  const float ChoiceDelaySeconds = 0.6f;
  const int BossBurstCount = 4;
  const int ModalSortingOrder = 100;
  const float BossBottomOffsetY = -2f;
  // 防衛ライン：プレイヤーのバーより少し上。モンスターがここまで迫ると仲間が食べられる
  const float DefenseLineAbovePaddle = 0.95f;
  // 属性の効果
  const float FireRadius = 1.3f;
  const int FireDamage = 1;
  const int ThunderDamage = 2;
  const float IceFreezeSeconds = 2.5f;
  // 仲間がやられてから、ボールを打ち直すまでの間
  const float ServeDelaySeconds = 1f;

  Phase phase;
  int floorIndex;
  int floorCount;
  int combo;
  int paddleHits;
  float defenseLineY;
  SpriteRenderer defenseLine;
  FloorData floor;
  Boss boss;
  Shield shield;

  Transform monstersRoot;
  Transform livesRoot;
  BallManager mainBall;
  readonly List<BallManager> extraBalls = new List<BallManager>();
  readonly List<Vector3> extraBallStarts = new List<Vector3>();
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
    defenseLineY = player.transform.position.y + DefenseLineAbovePaddle;
    defenseLine = CreateDefenseLine(defenseLineY);

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

  // 赤い点線で防衛ラインを見せる
  static SpriteRenderer CreateDefenseLine(float y)
  {
    var line = new GameObject("DefenseLine").AddComponent<SpriteRenderer>();
    line.sprite = PixelArt.WorldSprite("defense_line");
    line.color = new Color(1f, 1f, 1f, 0.5f);
    line.transform.position = new Vector3(0f, y, 0f);
    return line;
  }

  void StartFloor(int index)
  {
    floorIndex = index;
    floor = StageDatabase.GetFloor(RunState.Stage, floorIndex);
    combo = 0;
    paddleHits = 0;
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
    defenseLine.enabled = !floor.IsBossFloor;
    if (floor.IsBossFloor)
    {
      // 画面の縦横比が変わっても同じ並びになるよう、敵の親オブジェクトの位置を基準にする
      boss = Boss.Create(floor, monstersRoot.position.y + BossBottomOffsetY);
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
    extraBallStarts.Clear();
  }

  void SpawnMonsters()
  {
    foreach (var spawn in floor.Monsters)
    {
      // 画面の縦横比が変わっても同じ並びになるよう、敵の親オブジェクトの位置を基準にする
      var monster = Instantiate(enemySlimePrefab, monstersRoot);
      monster.transform.position = new Vector3(spawn.Offset.x, monstersRoot.position.y + spawn.Offset.y, 0f);
      var enemy = monster.GetComponent<EnemyManager>();
      enemy.ConfigureMonster(spawn.Hp, spawn.Element, spawn.WalkSpeed, spawn.WalkRange);
      enemy.Killed = killed => OnMonsterDefeated(killed.transform.position);
      enemy.ReachedDefenseLine = OnMonsterReachedDefenseLine;
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
      var lifeSlime = life.GetComponent<EnemyManager>();
      lifeSlime.ConfigureLife(PaddleSide.Bottom);
      lifeSlime.Killed = OnLifeLost;
    }
  }

  void SetUpEnemyBar()
  {
    enemyBar.gameObject.SetActive(floor.HasEnemyBar);
    enemyBar.Configure(floor.EnemyStyle, floor.EnemyLevel, floor.EnemyBarLimitX);
    Vector3 position = enemyBar.transform.position;
    position.x = 0f;
    enemyBar.transform.position = position;
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
      extraBallStarts.Add(position);
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

  public void OnPaddleHit(Paddle paddle, bool isSmash, Vector3 position)
  {
    if (paddle.Side != PaddleSide.Bottom || phase != Phase.Playing)
    {
      return;
    }
    combo = 0;
    if (isSmash)
    {
      hud.Popup("SMASH!", position, PixelArt.Yellow);
      HitEffects.Burst(position, PixelArt.Yellow);
    }
    paddleHits++;
    if (floor.AdvanceEveryHits > 0 && !floor.IsBossFloor && paddleHits % floor.AdvanceEveryHits == 0)
    {
      AdvanceMonsters();
    }
  }

  // 何回か打ち返すごとに、モンスターが一段迫ってくる
  void AdvanceMonsters()
  {
    foreach (var monster in monstersRoot.GetComponentsInChildren<EnemyManager>())
    {
      monster.Advance(floor.AdvanceStep, defenseLineY);
    }
  }

  // 防衛ラインまで来たモンスターは、仲間スライムを1匹食べて消える
  void OnMonsterReachedDefenseLine(EnemyManager monster)
  {
    HitEffects.Burst(monster.transform.position, PixelArt.Red);
    hud.Popup("たべられた!", monster.transform.position, PixelArt.Red);
    EnemyManager nearest = null;
    foreach (var life in livesRoot.GetComponentsInChildren<EnemyManager>())
    {
      if (nearest == null || Mathf.Abs(life.transform.position.x - monster.transform.position.x) < Mathf.Abs(nearest.transform.position.x - monster.transform.position.x))
      {
        nearest = life;
      }
    }
    if (nearest != null)
    {
      nearest.Kill();
    }
  }

  // ボールがモンスターに当たったときのダメージ。ボールの属性の効果もここで出す
  public int OnBallHitMonster(BallManager ball, EnemyManager monster)
  {
    int damage = RollBallDamage() + ball.ConsumeSmashBonus();
    if (ball.LoadedElement != Element.None)
    {
      TriggerElement(ball.LoadedElement, monster);
      ball.LoadedElement = Element.None;
    }
    // 倒せる一撃なら、そのモンスターの属性をボールがまとう
    if (damage >= monster.Hp && monster.Element != Element.None)
    {
      ball.LoadedElement = monster.Element;
      hud.Popup(Elements.NameOf(monster.Element) + "!", monster.transform.position, Elements.ColorOf(monster.Element));
    }
    return damage;
  }

  // ボールがボスに当たったときのダメージ（属性をまとっていれば +1）
  public int OnBallHitBoss(BallManager ball)
  {
    int damage = RollBallDamage() + ball.ConsumeSmashBonus();
    if (ball.LoadedElement != Element.None)
    {
      damage++;
      ball.LoadedElement = Element.None;
    }
    return damage;
  }

  void TriggerElement(Element element, EnemyManager hitMonster)
  {
    Vector3 center = hitMonster.transform.position;
    var others = new List<EnemyManager>();
    foreach (var monster in monstersRoot.GetComponentsInChildren<EnemyManager>())
    {
      if (monster != hitMonster)
      {
        others.Add(monster);
      }
    }
    switch (element)
    {
      case Element.Fire:
        // ほのお：まわりのモンスターにも燃え広がる
        HitEffects.Burst(center, Elements.ColorOf(element));
        foreach (var monster in others)
        {
          if (Vector3.Distance(monster.transform.position, center) <= FireRadius)
          {
            monster.TakeDamage(FireDamage);
          }
        }
        break;
      case Element.Thunder:
        // かみなり：一番近いモンスターに連鎖する
        EnemyManager nearest = null;
        foreach (var monster in others)
        {
          if (nearest == null || Vector3.Distance(monster.transform.position, center) < Vector3.Distance(nearest.transform.position, center))
          {
            nearest = monster;
          }
        }
        if (nearest != null)
        {
          HitEffects.Line(center, nearest.transform.position, Elements.ColorOf(element));
          nearest.TakeDamage(ThunderDamage);
        }
        break;
      case Element.Ice:
        // こおり：敵のバーを少しの間こおらせる
        if (enemyBar.gameObject.activeSelf)
        {
          enemyBar.Freeze(IceFreezeSeconds);
          HitEffects.Burst(enemyBar.transform.position, Elements.ColorOf(element));
        }
        break;
    }
    hud.Popup(Elements.NameOf(element) + "!", center, Elements.ColorOf(element));
  }

  // ボールが当たったときのダメージ（かいしんの一撃は2倍）
  int RollBallDamage()
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

  public void OnLifeLost(EnemyManager life)
  {
    if (phase != Phase.Playing)
    {
      return;
    }
    Vector3 position = life.transform.position;
    combo = 0;
    RunState.LoseLife();
    HitEffects.Burst(position, PixelArt.Red);
    HitEffects.Shake();
    if (OptionManager.isSePlaying)
    {
      HitEffects.Vibrate();
    }
    if (RunState.Lives <= 0 && RunState.TryRevive())
    {
      hud.Popup("ふっかつ!", position, PixelArt.Green);
      StartCoroutine(RespawnLivesNextFrame());
    }
    else if (RunState.Lives <= 0)
    {
      EndStage(false);
      return;
    }
    // ボールに抜かれてやられたときは、ホッケーの得点のようにボールを戻して打ち直す
    if (life.IsKilledByBall)
    {
      StartCoroutine(Serve());
    }
  }

  IEnumerator Serve()
  {
    phase = Phase.Serving;
    StopBalls();
    foreach (var bullet in FindObjectsByType<BossBullet>(FindObjectsSortMode.None))
    {
      Destroy(bullet.gameObject);
    }
    mainBall.transform.position = ballStartPosition;
    for (int i = 0; i < extraBalls.Count; i++)
    {
      extraBalls[i].transform.position = extraBallStarts[i];
    }
    yield return new WaitForSeconds(ServeDelaySeconds);
    if (phase == Phase.Serving)
    {
      LaunchBalls();
      phase = Phase.Playing;
    }
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
    LeaveTo("StagePlay");
  }

  void RetryStage()
  {
    RunState.SelectedStage = RunState.Stage;
    LeaveTo("StagePlay");
  }

  void GoToMap()
  {
    LeaveTo("Menu");
  }

  void LeaveTo(string sceneName)
  {
    // 広告が出たら、その閉じるボタンを結果画面の暗幕が隠さないよう結果画面を閉じる（どのみちシーンは切り替わる）
    if (sceneChanger.GoToScene(sceneName))
    {
      modalCanvas.gameObject.SetActive(false);
    }
  }
}
