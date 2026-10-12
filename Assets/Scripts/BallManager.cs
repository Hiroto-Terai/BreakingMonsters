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
  public AudioClip PlayerColSound;
  public AudioClip EnemyColSound;
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
    EndBig();
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
    // カーブ：横向きの力を少しずつ弱めながらかけ続ける
    if (spin != 0f && IsMoving)
    {
      myRigidbody2d.linearVelocity += Vector2.right * spin * Time.fixedDeltaTime;
      spin = Mathf.MoveTowards(spin, 0f, SpinDecayPerSecond * Time.fixedDeltaTime);
    }
    velocityBeforeCollision = myRigidbody2d.linearVelocity;
  }

  // Update is called once per frame
  void Update()
  {
    smashTimer -= Time.deltaTime;
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
      if (LoadedElement != Element.None) return Elements.ColorOf(LoadedElement);
      if (smashTimer > 0f) return PixelArt.Yellow;
      return PixelArt.LightBlue;
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
    Color tint = LoadedElement != Element.None ? Elements.ColorOf(LoadedElement) : Color.white;
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
  }

  void HitByPaddle(Paddle paddle)
  {
    // バーの裏側から当たったとき（敵のバーの上に上から落ちてきた等）は、ふつうに跳ね返すだけにする
    bool isFromFront = velocityBeforeCollision.y * paddle.ForwardY < 0f;
    if (!isFromFront)
    {
      return;
    }
    AudioClip sound = paddle.Side == PaddleSide.Bottom ? PlayerColSound : EnemyColSound;
    if (OptionManager.isSePlaying)
    {
      AudioSource.PlayClipAtPoint(sound, transform.position, 1.0f);
    }

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
    }
    myRigidbody2d.linearVelocity = direction * newSpeed;

    // キャラクターの技（打った人のキャラで球の性質が変わる）
    ghostTimer = paddle.Character == CharacterId.Ghost ? 0f : -1f;
    if (paddle.Character == CharacterId.Big)
    {
      StartBig();
    }
    else
    {
      EndBig();
    }

    if (StageRunController.Instance != null)
    {
      StageRunController.Instance.OnPaddleHit(paddle, isSmash, transform.position);
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

  void StartBig()
  {
    if (!isBig)
    {
      normalScale = transform.localScale;
      transform.localScale = normalScale * BigCharacterScale;
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
