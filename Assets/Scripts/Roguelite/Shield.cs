using System.Collections;
using UnityEngine;

// 味方スライムの上に張るバリア。ボールをはね返すか弾を防ぐたびに1回分へり、0になると消える
public class Shield : MonoBehaviour
{
  const float HitFlashSeconds = 0.1f;
  const float MinAlpha = 0.45f;
  const float AlphaPerCharge = 0.2f;

  int charges;
  SpriteRenderer spriteRenderer;

  public static Shield Create(int charges)
  {
    var gameObject = new GameObject("Shield");
    gameObject.transform.position = new Vector3(0f, Playfield.ShieldY, 0f);
    var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("shield");
    var collider = gameObject.AddComponent<BoxCollider2D>();
    collider.size = spriteRenderer.sprite.bounds.size;
    var shield = gameObject.AddComponent<Shield>();
    shield.spriteRenderer = spriteRenderer;
    shield.charges = charges;
    shield.UpdateLook();
    return shield;
  }

  public void AddCharge()
  {
    charges++;
    UpdateLook();
  }

  // 残りの回数が多いほど濃く見せる
  void UpdateLook()
  {
    spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.Clamp01(MinAlpha + charges * AlphaPerCharge));
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    // 仲間を傷つけないボール（増えたボールやミニボール）では、回数を減らさずにはね返すだけにする
    var ball = collision.gameObject.GetComponent<BallManager>();
    if (ball != null && ball.CanHurtLives)
    {
      Break();
    }
  }

  public void Break()
  {
    HitEffects.Burst(transform.position, PixelArt.LightBlue);
    SoundEffects.Play(Sfx.Ice);
    charges--;
    if (charges <= 0)
    {
      Destroy(gameObject);
      return;
    }
    UpdateLook();
    StartCoroutine(FlashOnHit());
  }

  IEnumerator FlashOnHit()
  {
    Color color = spriteRenderer.color;
    spriteRenderer.color = Color.white;
    yield return new WaitForSeconds(HitFlashSeconds);
    spriteRenderer.color = color;
  }
}
