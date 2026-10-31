using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 対戦モード：上下のバーで打ち合い、相手の仲間スライムを全部倒したほうが勝ち
public class VersusController : MonoBehaviour
{
  public static VersusController Instance { get; private set; }

  public GameObject playerSlimePrefab;

  enum Phase { Setup, Countdown, Playing, Serving, Ended }

  const int LivesPerSide = 3;
  const float LifeSpacingX = 0.95f;
  const float PaddleLimitX = 1.8f;
  const float BallMinSpeed = 5.5f;
  const float BallMaxSpeed = 11f;
  const int LabelSortingOrder = 0;
  const int ModalSortingOrder = 100;
  const float LabelOffsetY = 160f;
  // 仲間がやられてから、ボールを打ち直すまでの間
  const float ServeDelaySeconds = 1f;

  Phase phase;
  BallManager ball;
  Vector3 ballStartPosition;
  CountDown countDown;
  Transform bottomLivesRoot;
  Transform topLivesRoot;
  int bottomLives;
  int topLives;
  Canvas labelCanvas;
  Canvas modalCanvas;
  OptionManager sceneChanger;

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

  // 各オブジェクトの位置が画面に合わせて決まる Start の後に始める
  IEnumerator Start()
  {
    yield return null;
    ball = GameObject.Find("Ball").GetComponent<BallManager>();
    countDown = FindFirstObjectByType<CountDown>();
    bottomLivesRoot = GameObject.Find("PlayerSlimes").transform;
    topLivesRoot = GameObject.Find("EnemySlimes").transform;
    sceneChanger = FindSceneChanger();
    labelCanvas = PixelUI.CreateOverlayCanvas("VersusLabels", LabelSortingOrder);
    modalCanvas = PixelUI.CreateOverlayCanvas("ModalCanvas", ModalSortingOrder);
    FieldFloor.CreateOnField();
    FieldFrame.Create();

    // 上下の壁の内側の真ん中を基準に、上側を下側と線対称に並べる
    float topInner = GameObject.Find("wallTop").GetComponent<Collider2D>().bounds.min.y;
    float bottomInner = GameObject.Find("wallBottom").GetComponent<Collider2D>().bounds.max.y;
    float centerY = (topInner + bottomInner) / 2f;

    var bottomPlayer = FindFirstObjectByType<PlayerManager>();
    SetUpBottom(bottomPlayer);
    var topBar = FindFirstObjectByType<EnemyBarManager>();
    topBar.transform.position = new Vector3(0f, centerY * 2f - bottomPlayer.transform.position.y, 0f);
    SetUpTop(topBar);

    topLivesRoot.position = new Vector3(0f, centerY * 2f - bottomLivesRoot.position.y, 0f);
    bottomLives = SpawnLives(bottomLivesRoot, PaddleSide.Bottom);
    topLives = SpawnLives(topLivesRoot, PaddleSide.Top);

    ballStartPosition = new Vector3(0f, centerY, 0f);
    ball.transform.position = ballStartPosition;
    ball.Stop();
    ball.minSpeed = BallMinSpeed;
    ball.maxSpeed = BallMaxSpeed;
    ball.speed = BallMinSpeed / Mathf.Sqrt(2f);

    CreateLabel(Characters.Get(VersusSettings.Player1).Name + "  (P1)", false);
    string topName = Characters.Get(VersusSettings.Player2).Name + (VersusSettings.IsPlayer2Cpu ? "  (CPU)" : "  (P2)");
    CreateLabel(topName, true);

    countDown.Restart();
    phase = Phase.Countdown;
  }

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

  void SetUpBottom(PlayerManager player)
  {
    var paddle = player.GetComponent<Paddle>();
    paddle.Side = PaddleSide.Bottom;
    paddle.Character = VersusSettings.Player1;
    player.LimitX = PaddleLimitX;
    // 2人で遊ぶときは、画面の下半分の指だけで動かす
    player.UsesHalfScreen = !VersusSettings.IsPlayer2Cpu;
  }

  void SetUpTop(EnemyBarManager topBar)
  {
    var paddle = topBar.GetComponent<Paddle>();
    paddle.Side = PaddleSide.Top;
    paddle.Character = VersusSettings.Player2;
    if (VersusSettings.IsPlayer2Cpu)
    {
      topBar.Configure(Characters.Get(VersusSettings.Player2).CpuStyle, VersusSettings.CpuLevel, PaddleLimitX);
      return;
    }
    // 2人対戦：上のバーは画面の上半分の指で動かす
    topBar.enabled = false;
    var topPlayer = topBar.gameObject.AddComponent<PlayerManager>();
    topPlayer.IsTopPlayer = true;
    topPlayer.UsesHalfScreen = true;
    topPlayer.LimitX = PaddleLimitX;
  }

  int SpawnLives(Transform root, PaddleSide side)
  {
    for (int i = 0; i < LivesPerSide; i++)
    {
      var life = Instantiate(playerSlimePrefab, root);
      float x = (i - (LivesPerSide - 1) / 2f) * LifeSpacingX;
      life.transform.position = new Vector3(x, root.position.y, 0f);
      var slime = life.GetComponent<EnemyManager>();
      slime.ConfigureLife(side);
      slime.Killed = killed => OnLifeLost(side, killed.transform.position);
    }
    return LivesPerSide;
  }

  // キャラクター名を自分の側に出す（上の人には逆さまに出して、向かい合って読めるようにする）
  void CreateLabel(string message, bool isTop)
  {
    var label = PixelUI.CreateText(labelCanvas.transform, message, PixelUI.MediumFont, isTop ? PixelArt.Red : PixelArt.LightBlue);
    var anchor = new Vector2(0.5f, isTop ? 1f : 0f);
    PixelUI.Place(label.rectTransform, anchor, new Vector2(0f, isTop ? -LabelOffsetY - 330f : LabelOffsetY), new Vector2(900f, 80f));
    if (isTop && !VersusSettings.IsPlayer2Cpu)
    {
      label.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
    }
  }

  void Update()
  {
    if (phase == Phase.Countdown && countDown.IsFinished)
    {
      // どちらに打ち出すかはランダム
      ball.Launch(new Vector2(Random.value < 0.5f ? -1f : 1f, Random.value < 0.5f ? -1f : 1f));
      phase = Phase.Playing;
      // キャラ名は仲間スライムと重なるので、試合が始まったら消す
      labelCanvas.gameObject.SetActive(false);
    }
  }

  public void OnPaddleHit(Paddle paddle, bool isSmash, Vector3 position)
  {
    if (isSmash && phase == Phase.Playing)
    {
      HitEffects.Burst(position, PixelArt.Yellow);
    }
  }

  void OnLifeLost(PaddleSide side, Vector3 position)
  {
    if (phase != Phase.Playing)
    {
      return;
    }
    HitEffects.Burst(position, PixelArt.Red);
    HitEffects.Shake();
    if (side == PaddleSide.Bottom)
    {
      bottomLives--;
    }
    else
    {
      topLives--;
    }
    if (bottomLives <= 0 || topLives <= 0)
    {
      EndMatch(bottomLives > 0);
      return;
    }
    StartCoroutine(Serve(side));
  }

  // ホッケーの得点のように、ボールを真ん中に戻して、やられた側へ打ち出す
  IEnumerator Serve(PaddleSide loser)
  {
    phase = Phase.Serving;
    ball.Stop();
    ball.transform.position = ballStartPosition;
    yield return new WaitForSeconds(ServeDelaySeconds);
    if (phase == Phase.Serving)
    {
      ball.Launch(new Vector2(Random.value < 0.5f ? -1f : 1f, loser == PaddleSide.Bottom ? -1f : 1f));
      phase = Phase.Playing;
    }
  }

  void EndMatch(bool isBottomWinner)
  {
    phase = Phase.Ended;
    ball.Stop();
    string winner = isBottomWinner ? "P1 WIN!" : (VersusSettings.IsPlayer2Cpu ? "CPU WIN..." : "P2 WIN!");
    ShowResult(winner, isBottomWinner);
  }

  void ShowResult(string winner, bool isBottomWinner)
  {
    var root = PixelUI.Stretch(PixelUI.CreateRect("VersusResult", modalCanvas.transform));
    PixelUI.CreateDimmer(root, 0.7f);
    var panel = PixelUI.CreatePanel(root, PixelArt.Navy, isBottomWinner ? PixelArt.Yellow : PixelArt.Red);
    var center = new Vector2(0.5f, 0.5f);
    PixelUI.Place(panel.rectTransform, center, Vector2.zero, new Vector2(1100f, 800f));
    var title = PixelUI.CreateText(panel.transform, winner, PixelUI.HugeFont, isBottomWinner ? PixelArt.Yellow : PixelArt.Red);
    PixelUI.Place(title.rectTransform, center, new Vector2(0f, 200f), new Vector2(1100f, 140f));
    var rematch = PixelUI.CreateButton(panel.transform, "もういちど", PixelUI.MediumFont, PixelArt.Teal, () => LeaveTo("VersusPlay"));
    PixelUI.Place((RectTransform)rematch.transform, center, new Vector2(-230f, -170f), new Vector2(400f, 150f));
    var menu = PixelUI.CreateButton(panel.transform, "MENU", PixelUI.MediumFont, PixelArt.Teal, () => LeaveTo("Menu"));
    PixelUI.Place((RectTransform)menu.transform, center, new Vector2(230f, -170f), new Vector2(400f, 150f));
  }

  void LeaveTo(string sceneName)
  {
    // 広告が出たら、その閉じるボタンを結果画面の暗幕が隠さないよう結果画面を閉じる
    if (sceneChanger.GoToScene(sceneName))
    {
      modalCanvas.gameObject.SetActive(false);
    }
  }
}
