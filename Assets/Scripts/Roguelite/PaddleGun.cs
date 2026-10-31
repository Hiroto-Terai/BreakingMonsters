using UnityEngine;

// 自分のバーに付けて、強化「ショット」の弾と、進化「ギガバー」のレーザーを自動で撃つ
public class PaddleGun : MonoBehaviour
{
  const float SpreadX = 0.25f;
  const float MuzzleOffsetY = 0.25f;
  const float LaserInterval = 3.5f;

  float shotTimer;
  float laserTimer = LaserInterval;

  void Update()
  {
    var controller = StageRunController.Instance;
    if (controller == null || !controller.IsPlaying)
    {
      return;
    }
    if (RunState.Has(UpgradeId.Shot))
    {
      shotTimer -= Time.deltaTime;
      if (shotTimer <= 0f)
      {
        shotTimer = RunState.ShotInterval;
        Fire(RunState.ShotWays);
      }
    }
    if (RunState.Has(UpgradeId.GigaPaddle))
    {
      laserTimer -= Time.deltaTime;
      if (laserTimer <= 0f)
      {
        laserTimer = LaserInterval;
        Laser.Fire(transform.position.x, transform.position.y + MuzzleOffsetY);
      }
    }
  }

  void Fire(int ways)
  {
    SoundEffects.Play(Sfx.Shot);
    for (int i = 0; i < ways; i++)
    {
      float x = (i - (ways - 1) / 2f) * SpreadX;
      PlayerBullet.Fire(transform.position + new Vector3(x, MuzzleOffsetY, 0f));
    }
  }
}

// バーから上にまっすぐ飛ぶ弾。モンスターやボスに当たると消える
public class PlayerBullet : MonoBehaviour
{
  const float Speed = 9f;
  const float ColliderRadius = 0.08f;
  const int BulletSortingOrder = 6;
  const int ThunderStormTargets = 3;

  public static void Fire(Vector3 position)
  {
    var gameObject = new GameObject("PlayerBullet");
    gameObject.transform.position = position;
    var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("player_bullet");
    spriteRenderer.sortingOrder = BulletSortingOrder;
    var body = gameObject.AddComponent<Rigidbody2D>();
    body.gravityScale = 0f;
    body.linearVelocity = Vector2.up * Speed;
    var collider = gameObject.AddComponent<CircleCollider2D>();
    collider.isTrigger = true;
    collider.radius = ColliderRadius;
    gameObject.AddComponent<PlayerBullet>();
  }

  void Update()
  {
    if (transform.position.y > Playfield.TopWallInnerY || !StageRunController.Instance.IsPlaying)
    {
      Destroy(gameObject);
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    var monster = other.GetComponent<EnemyManager>();
    var boss = other.GetComponent<Boss>();
    if (monster != null && !monster.IsLife)
    {
      if (RunState.Has(UpgradeId.ThunderStorm))
      {
        StageRunController.Instance.ChainLightning(monster, ThunderStormTargets, 1);
      }
      monster.TakeDamage(StageRunController.Instance.ShotDamage, false);
    }
    else if (boss != null)
    {
      boss.TakeDamage(StageRunController.Instance.ShotDamage);
    }
    else
    {
      return;
    }
    HitEffects.Burst(transform.position, PixelArt.Yellow);
    Destroy(gameObject);
  }
}

// ギガバーのレーザー：バーの真上の列にいるモンスターをまとめて撃ち抜く
public static class Laser
{
  const float HalfWidth = 0.35f;
  const int Damage = 3;
  const float ShowSeconds = 0.25f;
  const int LaserSortingOrder = 7;

  public static void Fire(float x, float bottomY)
  {
    SoundEffects.Play(Sfx.Smash);
    HitEffects.Shake(2, 0.15f);
    var beam = new GameObject("Laser").AddComponent<SpriteRenderer>();
    beam.sprite = PixelArt.WorldSprite("laser", new Vector2(0.5f, 0f));
    beam.sortingOrder = LaserSortingOrder;
    float height = Playfield.TopWallInnerY - bottomY;
    beam.transform.position = new Vector3(x, bottomY, 0f);
    beam.transform.localScale = new Vector3(1f, height / PixelArt.WorldPixelSize, 1f);
    Object.Destroy(beam.gameObject, ShowSeconds);

    foreach (var monster in Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None))
    {
      if (!monster.IsLife && Mathf.Abs(monster.transform.position.x - x) <= HalfWidth && monster.transform.position.y > bottomY)
      {
        monster.TakeDamage(Damage, false);
      }
    }
    var boss = Object.FindFirstObjectByType<Boss>();
    if (boss != null && Mathf.Abs(boss.transform.position.x - x) <= HalfWidth * 3f)
    {
      boss.TakeDamage(Damage);
    }
  }
}
