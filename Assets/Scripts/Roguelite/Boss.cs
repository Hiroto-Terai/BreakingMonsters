using System.Collections;
using UnityEngine;

// ボス：左右に動きながら弾を撃つ。ボールが当たるとHPが減る
public class Boss : MonoBehaviour
{
  // ドットを2倍の大きさで描いて存在感を出す（整数倍なのでドットの見た目は崩れない）
  const float PixelScale = 2f;
  const float MoveLimitX = 0.85f;
  // 2コマの絵を交互に出して、ぷるぷる動いているように見せる
  const float FrameSeconds = 0.35f;
  static readonly Vector2 BottomPivot = new Vector2(0.5f, 0f);
  const float ColliderShrink = 0.85f;
  const float FlashSeconds = 0.08f;
  // ボールが何個あっても一瞬で倒れないよう、当たった直後は少しだけ無敵にする
  const float InvincibleSeconds = 0.12f;
  float invincibleUntil;
  const float ShotOffsetY = 0.1f;

  public int Hp { get; private set; }
  public int MaxHp { get; private set; }
  FloorData floor;
  SpriteRenderer spriteRenderer;
  float direction = 1f;
  float shotTimer;
  bool isDead;

  // bottomY はボスの足元（絵の下端）の高さ
  public static Boss Create(FloorData floor, float bottomY)
  {
    var gameObject = new GameObject("Boss");
    gameObject.tag = "Enemy";
    gameObject.transform.position = new Vector3(0f, bottomY, 0f);
    gameObject.transform.localScale = Vector3.one * PixelScale;
    var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("boss", BottomPivot);
    // ボールと同じ物理で跳ね返るよう、動く壁（Kinematic）として扱う
    var body = gameObject.AddComponent<Rigidbody2D>();
    body.bodyType = RigidbodyType2D.Kinematic;
    var collider = gameObject.AddComponent<BoxCollider2D>();
    collider.size = spriteRenderer.sprite.bounds.size * ColliderShrink;
    collider.offset = spriteRenderer.sprite.bounds.center;
    var boss = gameObject.AddComponent<Boss>();
    boss.floor = floor;
    boss.MaxHp = floor.BossHp;
    boss.Hp = floor.BossHp;
    boss.spriteRenderer = spriteRenderer;
    boss.shotTimer = floor.BossShotInterval;
    return boss;
  }

  void Update()
  {
    bool isSquashed = Mathf.FloorToInt(Time.time / FrameSeconds) % 2 == 1;
    spriteRenderer.sprite = PixelArt.WorldSprite(isSquashed ? "boss_b" : "boss", BottomPivot);
    if (isDead || !StageRunController.Instance.IsPlaying)
    {
      return;
    }
    Vector3 position = transform.position;
    position.x += direction * floor.BossMoveSpeed * Time.deltaTime;
    if (Mathf.Abs(position.x) > MoveLimitX)
    {
      position.x = Mathf.Sign(position.x) * MoveLimitX;
      direction = -direction;
    }
    transform.position = position;

    shotTimer -= Time.deltaTime;
    if (shotTimer <= 0f)
    {
      shotTimer = floor.BossShotInterval;
      BossBullet.Fire(transform.position + Vector3.down * ShotOffsetY, floor.BulletSpeed);
    }
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    var ball = collision.gameObject.GetComponent<BallManager>();
    if (isDead || ball == null || Time.time < invincibleUntil)
    {
      return;
    }
    invincibleUntil = Time.time + InvincibleSeconds;
    Hp -= StageRunController.Instance.OnBallHitBoss(ball);
    HitEffects.Burst(collision.GetContact(0).point, PixelArt.LightBlue);
    if (Hp <= 0)
    {
      isDead = true;
      Hp = 0;
      StageRunController.Instance.OnBossDefeated(transform.position);
      Destroy(gameObject);
      return;
    }
    StartCoroutine(Flash());
  }

  IEnumerator Flash()
  {
    spriteRenderer.color = PixelArt.Red;
    yield return new WaitForSeconds(FlashSeconds);
    spriteRenderer.color = Color.white;
  }
}
