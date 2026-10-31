using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1ステージ（3〜5階層）の流れを管理する：階層づくり → カウントダウン → プレイ（経験値でレベルアップ）→ 宝箱 → 次の階層 → クリア/ゲームオーバー
public class StageRunController : MonoBehaviour
{
  public static StageRunController Instance { get; private set; }

  public GameObject enemySlimePrefab;
  public GameObject playerSlimePrefab;

  enum Phase { Setup, Countdown, Playing, Serving, Choosing, ReviveOffer, Ended }

  public struct HitResult
  {
    public int Damage;
    public bool IsCritical;
  }

  const float LifeSpacingX = 0.95f;
  // バーが壁にめり込まない範囲：壁の内側の端 - バーの半分の幅
  const float FieldHalfWidth = 2.42f;
  const float PaddleHalfWidth = 0.61f;
  const float ExtraBallOffsetX = 0.4f;
  const int BossBurstCount = 6;
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

  // 手ごたえの演出
  const float SmashFreezeSeconds = 0.06f;
  const float CriticalFreezeSeconds = 0.04f;
  const float LastKillSlowScale = 0.25f;
  const float LastKillSlowSeconds = 0.7f;
  const float FloorEndWaitSeconds = 0.9f;
  const float GemCollectWaitSeconds = 1.6f;
  const int CriticalMultiplier = 3;
  const int MeteorMultiplier = 3;
  const int FeverBonusDamage = 1;
  const int KillBurstPieces = 14;

  // 強化の効果
  const int MaxMiniBalls = 30;
  const float MiniBallSpeed = 6.5f;
  const int BallStormMinis = 2;
  const float ExplosionChainDelay = 0.08f;
  const int MaxExplosionDepth = 3;
  const float ExplosionBossRange = 1.4f;
  const int MaxGemsPerMonster = 5;
  // 爆弾スライムの爆発（強化「ばくはつ」がなくても起きる）
  const float BombRadius = 1.15f;
  const int BombDamage = 2;
  // 2階層目からはカウントダウンを短くして、テンポよく続ける
  const float FirstFloorCountdown = 4f;
  const float LaterFloorCountdown = 2.5f;

  // 金色のスライム
  const int GoldenStartStage = 2;
  const float GoldenChance = 0.05f;
  const int GoldenBonusGems = 6;
  const int GoldenBonusCoins = 10;
  // ゴールデンラッシュは数が多いので、1体ぶんのおまけは少なくする（それでも合計はふだんの階層よりずっと多い）
  const int GoldenRushBonusGems = 2;
  const int GoldenRushBonusCoins = 2;

  // コンボの節目で出す言葉
  static readonly int[] ComboMilestones = { 10, 20, 30, 50, 75, 100 };
  static readonly string[] ComboWords = { "GREAT!", "AMAZING!", "INCREDIBLE!", "UNSTOPPABLE!", "GODLIKE!", "LEGEND!!" };
  const int MaxComboSemitones = 14;
  const int GemPitchCycle = 12;

  Phase phase;
  int floorIndex;
  int floorCount;
  int paddleHits;
  float defenseLineY;
  SpriteRenderer defenseLine;
  FloorData floor;
  Boss boss;
  Shield shield;

  Transform monstersRoot;
  Transform livesRoot;
  BallManager mainBall;
  SpriteRenderer mainBallSprite;
  // ボールが増えたときに、本物のボールに付ける輪
  SpriteRenderer mainBallRing;
  const int RingSortingOrder = 4;
  readonly List<BallManager> extraBalls = new List<BallManager>();
  readonly List<Vector3> extraBallStarts = new List<Vector3>();
  readonly List<BallManager> miniBalls = new List<BallManager>();
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
  // 広告を見ての ふっかつ は1ステージに1回まで
  bool hasUsedAdRevive;
  RevivePanel revivePanel;
  StageResultPanel resultPanel;
  ComboMeter combo;
  int pendingLevelUps;
  bool isPanelOpen;
  int gemStreak;
  // 爆発の連鎖の深さ（爆発で倒したモンスターがまた爆発するのは、進化「ビッグバン」のときだけ）
  int explosionDepth;
  readonly System.Random random = new System.Random();
  // フィーバー中に背景を赤く点滅させるため、フィールドの背景の板を持っておく
  Material fieldBackground;
  Color fieldBackgroundColor;
  const float FeverPulseSeconds = 0.2f;
  static readonly Color FeverBackgroundColor = new Color(0.35f, 0.05f, 0.12f);

  public bool IsPlaying => phase == Phase.Playing;
  public bool IsFever => combo != null && combo.IsFever;
  public int ShotDamage => 1 + RunState.GetUpgradeLevel(UpgradeId.PowerBall) / 2 + (IsFever ? FeverBonusDamage : 0);

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
    BallManager.IsFeverActive = false;
  }

  // ボールや敵の位置は各オブジェクトの Start で決まるので、それが終わる1フレーム後に始める
  IEnumerator Start()
  {
    yield return null;
    RunState.Begin(RunState.SelectedStage);
    floorCount = StageDatabase.GetFloorCount(RunState.Stage);

    monstersRoot = GameObject.Find("EnemySlimes").transform;
    livesRoot = GameObject.Find("PlayerSlimes").transform;
    mainBall = GameObject.Find("Ball").GetComponent<BallManager>();
    mainBallSprite = mainBall.GetComponent<SpriteRenderer>();
    mainBallRing = new GameObject("MainBallRing").AddComponent<SpriteRenderer>();
    mainBallRing.sprite = PixelArt.WorldSprite("main_ball_ring");
    mainBallRing.sortingOrder = RingSortingOrder;
    mainBallRing.enabled = false;
    ballStartPosition = mainBall.transform.position;
    ballBaseScale = mainBall.transform.localScale;
    countDown = FindFirstObjectByType<CountDown>();
    enemyBar = FindFirstObjectByType<EnemyBarManager>();
    player = FindFirstObjectByType<PlayerManager>();
    player.gameObject.AddComponent<PaddleGun>();
    playerBaseScale = player.transform.localScale;
    sceneChanger = FindSceneChanger();
    defenseLineY = player.transform.position.y + DefenseLineAbovePaddle;
    defenseLine = CreateDefenseLine(defenseLineY);
    BallManager.IsFeverActive = false;
    var plane = GameObject.Find("Plane");
    if (plane != null)
    {
      fieldBackground = plane.GetComponent<Renderer>().material;
      fieldBackgroundColor = fieldBackground.color;
    }
    FieldFloor.CreateOnField();
    FieldFrame.Create();

    hud = new StageHud(FindFirstObjectByType<OptionButtonPosition>().GetComponent<RectTransform>());
    modalCanvas = PixelUI.CreateOverlayCanvas("ModalCanvas", ModalSortingOrder);
    combo = new ComboMeter(OnComboChanged, OnFeverStart, OnFeverEnd);
    hud.SetCoins(0);
    hud.SetXp(RunState.Level, RunState.Xp, RunState.XpToNextLevel);
    hud.SetOwnedUpgrades(RunState.OwnedUpgrades);
    StartFloor(0);
    hud.ShowIntro("STAGE " + RunState.Stage, IntroMessage());
    // ステージの最初に宝箱を1つ開けて、すぐにビルドづくりを始められるようにする（カウントダウンは開けるまで止まる）
    StartCoroutine(OpenChest());
  }

  // このステージで初めて出てくるもの（敵の種類や、新しく選べるようになった強化）
  static string IntroMessage()
  {
    string message = StageDatabase.GetIntro(RunState.Stage);
    bool isFirstVisit = RunState.Stage == Progress.HighestCleared + 1;
    var unlock = Upgrades.NextUnlock(RunState.Stage - 1);
    if (isFirstVisit && unlock != null)
    {
      string unlockText = "NEW 強化「" + unlock.Name + "」が出るように!";
      message = message == null ? unlockText : message + "\n" + unlockText;
    }
    return message;
  }

  IEnumerator OpenChest()
  {
    isPanelOpen = true;
    Time.timeScale = 0f;
    bool isReceived = false;
    ChestPanel.Open(modalCanvas.transform, Upgrades.RollChest(random), upgrade =>
    {
      ApplyUpgrade(upgrade);
      isReceived = true;
    });
    while (!isReceived)
    {
      yield return null;
    }
    Time.timeScale = 1f;
    isPanelOpen = false;
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
    paddleHits = 0;
    ClearField();
    SpawnMonsters();
    SpawnLives();
    SetUpEnemyBar();
    SetUpPaddle();
    SetUpBalls();
    if (RunState.ShieldCharges > 0)
    {
      shield = Shield.Create(RunState.ShieldCharges);
    }
    hud.SetStage(RunState.Stage, floorIndex, floorCount);
    defenseLine.enabled = !floor.IsBossFloor;
    if (floor.IsBossFloor)
    {
      // 画面の縦横比が変わっても同じ並びになるよう、敵の親オブジェクトの位置を基準にする
      boss = Boss.Create(floor, monstersRoot.position.y + BossBottomOffsetY);
      hud.ShowBossHp(boss.Hp, boss.MaxHp);
      SoundEffects.Play(Sfx.BossAppear);
      HitEffects.Shake(3, 0.6f);
    }
    else
    {
      hud.HideBossHp();
    }
    if (floor.IsGoldenRush)
    {
      hud.ShowIntro("GOLDEN RUSH!", "金のスライムを たおしまくれ!");
      HitEffects.Flash(new Color(1f, 0.85f, 0.3f, 0.5f));
      SoundEffects.Play(Sfx.Coin);
    }
    countDown.Restart(index == 0 ? FirstFloorCountdown : LaterFloorCountdown);
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
    foreach (var gem in FindObjectsByType<XpGem>(FindObjectsSortMode.None))
    {
      Destroy(gem.gameObject);
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
    ClearMiniBalls();
  }

  void ClearMiniBalls()
  {
    foreach (var ball in miniBalls)
    {
      if (ball != null)
      {
        Destroy(ball.gameObject);
      }
    }
    miniBalls.Clear();
  }

  void SpawnMonsters()
  {
    bool canBeGolden = RunState.Stage >= GoldenStartStage;
    foreach (var spawn in floor.Monsters)
    {
      // 画面の縦横比が変わっても同じ並びになるよう、敵の親オブジェクトの位置を基準にする
      var monster = Instantiate(enemySlimePrefab, monstersRoot);
      monster.transform.position = new Vector3(spawn.Offset.x, monstersRoot.position.y + spawn.Offset.y, 0f);
      var enemy = monster.GetComponent<EnemyManager>();
      var configured = spawn;
      configured.IsGolden = spawn.IsGolden || (canBeGolden && Random.value < GoldenChance);
      enemy.ConfigureMonster(configured);
      enemy.Killed = OnMonsterDefeated;
      enemy.ReachedDefenseLine = OnMonsterReachedDefenseLine;
    }
  }

  // 味方スライム（残りライフ）を横に並べる
  void SpawnLives()
  {
    foreach (Transform child in livesRoot)
    {
      Destroy(child.gameObject);
    }
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
    ApplyPaddleWidth();
    Vector3 position = player.transform.position;
    position.x = 0f;
    player.transform.position = position;
  }

  void ApplyPaddleWidth()
  {
    float scale = RunState.PaddleWidthScale;
    player.transform.localScale = new Vector3(playerBaseScale.x * scale, playerBaseScale.y, playerBaseScale.z);
    player.LimitX = Mathf.Max(0f, FieldHalfWidth - PaddleHalfWidth * scale);
  }

  void SetUpBalls()
  {
    ConfigureBall(mainBall, ballStartPosition);
    for (int i = 1; i < RunState.BallCount; i++)
    {
      AddExtraBall(ExtraBallStartPosition(i));
    }
  }

  // 本物のボールの左右に、交互に並べる
  Vector3 ExtraBallStartPosition(int index)
  {
    float side = index % 2 == 1 ? -1f : 1f;
    return ballStartPosition + new Vector3(side * ExtraBallOffsetX * ((index + 1) / 2), 0f, 0f);
  }

  BallManager AddExtraBall(Vector3 position)
  {
    var ball = Instantiate(mainBall.gameObject, position, Quaternion.identity).GetComponent<BallManager>();
    ball.name = "ExtraBall";
    ConfigureBall(ball, position);
    ball.CanHurtLives = false;
    extraBalls.Add(ball);
    extraBallStarts.Add(position);
    return ball;
  }

  void ConfigureBall(BallManager ball, Vector3 position)
  {
    ball.Stop();
    ball.transform.position = position;
    ball.transform.localScale = ballBaseScale * RunState.BallSizeScale;
    ball.maxSpeed = floor.BallMaxSpeed;
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

  // ボールが2つ以上あるときだけ、本物のボールに輪を付けて追いかけさせる（ハードで消えている間は輪も消す）
  void LateUpdate()
  {
    if (mainBallRing == null)
    {
      return;
    }
    bool hasOtherBalls = extraBalls.Count > 0 || miniBalls.Count > 0;
    mainBallRing.enabled = hasOtherBalls && mainBallSprite.color.a >= 1f;
    mainBallRing.transform.position = PixelArt.Snap(mainBall.transform.position);
  }

  void Update()
  {
    if (phase == Phase.Countdown && countDown.IsFinished)
    {
      LaunchBalls();
      hud.HideIntro();
      phase = Phase.Playing;
    }
    if (combo != null && phase == Phase.Playing)
    {
      combo.Tick(Time.deltaTime);
    }
    if (hud != null)
    {
      hud.Tick(Time.unscaledDeltaTime);
    }
    miniBalls.RemoveAll(ball => ball == null);
    // ゴールデンラッシュ中は止めずに倒しまくってもらい、たまったレベルアップは階層のあとにまとめて選ぶ
    if (phase == Phase.Playing && pendingLevelUps > 0 && !isPanelOpen && !floor.IsGoldenRush)
    {
      StartCoroutine(ShowLevelUpsDuringPlay());
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
    return AliveMonsterCount() == 0;
  }

  // 残りが少ないときは、ボールを近くのモンスターへ少し引き寄せる（最後の1体に当たらずに長引くのを防ぐ）
  const int HomingMonsterCount = 3;

  public Transform FindHomingTarget(Vector3 ballPosition)
  {
    if (phase != Phase.Playing || floor.IsBossFloor)
    {
      return null;
    }
    Transform nearest = null;
    int alive = 0;
    float nearestDistance = float.MaxValue;
    foreach (var monster in monstersRoot.GetComponentsInChildren<EnemyManager>())
    {
      if (monster.IsDead) continue;
      alive++;
      float distance = Vector3.Distance(monster.transform.position, ballPosition);
      if (distance < nearestDistance)
      {
        nearestDistance = distance;
        nearest = monster.transform;
      }
    }
    return alive <= HomingMonsterCount ? nearest : null;
  }

  int AliveMonsterCount()
  {
    int count = 0;
    foreach (var monster in monstersRoot.GetComponentsInChildren<EnemyManager>())
    {
      if (!monster.IsDead) count++;
    }
    return count;
  }

  public void OnPaddleHit(BallManager ball, Paddle paddle, bool isSmash, Vector3 position)
  {
    if (paddle.Side != PaddleSide.Bottom || phase != Phase.Playing)
    {
      return;
    }
    HitEffects.Burst(position, PixelArt.LightBlue);
    if (isSmash)
    {
      hud.Popup(ball.IsMeteor ? "METEOR!!" : "SMASH!", position, ball.IsMeteor ? PixelArt.Orange : PixelArt.Yellow);
      HitEffects.BigBurst(position, PixelArt.Yellow, KillBurstPieces);
      HitEffects.Freeze(SmashFreezeSeconds);
      HitEffects.Shake(2, 0.15f);
    }
    // 進化「ボールのあらし」：本物のボールを打ち返すたびにミニボールが出る
    if (ball == mainBall && RunState.Has(UpgradeId.BallStorm))
    {
      for (int i = 0; i < BallStormMinis; i++)
      {
        SpawnMiniBall(position + Vector3.up * 0.3f);
      }
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
    SoundEffects.Play(Sfx.Advance);
    HitEffects.Shake(1, 0.15f);
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

  // ボールがモンスターに当たったときのダメージ。属性・いなずま・ぶんれつの効果もここで出す
  public HitResult OnBallHitMonster(BallManager ball, EnemyManager monster)
  {
    var hit = RollBallDamage(ball);
    if (ball.LoadedElement != Element.None)
    {
      TriggerElement(ball.LoadedElement, monster);
      ball.LoadedElement = Element.None;
    }
    // 倒せる一撃なら、そのモンスターの属性をボールがまとう
    if (hit.Damage >= monster.Hp && monster.Element != Element.None)
    {
      ball.LoadedElement = monster.Element;
      hud.Popup(Elements.NameOf(monster.Element) + "!", monster.transform.position, Elements.ColorOf(monster.Element));
    }
    if (RunState.ChainTargets > 0)
    {
      ChainLightning(monster, RunState.ChainTargets, 1 + RunState.GetUpgradeLevel(UpgradeId.Chain) / 3);
    }
    if (!ball.IsMini && Random.value < RunState.SplitChance)
    {
      SpawnMiniBall(monster.transform.position);
    }
    return hit;
  }

  // ボールがボスに当たったときのダメージ（属性をまとっていれば +1）
  public int OnBallHitBoss(BallManager ball)
  {
    var hit = RollBallDamage(ball);
    int damage = hit.Damage;
    if (ball.LoadedElement != Element.None)
    {
      damage++;
      ball.LoadedElement = Element.None;
    }
    return damage;
  }

  public void OnBossDamaged()
  {
    // ボスのHPは当たった後に更新されるので、次のフレームで表示を合わせる
    StartCoroutine(RefreshBossHpNextFrame());
  }

  // かいしんは3倍。メテオ中は3倍、フィーバー中は+1。ミニボールは弱め
  HitResult RollBallDamage(BallManager ball)
  {
    int damage = RunState.BallDamage + ball.ConsumeSmashBonus();
    if (ball.IsMini)
    {
      damage = Mathf.Max(1, RunState.BallDamage / 2);
    }
    if (ball.IsMeteor)
    {
      damage *= MeteorMultiplier;
    }
    if (IsFever)
    {
      damage += FeverBonusDamage;
    }
    bool isCritical = !ball.IsMini && Random.value < RunState.CriticalChance;
    if (isCritical)
    {
      damage *= CriticalMultiplier;
      hud.Popup("CRITICAL!", ball.transform.position, PixelArt.Yellow);
      SoundEffects.Play(Sfx.Crit);
      HitEffects.Freeze(CriticalFreezeSeconds);
    }
    return new HitResult { Damage = damage, IsCritical = isCritical };
  }

  IEnumerator RefreshBossHpNextFrame()
  {
    yield return null;
    if (boss != null)
    {
      hud.ShowBossHp(boss.Hp, boss.MaxHp);
    }
  }

  void TriggerElement(Element element, EnemyManager hitMonster)
  {
    Vector3 center = hitMonster.transform.position;
    var others = OtherMonsters(hitMonster);
    switch (element)
    {
      case Element.Fire:
        // ほのお：まわりのモンスターにも燃え広がる
        SoundEffects.Play(Sfx.Fire);
        HitEffects.BigBurst(center, Elements.ColorOf(element), KillBurstPieces);
        foreach (var monster in others)
        {
          if (Vector3.Distance(monster.transform.position, center) <= FireRadius)
          {
            monster.TakeDamage(FireDamage, false);
          }
        }
        break;
      case Element.Thunder:
        // かみなり：一番近いモンスターに連鎖する
        ChainLightning(hitMonster, 1, ThunderDamage);
        break;
      case Element.Ice:
        // こおり：敵のバーを少しの間こおらせる
        if (enemyBar.gameObject.activeSelf)
        {
          SoundEffects.Play(Sfx.Ice);
          enemyBar.Freeze(IceFreezeSeconds);
          HitEffects.Burst(enemyBar.transform.position, Elements.ColorOf(element));
        }
        break;
    }
    hud.Popup(Elements.NameOf(element) + "!", center, Elements.ColorOf(element));
  }

  List<EnemyManager> OtherMonsters(EnemyManager except)
  {
    var others = new List<EnemyManager>();
    foreach (var monster in monstersRoot.GetComponentsInChildren<EnemyManager>())
    {
      if (monster != except && !monster.IsDead)
      {
        others.Add(monster);
      }
    }
    return others;
  }

  // 近いモンスターから順に targets 体へ電撃をとばす
  public void ChainLightning(EnemyManager source, int targets, int damage)
  {
    var others = OtherMonsters(source);
    Vector3 from = source.transform.position;
    others.Sort((a, b) => Vector3.Distance(a.transform.position, from).CompareTo(Vector3.Distance(b.transform.position, from)));
    if (others.Count > 0)
    {
      SoundEffects.Play(Sfx.Thunder);
    }
    for (int i = 0; i < targets && i < others.Count; i++)
    {
      HitEffects.Line(from, others[i].transform.position, Elements.ColorOf(Element.Thunder));
      from = others[i].transform.position;
      others[i].TakeDamage(damage, false);
    }
  }

  // ぶんれつ：上向きに飛ぶミニボールを出す（仲間を傷つけず、しばらくすると消える）
  void SpawnMiniBall(Vector3 position)
  {
    if (miniBalls.Count >= MaxMiniBalls || phase != Phase.Playing)
    {
      return;
    }
    var ball = Instantiate(mainBall.gameObject, position, Quaternion.identity).GetComponent<BallManager>();
    ball.name = "MiniBall";
    ConfigureBall(ball, position);
    ball.MakeMini();
    float angle = Random.Range(40f, 140f) * Mathf.Deg2Rad;
    ball.Launch(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * MiniBallSpeed);
    miniBalls.Add(ball);
  }

  void OnMonsterDefeated(EnemyManager killed)
  {
    Vector3 position = killed.transform.position;
    RunState.CountKill();
    combo.AddKill();
    SoundEffects.PlayRaised(Sfx.MonsterBreak, Mathf.Min(combo.Count, MaxComboSemitones));
    HitEffects.BigBurst(position, killed.BurstColor, KillBurstPieces);
    int gems = Mathf.Min(killed.MaxHp, MaxGemsPerMonster);
    if (killed.IsGolden)
    {
      gems += floor.IsGoldenRush ? GoldenRushBonusGems : GoldenBonusGems;
      int coins = RunState.AddCoins(floor.IsGoldenRush ? GoldenRushBonusCoins : GoldenBonusCoins);
      hud.SetCoins(RunState.CoinsEarned);
      hud.Popup("GOLD! +" + coins, position, PixelArt.Yellow);
      SoundEffects.Play(Sfx.Coin);
      HitEffects.Shake(2, 0.2f);
    }
    for (int i = 0; i < gems; i++)
    {
      XpGem.Spawn(position, player.transform, OnGemCollected);
    }
    bool isExplosionKill = explosionDepth > 0;
    if (killed.IsBomb)
    {
      // 爆弾は何段でも連鎖する（爆弾が並んでいると一気に吹き飛ぶ）
      StartCoroutine(Explode(position, explosionDepth + 1, BombRadius, BombDamage));
    }
    else if (RunState.Has(UpgradeId.Explosion) && (!isExplosionKill || (RunState.Has(UpgradeId.BigBang) && explosionDepth < MaxExplosionDepth)))
    {
      StartCoroutine(Explode(position, explosionDepth + 1, RunState.ExplosionRadius, RunState.ExplosionDamage));
    }
    // 最後の1体はスローにして、クリアの瞬間を気持ちよく見せる
    if (!floor.IsBossFloor && AliveMonsterCount() == 0)
    {
      HitEffects.SlowMotion(LastKillSlowScale, LastKillSlowSeconds);
      HitEffects.Shake(3, 0.3f);
    }
  }

  IEnumerator Explode(Vector3 center, int depth, float radius, int damage)
  {
    yield return depth > 1 ? new WaitForSeconds(ExplosionChainDelay) : null;
    SoundEffects.Play(Sfx.Explosion);
    HitEffects.Ring(center, radius, PixelArt.Orange);
    HitEffects.BigBurst(center, PixelArt.Red, KillBurstPieces / 2);
    HitEffects.Shake(RunState.Has(UpgradeId.BigBang) ? 3 : 1, 0.15f);
    int previousDepth = explosionDepth;
    explosionDepth = depth;
    foreach (var monster in monstersRoot.GetComponentsInChildren<EnemyManager>())
    {
      if (!monster.IsDead && Vector3.Distance(monster.transform.position, center) <= radius)
      {
        monster.TakeDamage(damage, false);
      }
    }
    if (boss != null && Vector3.Distance(boss.transform.position, center) <= radius + ExplosionBossRange)
    {
      boss.TakeDamage(damage);
    }
    explosionDepth = previousDepth;
  }

  void OnGemCollected()
  {
    gemStreak++;
    SoundEffects.PlayRaised(Sfx.Gem, gemStreak % GemPitchCycle);
    int levelUps = RunState.AddXp(1);
    hud.SetXp(RunState.Level, RunState.Xp, RunState.XpToNextLevel);
    if (levelUps > 0)
    {
      pendingLevelUps += levelUps;
      gemStreak = 0;
    }
  }

  void OnComboChanged(int count)
  {
    RunState.RecordCombo(count);
    hud.SetCombo(count, IsFever);
    int milestone = System.Array.IndexOf(ComboMilestones, count);
    if (milestone >= 0)
    {
      hud.Popup(ComboWords[milestone], new Vector3(0f, 1.6f, 0f), PixelArt.Orange, PixelUI.LargeFont);
      SoundEffects.Play(Sfx.UpgradePick);
      HitEffects.Shake(2, 0.2f);
    }
  }

  // フィーバー：しばらくボールが燃えてダメージ+1、画面も熱くなる
  void OnFeverStart()
  {
    BallManager.IsFeverActive = true;
    SoundEffects.Play(Sfx.Fever);
    HitEffects.Flash(new Color(1f, 0.6f, 0.2f, 0.5f));
    HitEffects.Shake(3, 0.4f);
    hud.Popup("FEVER!!", new Vector3(0f, 0.2f, 0f), PixelArt.Orange, PixelUI.HugeFont);
    hud.SetCombo(combo.Count, true);
    if (fieldBackground != null)
    {
      StartCoroutine(PulseBackgroundDuringFever());
    }
  }

  IEnumerator PulseBackgroundDuringFever()
  {
    bool isRed = false;
    while (combo.IsFever)
    {
      isRed = !isRed;
      fieldBackground.color = isRed ? FeverBackgroundColor : fieldBackgroundColor;
      yield return new WaitForSeconds(FeverPulseSeconds);
    }
    fieldBackground.color = fieldBackgroundColor;
  }

  void OnFeverEnd()
  {
    BallManager.IsFeverActive = false;
    hud.SetCombo(combo.Count, false);
  }

  public void OnBossDefeated(Vector3 position)
  {
    hud.HideBossHp();
    for (int i = 0; i < BossBurstCount; i++)
    {
      HitEffects.BigBurst(position + (Vector3)(Random.insideUnitCircle * 0.8f), i % 2 == 0 ? PixelArt.Yellow : PixelArt.LightBlue, KillBurstPieces);
    }
    HitEffects.Shake(4, 0.6f);
    HitEffects.Flash(new Color(1f, 1f, 1f, 0.6f));
    HitEffects.SlowMotion(LastKillSlowScale, LastKillSlowSeconds);
    SoundEffects.Play(Sfx.Explosion);
    hud.Popup("BOSS DOWN!", position, PixelArt.Yellow, PixelUI.LargeFont);
    for (int i = 0; i < floor.BossHp / 4; i++)
    {
      XpGem.Spawn(position, player.transform, OnGemCollected);
    }
  }

  public void OnLifeLost(EnemyManager life)
  {
    if (phase != Phase.Playing)
    {
      return;
    }
    Vector3 position = life.transform.position;
    combo.Break();
    RunState.LoseLife();
    SoundEffects.Play(Sfx.SlimeLost);
    HitEffects.Burst(position, PixelArt.Red);
    HitEffects.Shake();
    if (OptionManager.isSePlaying)
    {
      HitEffects.Vibrate();
    }
    if (RunState.Lives <= 0 && RunState.TryRevive())
    {
      hud.Popup("ふっかつ!", position, PixelArt.Green);
      SoundEffects.Play(Sfx.Revive);
      StartCoroutine(RespawnLivesNextFrame());
    }
    else if (RunState.Lives <= 0)
    {
      if (!hasUsedAdRevive && RewardedAds.IsReady)
      {
        OfferAdRevive();
        return;
      }
      EndStage(false);
      return;
    }
    // ボールに抜かれてやられたときは、ホッケーの得点のようにボールを戻して打ち直す
    if (life.IsKilledByBall)
    {
      StartCoroutine(Serve());
    }
  }

  // 全滅したとき、広告を見れば1匹で続けられるか聞く（聞いている間は時間を止める）
  void OfferAdRevive()
  {
    phase = Phase.ReviveOffer;
    StopBalls();
    Time.timeScale = 0f;
    revivePanel = new RevivePanel(modalCanvas.transform, WatchAdToRevive, GiveUpRevive);
  }

  void WatchAdToRevive()
  {
    revivePanel.Close();
    revivePanel = null;
    hasUsedAdRevive = true;
    RewardedAds.Show(ReviveAfterAd, GiveUpRevive);
  }

  void ReviveAfterAd()
  {
    Time.timeScale = 1f;
    RunState.ReviveByAd();
    hud.Popup("ふっかつ!", player.transform.position + Vector3.up, PixelArt.Green);
    SoundEffects.Play(Sfx.Revive);
    StartCoroutine(RespawnLivesNextFrame());
    StartCoroutine(Serve());
  }

  void GiveUpRevive()
  {
    revivePanel?.Close();
    revivePanel = null;
    Time.timeScale = 1f;
    EndStage(false);
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

  // プレイ中のレベルアップ：時間を止めて3択を出す。たまっている分は続けて選ぶ
  IEnumerator ShowLevelUpsDuringPlay()
  {
    isPanelOpen = true;
    phase = Phase.Choosing;
    HitEffects.Flash(new Color(0.4f, 1f, 0.5f, 0.45f));
    SoundEffects.Play(Sfx.LevelUp);
    yield return ShowPendingLevelUps();
    isPanelOpen = false;
    if (phase == Phase.Choosing)
    {
      phase = Phase.Playing;
    }
  }

  IEnumerator ShowPendingLevelUps()
  {
    while (pendingLevelUps > 0)
    {
      bool isPicked = false;
      Time.timeScale = 0f;
      ShowChoices(() => isPicked = true);
      while (!isPicked)
      {
        yield return null;
      }
      pendingLevelUps--;
    }
    Time.timeScale = 1f;
  }

  void ShowChoices(System.Action onPicked)
  {
    var choices = Upgrades.Roll(random);
    string subtitle = "Lv" + (RunState.Level - pendingLevelUps + 1) + "  つよさを1つえらぼう";
    choicePanel = new UpgradeChoicePanel(modalCanvas.transform, "LEVEL UP!", subtitle, choices, RunState.RerollsLeft, upgrade =>
    {
      choicePanel.Close();
      ApplyUpgrade(upgrade);
      onPicked();
    }, () =>
    {
      if (RunState.TryUseReroll())
      {
        choicePanel.Close();
        ShowChoices(onPicked);
      }
    });
  }

  // 強化をすぐ画面に反映する（取った瞬間に強くなったと分かるように）
  void ApplyUpgrade(UpgradeDefinition upgrade)
  {
    RunState.AddUpgrade(upgrade.Id);
    hud.SetOwnedUpgrades(RunState.OwnedUpgrades);
    if (upgrade.IsEvolution)
    {
      SoundEffects.Play(Sfx.Evolve);
      HitEffects.Flash(new Color(1f, 0.85f, 0.3f, 0.6f));
      HitEffects.Shake(3, 0.4f);
      hud.Popup("進化! " + upgrade.Name, new Vector3(0f, 0.8f, 0f), PixelArt.Yellow, PixelUI.LargeFont);
    }
    else
    {
      SoundEffects.Play(Sfx.UpgradePick);
    }
    switch (upgrade.Id)
    {
      case UpgradeId.MultiBall:
        // カウントダウン中なら打ち出す前の列に並べ、プレイ中ならバーの上からすぐ飛ばす
        if (phase == Phase.Countdown)
        {
          AddExtraBall(ExtraBallStartPosition(extraBalls.Count + 1));
        }
        else
        {
          AddExtraBall(player.transform.position + Vector3.up * 0.5f).Launch(new Vector2(Random.Range(-0.6f, 0.6f), 1f));
        }
        break;
      case UpgradeId.WidePaddle:
      case UpgradeId.GigaPaddle:
        ApplyPaddleWidth();
        break;
      case UpgradeId.BigBall:
        mainBall.transform.localScale = ballBaseScale * RunState.BallSizeScale;
        foreach (var extra in extraBalls)
        {
          extra.transform.localScale = ballBaseScale * RunState.BallSizeScale;
        }
        break;
      case UpgradeId.Shield:
        if (shield != null)
        {
          shield.AddCharge();
        }
        else
        {
          shield = Shield.Create(1);
        }
        break;
      case UpgradeId.Heal:
        SpawnLives();
        break;
    }
  }

  void OnFloorCleared()
  {
    phase = Phase.Choosing;
    StopBalls();
    ClearMiniBalls();
    combo.EndFever();
    foreach (var bullet in FindObjectsByType<BossBullet>(FindObjectsSortMode.None))
    {
      Destroy(bullet.gameObject);
    }
    int coins = RunState.AddCoins(StageDatabase.GetFloorReward(RunState.Stage));
    hud.SetCoins(RunState.CoinsEarned);
    hud.Popup("+" + coins + " COIN", player.transform.position + Vector3.up, PixelArt.Yellow);
    hud.Popup("CLEAR!", new Vector3(0f, 0.8f, 0f), PixelArt.Yellow, PixelUI.HugeFont);
    SoundEffects.Play(Sfx.FloorClear);

    bool isLastFloor = floorIndex + 1 >= floorCount;
    if (isLastFloor)
    {
      RunState.AddCoins(StageDatabase.GetClearBonus(RunState.Stage));
      hud.SetCoins(RunState.CoinsEarned);
    }
    StartCoroutine(BetweenFloors(isLastFloor));
  }

  // 残っているジェムを集めきり、たまったレベルアップと宝箱を順に出してから次の階層へ
  IEnumerator BetweenFloors(bool isLastFloor)
  {
    yield return new WaitForSecondsRealtime(FloorEndWaitSeconds);
    XpGem.AttractAllNow();
    float waited = 0f;
    while (FindFirstObjectByType<XpGem>() != null && waited < GemCollectWaitSeconds)
    {
      waited += Time.unscaledDeltaTime;
      yield return null;
    }
    if (isLastFloor)
    {
      EndStage(true);
      yield break;
    }
    isPanelOpen = true;
    if (pendingLevelUps > 0)
    {
      SoundEffects.Play(Sfx.LevelUp);
      yield return ShowPendingLevelUps();
    }
    yield return OpenChest();
    StartFloor(floorIndex + 1);
  }

  void EndStage(bool isClear)
  {
    phase = Phase.Ended;
    StopBalls();
    ClearMiniBalls();
    combo.EndFever();
    int stars = isClear ? StarsForLives() : 0;
    var nextUnlock = isClear && RunState.Stage > Progress.HighestCleared ? Upgrades.NextUnlock(RunState.Stage) : null;
    if (isClear)
    {
      Progress.MarkCleared(RunState.Stage, stars);
      SoundEffects.Play(Sfx.StageClear);
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
      Level = RunState.Level,
      Kills = RunState.Kills,
      BestCombo = RunState.BestCombo,
      Evolutions = RunState.Evolutions,
      MonstersLeft = floor.IsBossFloor ? 0 : AliveMonsterCount(),
      NextUnlock = nextUnlock,
    };
    bool canDoubleCoins = RunState.CoinsEarned > 0 && RewardedAds.IsReady;
    resultPanel = new StageResultPanel(modalCanvas.transform, result, GoToNextStage, RetryStage, GoToMap, canDoubleCoins ? WatchAdToDoubleCoins : null);
  }

  // 広告を見たら、持ち帰るコインをもう1回分もらえる
  void WatchAdToDoubleCoins()
  {
    resultPanel.HideDoubleCoinsButton();
    RewardedAds.Show(() =>
    {
      Progress.AddCoins(RunState.CoinsEarned);
      resultPanel.ShowDoubledCoins(RunState.CoinsEarned * 2);
    }, () => { });
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
