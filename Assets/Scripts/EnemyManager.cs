using System.Collections;
using UnityEngine;

// 敵スライムと味方スライムの両方に付いている。敵はHPが0になると消え、味方はボールや弾に当たると消える
public class EnemyManager : MonoBehaviour
{
  const float FlashSeconds = 0.08f;

  public AudioClip BallColSound;
  public int Hp { get; private set; } = 1;
  public bool IsLife { get; private set; }
  bool isDead;
  SpriteRenderer spriteRenderer;

  // HPが多いほど色を変えて、硬さが見て分かるようにする
  static readonly Color[] HpColors =
  {
    Color.white,
    new Color(0.6f, 1f, 0.6f),
    new Color(1f, 0.85f, 0.4f),
    new Color(1f, 0.55f, 0.35f),
    new Color(1f, 0.4f, 0.6f),
  };

  void Awake()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  public void Configure(int hp, bool isLife)
  {
    Hp = hp;
    IsLife = isLife;
    UpdateColor();
  }

  void UpdateColor()
  {
    spriteRenderer.color = IsLife ? Color.white : HpColors[Mathf.Clamp(Hp - 1, 0, HpColors.Length - 1)];
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (isDead || collision.gameObject.GetComponent<BallManager>() == null)
    {
      return;
    }
    if (IsLife)
    {
      Kill();
      return;
    }
    TakeDamage(StageRunController.Instance.RollBallDamage());
  }

  public void TakeDamage(int damage)
  {
    if (isDead)
    {
      return;
    }
    PlayHitSound();
    Hp -= damage;
    if (Hp <= 0)
    {
      Kill();
      return;
    }
    UpdateColor();
    StartCoroutine(Flash());
  }

  // 敵なら撃破、味方ならやられた扱いにして消える
  public void Kill()
  {
    if (isDead)
    {
      return;
    }
    isDead = true;
    PlayHitSound();
    if (IsLife)
    {
      StageRunController.Instance.OnLifeLost(transform.position);
    }
    else
    {
      StageRunController.Instance.OnMonsterDefeated(transform.position);
    }
    Destroy(gameObject);
  }

  void PlayHitSound()
  {
    if (OptionManager.isSePlaying)
    {
      AudioSource.PlayClipAtPoint(BallColSound, transform.position);
    }
  }

  IEnumerator Flash()
  {
    spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
    yield return new WaitForSeconds(FlashSeconds);
    UpdateColor();
  }
}
