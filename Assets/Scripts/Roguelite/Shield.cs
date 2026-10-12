using UnityEngine;

// 味方スライムの上に張るバリア。ボールを1回はね返すか、弾を1発防ぐと消える
public class Shield : MonoBehaviour
{
  const float PositionY = -3.42f;

  public static Shield Create()
  {
    var gameObject = new GameObject("Shield");
    gameObject.transform.position = new Vector3(0f, PositionY, 0f);
    var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("shield");
    spriteRenderer.color = new Color(1f, 1f, 1f, 0.8f);
    var collider = gameObject.AddComponent<BoxCollider2D>();
    collider.size = spriteRenderer.sprite.bounds.size;
    return gameObject.AddComponent<Shield>();
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.gameObject.GetComponent<BallManager>() != null)
    {
      Break();
    }
  }

  public void Break()
  {
    HitEffects.Burst(transform.position, PixelArt.LightBlue);
    Destroy(gameObject);
  }
}
