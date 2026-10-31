using UnityEngine;

// 敵のバーの性格
public enum AiStyle
{
  Guard,   // しっかり守る。狙い打ちは控えめ
  Sniper,  // 仲間スライムを狙って打ち返してくる
  Trick,   // 逆に動くフェイントを入れてくる
}

// 敵のバー（コンピュータ）：ボールが跳ね返る先を読んで先回りし、打ち返して仲間スライムを狙う
public class EnemyBarManager : MonoBehaviour
{
  // 敵のバーのスピード
  public float speed = 3f;
  // 左右に動ける範囲
  public float limitX = 1.8f;
  // 次の行き先を考え直すまでの時間（短いほど反応がよい）
  public float reactionSeconds = 0.3f;
  // 予測のぶれ（大きいほど読みが外れる）
  public float aimError = 0.5f;
  public AiStyle style = AiStyle.Guard;

  // 壁の内側の端からボールの半径を引いた、ボールの中心が動ける範囲
  const float BallFieldHalfWidth = 2.3f;
  const float MinReadAhead = 0.25f;
  const float TrickFakeChance = 0.35f;
  const float TrickFakeSeconds = 0.22f;
  const float TrickFakeDistance = 0.6f;
  // 打つ直前にバーを振って、カーブやスマッシュをかける（守り型はしない）
  const float SwingWindowSeconds = 0.14f;
  // バーの半分の幅より小さくして、振っている間も打点をはずさないようにする
  const float SwingOffsetX = 0.35f;
  const float SwingSpeedMultiplier = 1.6f;
  // 機敏な動き：行き先が決まると素早く加速し、着いたらほぼピタッと止まる（少しだけ行き過ぎて戻る）
  const float SpringStiffness = 400f;
  const float SpringDampingRatio = 0.65f;
  const float MinDashSpeedMultiplier = 1.1f;
  const float MaxDashSpeedMultiplier = 1.5f;
  // 一度決めた行き先は少しの間変えない（ボールにずっと吸い付いて見えないようにする）
  const float CommitMultiplier = 1.8f;
  // ボールが離れている間は、小さく揺れて構える
  const float IdleSwayAmount = 0.06f;
  const float IdleSwaySpeed = 6f;

  Paddle paddle;
  SpriteRenderer spriteRenderer;
  float targetX;
  float retargetTimer;
  float fakeUntil;
  float fakeX;
  float frozenUntil;
  BallManager trackedBall;
  float swingDirection;
  float level;
  float velocityX;

  public bool IsFrozen => Time.time < frozenUntil;

  void Awake()
  {
    paddle = GetComponent<Paddle>();
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  // 強さ(0〜1)と性格から、動きの速さ・反応・狙いの強さを決める
  public void Configure(AiStyle aiStyle, float strength, float moveLimitX)
  {
    style = aiStyle;
    level = strength;
    limitX = moveLimitX;
    // 強さ0（ステージ1）はかなり弱く、強さ1（たいせんの「つよい」）は速い球にも追いつける
    speed = 1.5f + strength * 9.5f;
    reactionSeconds = 0.5f - strength * 0.42f;
    aimError = 0.9f - strength * 0.8f;
    switch (style)
    {
      case AiStyle.Sniper: paddle.AimStrength = 0.35f + strength * 0.5f; break;
      case AiStyle.Trick: paddle.AimStrength = 0.2f + strength * 0.3f; break;
      default: paddle.AimStrength = strength * 0.15f; break;
    }
  }

  // こおり属性で一時的に動けなくする
  public void Freeze(float seconds)
  {
    frozenUntil = Time.time + seconds;
  }

  // Update is called once per frame
  void Update()
  {
    // こおっている間は水色にして止まる
    spriteRenderer.color = IsFrozen ? Elements.ColorOf(Element.Ice) : Color.white;
    if (IsFrozen)
    {
      return;
    }
    var approaching = FindApproachingBall();
    if (approaching.ball != trackedBall)
    {
      // 新しいボールが向かってきたら、気づくまでの間（反応時間）を置いてから動き出す
      OnNewBall(approaching.ball);
    }
    retargetTimer -= Time.deltaTime;
    if (retargetTimer <= 0f)
    {
      retargetTimer = trackedBall != null ? reactionSeconds * CommitMultiplier : reactionSeconds;
      targetX = trackedBall != null ? PredictTargetX(approaching.ball, approaching.velocity) : 0f;
    }
    float goal = Time.time < fakeUntil ? fakeX : targetX;
    // 弱い敵はダッシュも控えめにする
    float maxSpeed = speed * Mathf.Lerp(MinDashSpeedMultiplier, MaxDashSpeedMultiplier, level);
    if (trackedBall == null)
    {
      goal += Mathf.Sin(Time.time * IdleSwaySpeed) * IdleSwayAmount;
    }
    else if (swingDirection != 0f)
    {
      // 打点の手前側で構えて、届く直前に打点を通り抜けるように振り抜く
      bool isSwinging = IsAboutToHit();
      goal += swingDirection * SwingOffsetX * (isSwinging ? 1f : -1f);
      if (isSwinging)
      {
        maxSpeed *= SwingSpeedMultiplier;
      }
    }
    MoveTowardsGoal(Mathf.Clamp(goal, -limitX, limitX), maxSpeed);
  }

  // ばねのように行き先へ引っぱられて動く。遠いときは最高速度で走り、近づくと急に止まる
  void MoveTowardsGoal(float goal, float maxSpeed)
  {
    Vector3 position = transform.position;
    float damping = 2f * SpringDampingRatio * Mathf.Sqrt(SpringStiffness);
    float acceleration = SpringStiffness * (goal - position.x) - damping * velocityX;
    velocityX = Mathf.Clamp(velocityX + acceleration * Time.deltaTime, -maxSpeed, maxSpeed);
    position.x += velocityX * Time.deltaTime;
    if (Mathf.Abs(position.x) > limitX)
    {
      position.x = Mathf.Sign(position.x) * limitX;
      velocityX = 0f;
    }
    transform.position = position;
  }

  void OnNewBall(BallManager ball)
  {
    trackedBall = ball;
    retargetTimer = reactionSeconds;
    if (ball == null)
    {
      return;
    }
    // 強いほど振って打つことが多い
    swingDirection = style != AiStyle.Guard && Random.value < level ? (Random.value < 0.5f ? -1f : 1f) : 0f;
    // たまに、ボールと反対側へ一歩ぶれるフェイントを入れる
    if (style == AiStyle.Trick && Random.value < TrickFakeChance)
    {
      fakeX = transform.position.x - Mathf.Sign(ball.transform.position.x - transform.position.x) * TrickFakeDistance;
      fakeUntil = Time.time + reactionSeconds + TrickFakeSeconds;
    }
  }

  // こちらへ向かってくるボールのうち、一番早く届くもの（なければ ball が null）
  (BallManager ball, Vector2 velocity) FindApproachingBall()
  {
    BallManager nearest = null;
    Vector2 nearestVelocity = Vector2.zero;
    float nearestTime = float.MaxValue;
    foreach (var ball in FindObjectsByType<BallManager>(FindObjectsSortMode.None))
    {
      Vector2 velocity = ball.GetComponent<Rigidbody2D>().linearVelocity;
      if (velocity.y * paddle.ForwardY >= 0f || Mathf.Abs(velocity.y) < 0.01f)
      {
        continue;
      }
      float time = Mathf.Abs((transform.position.y - ball.transform.position.y) / velocity.y);
      if (time < nearestTime)
      {
        nearest = ball;
        nearestVelocity = velocity;
        nearestTime = time;
      }
    }
    return (nearest, nearestVelocity);
  }

  float PredictTargetX(BallManager ball, Vector2 velocity)
  {
    // 弱い敵は跳ね返り先を読み切れず、今ボールがある位置を追いかけがちにする
    float readAhead = Mathf.Clamp01(MinReadAhead + level);
    float predicted = PredictX(ball.transform.position, velocity, transform.position.y);
    return Mathf.Lerp(ball.transform.position.x, predicted, readAhead) + Random.Range(-aimError, aimError);
  }

  // 追っているボールが、もうすぐバーに届くか
  bool IsAboutToHit()
  {
    if (trackedBall == null || swingDirection == 0f)
    {
      return false;
    }
    Vector2 velocity = trackedBall.GetComponent<Rigidbody2D>().linearVelocity;
    if (velocity.y * paddle.ForwardY >= 0f)
    {
      return false;
    }
    float timeToReach = (transform.position.y - trackedBall.transform.position.y) / velocity.y;
    return timeToReach > 0f && timeToReach < SwingWindowSeconds;
  }

  // まっすぐ進んで左右の壁で跳ね返ると仮定して、高さ y を通るときの x を求める（カーブは読めない）
  static float PredictX(Vector2 position, Vector2 velocity, float y)
  {
    float time = (y - position.y) / velocity.y;
    float x = position.x + velocity.x * time;
    float width = BallFieldHalfWidth * 2f;
    // 壁での跳ね返りを、折り返しで計算する
    float shifted = Mathf.Repeat(x + BallFieldHalfWidth, width * 2f);
    if (shifted > width)
    {
      shifted = width * 2f - shifted;
    }
    return shifted - BallFieldHalfWidth;
  }
}
