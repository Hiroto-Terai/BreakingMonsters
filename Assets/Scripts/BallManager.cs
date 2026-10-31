using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallManager : MonoBehaviour
{
  // ボールの移動の速さを指定する変数
  public float speed = 5f;
  // 速さの最小値を指定する変数を追加
  public float minSpeed = 5f;
  // 速さの最大値を指定する変数を追加
  public float maxSpeed = 10f;
  Rigidbody2D myRigidbody2d;
  // ボールの色を取得(透明化のため)
  SpriteRenderer spriteRenderer;

  // ハードステージでは一定時間ごとにボールが消えて、また現れる
  public bool IsFading { get; set; }
  const float VisibleSeconds = 5f;
  const float FadeOutSeconds = 1f;
  const float HiddenSeconds = 0.5f;
  const float FadeInSeconds = 0.3f;
  // 透明度をなめらかにせず段階的に変えて、ドットゲームらしい見た目にする
  const int FadeSteps = 4;
  float fadeCycleTime = 0f;

  // 水平に近すぎる角度(ラリーが長引く)や垂直に近すぎる角度(同じ所を往復する)を避ける
  const float MinAngleFromHorizontalDeg = 38f;
  const float MaxAngleFromHorizontalDeg = 80f;
  // バーで打ち返すたびに少しずつ速くする（自分と敵の両方で速くなるので、1回あたりは控えめ）
  const float SpeedUpPerPaddleHit = 1.02f;

  // 「振って打つ」：当たる瞬間のバーの速さで、カーブとスマッシュが出る
  // ボールを追いかけているだけの動きではカーブしないよう、ゆっくりした動きは無視する
  const float SpinDeadZoneSpeed = 3f;
  const float SpinPerPaddleSpeed = 0.6f;
  const float MaxSpin = 9f;
  const float SpinDecayPerSecond = 7f;
  const float SmashPaddleSpeed = 9f;
  const float SmashSpeedMultiplier = 1.35f;
  const float SmashMaxSpeedMultiplier = 1.3f;
  const float SmashSeconds = 1.2f;
  // キャラクターの技
  const float CurveCharacterSpinMultiplier = 2f;
  const float GhostHideStartSeconds = 0.25f;
  const float GhostHideEndSeconds = 0.9f;
  const float BigCharacterScale = 1.7f;

  // 残像（ドットの影を少し残す）
  const float AfterimageInterval = 0.04f;
  float afterimageTimer;

  float spin;
  float smashTimer;
  // ぶつかる直前の速度（ぶつかった後の処理では、物理演算で跳ね返った後の速度になっているため）
  Vector2 velocityBeforeCollision;
  float ghostTimer = -1f;
  Vector3 normalScale;
  bool isBig;

  // 次にモンスターへ当たったとき +1 ダメージになる
  public bool IsSmashCharged { get; private set; }
  // 強化で増えたボールやミニボールは仲間スライムを傷つけない
  public bool CanHurtLives { get; set; } = true;
  // ぶんれつで出たミニボール。しばらくすると消える
  public bool IsMini { get; private set; }
  const float MiniLifeSeconds = 6f;
  const float MiniBlinkSeconds = 1f;
  const float MiniScale = 0.6f;
  float miniAge;
  // つらぬき：打ち返してから、あと何体つらぬけるか
  int pierceLeft;
  // 倒して突き抜けるとき、ぶつかる前の速度に戻すため次の物理更新で使う
  bool shouldRestoreVelocity;
  // 進化「メテオ」：スマッシュしてからしばらく、ぜんぶ突き抜けてダメージ3倍
  const float MeteorSeconds = 2.5f;
  const float MeteorScale = 1.35f;
  float meteorTimer;
  public bool IsMeteor => meteorTimer > 0f;
  // フィーバー中はボールが燃えて見える
  public static bool IsFeverActive;
  static readonly Color SubBallTint = new Color(0.55f, 0.8f, 1f, 1f);
  // 倒したモンスターから受け取った属性
  public Element LoadedElement { get; set; }

  void Awake()
  {
    myRigidbody2d = GetComponent<Rigidbody2D>();
    this.spriteRenderer = GetComponent<SpriteRenderer>();
  }

  public bool IsMoving => myRigidbody2d.linearVelocity != Vector2.zero;

  // 指定した向きに、最初の速さで打ち出す
  public void Launch(Vector2 direction)
  {
    // 元の作りでは (speed, speed) で打ち出していたので、その速さにそろえる
    float launchSpeed = speed * Mathf.Sqrt(2f);
    myRigidbody2d.linearVelocity = ClampDirection(direction) * launchSpeed;
  }

  public void Stop()
  {
    myRigidbody2d.linearVelocity = Vector2.zero;
    spin = 0f;
    smashTimer = 0f;
    ghostTimer = -1f;
    IsSmashCharged = false;
    LoadedElement = Element.None;
    pierceLeft = 0;
    meteorTimer = 0f;
    shouldRestoreVelocity = false;
    EndBig();
  }

  // ぶんれつで出るミニボールにする（小さく、仲間を傷つけず、しばらくすると消える）
  public void MakeMini()
  {
    IsMini = true;
    CanHurtLives = false;
    transform.localScale *= MiniScale;
    miniAge = 0f;
  }

  // モンスターを倒した直後に呼ばれる。つらぬける間は、跳ね返らずにそのまま進む
  public void OnKilledMonster()
  {
    if (IsMeteor)
    {
      shouldRestoreVelocity = true;
    }
    else if (pierceLeft > 0)
    {
      pierceLeft--;
      shouldRestoreVelocity = true;
    }
  }

  // スマッシュの+1ダメージを使い切る
  public int ConsumeSmashBonus()
  {
    bool wasCharged = IsSmashCharged;
    IsSmashCharged = false;
    return wasCharged ? 1 : 0;
  }

  void FixedUpdate()
  {
    if (shouldRestoreVelocity)
    {
      myRigidbody2d.linearVelocity = velocityBeforeCollision;
      shouldRestoreVelocity = false;
    }
    // カーブ：横向きの力を少しずつ弱めながらかけ続ける
    if (spin != 0f && IsMoving)
    {
      myRigidbody2d.linearVelocity += Vector2.right * spin * Time.fixedDeltaTime;
      spin = Mathf.MoveTowards(spin, 0f, SpinDecayPerSecond * Time.fixedDeltaTime);
    }
    HomeTowardLastMonsters();
    velocityBeforeCollision = myRigidbody2d.linearVelocity;
  }

  // 敵側へ飛んでいるあいだ、残り少ないモンスターへ向けて少しずつ曲げる
  const float HomingDegreesPerSecond = 70f;

  void HomeTowardLastMonsters()
  {
    var controller = StageRunController.Instance;
    Vector2 velocity = myRigidbody2d.linearVelocity;
    if (controller == null || velocity.y <= 0f)
    {
      return;
    }
    var target = controller.FindHomingTarget(transform.position);
    if (target == null)
    {
      return;
    }
    Vector2 toTarget = (Vector2)(target.position - transform.position);
    float maxRadians = HomingDegreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime;
    Vector3 turned = Vector3.RotateTowards(velocity, toTarget.normalized * velocity.magnitude, maxRadians, 0f);
    myRigidbody2d.linearVelocity = turned;
  }

  // Update is called once per frame
  void Update()
  {
    smashTimer -= Time.deltaTime;
    if (meteorTimer > 0f)
    {
      meteorTimer -= Time.deltaTime;
      if (meteorTimer <= 0f)
      {
        EndBig();
      }
    }
    if (IsMini)
    {
      miniAge += Time.deltaTime;
      if (miniAge >= MiniLifeSeconds)
      {
        Destroy(gameObject);
        return;
      }
    }
    UpdateVisibility();

    // 速度チェック
    Vector2 velocity = myRigidbody2d.linearVelocity;
    if (velocity == Vector2.zero)
    {
      return;
    }
    // スマッシュ中は少しだけ速さの上限を上げる
    float currentMax = smashTimer > 0f ? maxSpeed * SmashMaxSpeedMultiplier : maxSpeed;
    float clampedSpeed = Mathf.Clamp(velocity.magnitude, minSpeed, currentMax);
    // 移動する方向を一定の角度の範囲に制限する
    myRigidbody2d.linearVelocity = ClampDirection(velocity) * clampedSpeed;

    afterimageTimer -= Time.deltaTime;
    if (afterimageTimer <= 0f && spriteRenderer.color.a > 0f)
    {
      afterimageTimer = AfterimageInterval;
      HitEffects.Afterimage(spriteRenderer, transform.position, TrailColor);
    }
  }

  // 残像の色で、ボールの状態（属性・スマッシュ）が分かるようにする
  Color TrailColor
  {
    get
    {
      if (IsMeteor || IsFeverActive) return PixelArt.Orange;
      if (LoadedElement != Element.None) return Elements.ColorOf(LoadedElement);
      if (smashTimer > 0f) return PixelArt.Yellow;
      return IsMini ? PixelArt.Pale : PixelArt.LightBlue;
    }
  }

  void UpdateVisibility()
  {
    float alpha = IsFading ? FadeAlpha() : 1f;
    // ステルス型キャラクターが打った球は、途中で見えなくなる
    if (ghostTimer >= 0f)
    {
      ghostTimer += Time.deltaTime;
      if (ghostTimer >= GhostHideStartSeconds && ghostTimer < GhostHideEndSeconds)
      {
        alpha = 0f;
      }
      else if (ghostTimer >= GhostHideEndSeconds)
      {
        ghostTimer = -1f;
      }
    }
    // ミニボールは消える前に点滅して知らせる
    if (IsMini && miniAge > MiniLifeSeconds - MiniBlinkSeconds && Mathf.FloorToInt(miniAge * 10f) % 2 == 0)
    {
      alpha *= 0.3f;
    }
    // 仲間を傷つけないボール（増えたボール・ミニボール）は水色にして、本物のボールと見分けられるようにする
    Color tint = CanHurtLives ? Color.white : SubBallTint;
    if (IsMeteor || IsFeverActive) tint = PixelArt.Orange;
    else if (LoadedElement != Element.None) tint = Elements.ColorOf(LoadedElement);
    spriteRenderer.color = new Color(tint.r, tint.g, tint.b, alpha);
  }

  float FadeAlpha()
  {
    fadeCycleTime += Time.deltaTime;
    float t = fadeCycleTime - VisibleSeconds;
    float alpha;
    if (t < 0f)
    {
      alpha = 1f;
    }
    else if (t < FadeOutSeconds)
    {
      alpha = 1f - t / FadeOutSeconds;
    }
    else if (t < FadeOutSeconds + HiddenSeconds)
    {
      alpha = 0f;
    }
    else if (t < FadeOutSeconds + HiddenSeconds + FadeInSeconds)
    {
      alpha = (t - FadeOutSeconds - HiddenSeconds) / FadeInSeconds;
    }
    else
    {
      alpha = 1f;
      fadeCycleTime = 0f;
    }
    return Mathf.Round(alpha * FadeSteps) / FadeSteps;
  }

  Vector2 ClampDirection(Vector2 velocity)
  {
    float angle = Mathf.Atan2(Mathf.Abs(velocity.y), Mathf.Abs(velocity.x)) * Mathf.Rad2Deg;
    angle = Mathf.Clamp(angle, MinAngleFromHorizontalDeg, MaxAngleFromHorizontalDeg) * Mathf.Deg2Rad;
    float signX = velocity.x >= 0f ? 1f : -1f;
    float signY = velocity.y >= 0f ? 1f : -1f;
    return new Vector2(Mathf.Cos(angle) * signX, Mathf.Sin(angle) * signY);
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    var paddle = collision.gameObject.GetComponent<Paddle>();
    if (paddle != null)
    {
      HitByPaddle(paddle);
    }
    else if (collision.gameObject.GetComponent<EnemyManager>() == null && collision.gameObject.GetComponent<Boss>() == null)
    {
      SoundEffects.Play(Sfx.WallHit);
    }
  }

  void HitByPaddle(Paddle paddle)
  {
    // バーの裏側から当たったとき（敵のバーの上に上から落ちてきた等）は、ふつうに跳ね返すだけにする
    bool isFromFront = velocityBeforeCollision.y * paddle.ForwardY < 0f;
    if (!isFromFront)
    {
      return;
    }
    SoundEffects.Play(Sfx.PaddleHit);

    // バーから見たボールの方向に跳ね返す（端で当てるほど斜めに飛ぶ）
    Vector2 direction = ((Vector2)transform.position - (Vector2)paddle.transform.position).normalized;
    // バーの端に当たっても、必ず相手側へ打ち返す（味方の側へ飛ばない）
    direction.y = Mathf.Abs(direction.y) * paddle.ForwardY;
    if (paddle.AimStrength > 0f)
    {
      direction = AimAtOpponentLife(paddle, direction);
    }

    float newSpeed = Mathf.Min(myRigidbody2d.linearVelocity.magnitude * SpeedUpPerPaddleHit, maxSpeed);
    float swing = paddle.VelocityX;
    float spinScale = paddle.Character == CharacterId.Curve ? CurveCharacterSpinMultiplier : 1f;
    float effectiveSwing = Mathf.Sign(swing) * Mathf.Max(0f, Mathf.Abs(swing) - SpinDeadZoneSpeed);
    spin = Mathf.Clamp(effectiveSwing * SpinPerPaddleSpeed * spinScale, -MaxSpin, MaxSpin);
    bool isSmash = Mathf.Abs(swing) >= SmashPaddleSpeed;
    if (isSmash)
    {
      smashTimer = SmashSeconds;
      IsSmashCharged = true;
      newSpeed = Mathf.Min(newSpeed * SmashSpeedMultiplier, maxSpeed * SmashMaxSpeedMultiplier);
      SoundEffects.Play(Sfx.Smash);
      if (paddle.Side == PaddleSide.Bottom && RunState.Has(UpgradeId.Meteor) && StageRunController.Instance != null)
      {
        meteorTimer = MeteorSeconds;
      }
    }
    else if (Mathf.Abs(spin) > 0f)
    {
      SoundEffects.Play(Sfx.Curve);
    }
    if (paddle.Side == PaddleSide.Bottom && StageRunController.Instance != null)
    {
      pierceLeft = RunState.PierceCount;
    }
    myRigidbody2d.linearVelocity = direction * newSpeed;

    // キャラクターの技（打った人のキャラで球の性質が変わる）
    ghostTimer = paddle.Character == CharacterId.Ghost ? 0f : -1f;
    if (paddle.Character == CharacterId.Big)
    {
      StartBig(BigCharacterScale);
    }
    else if (IsMeteor)
    {
      StartBig(MeteorScale);
    }
    else
    {
      EndBig();
    }

    if (StageRunController.Instance != null)
    {
      StageRunController.Instance.OnPaddleHit(this, paddle, isSmash, transform.position);
    }
    if (VersusController.Instance != null)
    {
      VersusController.Instance.OnPaddleHit(paddle, isSmash, transform.position);
    }
  }

  // 相手の仲間スライムのどれかへ向けて打ち返す（強さに応じて自然な跳ね返りと混ぜる）
  Vector2 AimAtOpponentLife(Paddle paddle, Vector2 natural)
  {
    PaddleSide opponent = paddle.Side == PaddleSide.Bottom ? PaddleSide.Top : PaddleSide.Bottom;
    var targets = FindObjectsByType<EnemyManager>(FindObjectsSortMode.None).Where(e => e.IsLife && e.LifeSide == opponent).ToList();
    if (targets.Count == 0)
    {
      return natural;
    }
    Vector2 target = targets[Random.Range(0, targets.Count)].transform.position;
    Vector2 aim = (target - (Vector2)transform.position).normalized;
    return Vector2.Lerp(natural, aim, paddle.AimStrength).normalized;
  }

  void StartBig(float scale)
  {
    if (!isBig)
    {
      normalScale = transform.localScale;
      transform.localScale = normalScale * scale;
      isBig = true;
    }
  }

  void EndBig()
  {
    if (isBig)
    {
      transform.localScale = normalScale;
      isBig = false;
    }
  }
}
