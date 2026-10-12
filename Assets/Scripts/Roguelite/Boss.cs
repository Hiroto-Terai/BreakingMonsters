using System.Collections;
using UnityEngine;

// ボス：左右に動きながら弾を撃つ。ボールが当たるとHPが減る
public class Boss : MonoBehaviour
{
  // ドットを2倍の大きさで描いて存在感を出す（整数倍なのでドットの見た目は崩れない）
  const float PixelScale = 2f;
  const float MoveLimitX = 1f;
  const float PositionY = 1.55f;
  const float ColliderShrink = 0.85f;
  const float FlashSeconds = 0.08f;
  // ボールが何個あっても一瞬で倒れないよう、当たった直後は少しだけ無敵にする
  const float InvincibleSeconds = 0.12f;
  float invincibleUntil;
  const float ShotOffsetY = 1.1f;

  public int Hp { get; private set; }
  public int MaxHp { get; private set; }
  FloorData floor;
  SpriteRenderer spriteRenderer;
  float direction = 1f;
  float shotTimer;
  bool isDead;

  public static Boss Create(FloorData floor)
  {
    var gameObject = new GameObject("Boss");
    gameObject.tag = "Enemy";
    gameObject.transform.position = new Vector3(0f, PositionY, 0f);
    gameObject.transform.localScale = Vector3.one * PixelScale;
    var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("boss");
    // ボールと同じ物理で跳ね返るよう、動く壁（Kinematic）として扱う
    var body = gameObject.AddComponent<Rigidbody2D>();
    body.bodyType = RigidbodyType2D.Kinematic;
    var collider = gameObject.AddComponent<BoxCollider2D>();
    collider.size = spriteRenderer.sprite.bounds.size * ColliderShrink;
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
    if (isDead || collision.gameObject.GetComponent<BallManager>() == null || Time.time < invincibleUntil)
    {
      return;
    }
    invincibleUntil = Time.time + InvincibleSeconds;
    Hp -= StageRunController.Instance.RollBallDamage();
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
