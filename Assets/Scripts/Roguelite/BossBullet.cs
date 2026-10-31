using UnityEngine;

// ボスの弾：まっすぐ下に落ちる。バーで防げて、味方スライムやバリアに当たると壊す
public class BossBullet : MonoBehaviour
{
  const float DestroyBelowY = -6f;

  public static void Fire(Vector3 position, float speed)
  {
    var gameObject = new GameObject("BossBullet");
    gameObject.transform.position = position;
    var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("bullet");
    spriteRenderer.sortingOrder = 5;
    // すり抜け判定（トリガー）で当たりを調べ、ボールの動きには影響しないようにする
    var body = gameObject.AddComponent<Rigidbody2D>();
    body.gravityScale = 0f;
    body.linearVelocity = Vector2.down * speed;
    var collider = gameObject.AddComponent<CircleCollider2D>();
    collider.isTrigger = true;
    collider.radius = spriteRenderer.sprite.bounds.extents.x;
    gameObject.AddComponent<BossBullet>();
  }

  void Update()
  {
    if (transform.position.y < DestroyBelowY || !StageRunController.Instance.IsPlaying)
    {
      Destroy(gameObject);
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    var life = other.GetComponent<EnemyManager>();
    var shield = other.GetComponent<Shield>();
    bool isPaddle = other.CompareTag("Player");
    if (life != null && life.IsLife)
    {
      life.Kill();
    }
    else if (shield != null)
    {
      shield.Break();
    }
    else if (!isPaddle)
    {
      return;
    }
    HitEffects.Burst(transform.position, PixelArt.Red);
    Destroy(gameObject);
  }
}
