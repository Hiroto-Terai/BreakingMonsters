using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵スライムと味方スライムの両方に付いている。敵はHPが0になると消え、味方はボールや弾に当たると消える
public class EnemyManager : MonoBehaviour
{
  const float FlashSeconds = 0.08f;
  // HPの点（2x2ドット）を頭の上に並べる
  const float PipDots = 2f;
  const float PipGap = 0.15f;
  const float PipOffsetY = 0.52f;
  // 属性マーク（頭の右上）
  static readonly Vector2 BadgeOffset = new Vector2(0.32f, 0.36f);
  const int MaxPips = 5;
  // 横に歩くモンスターの動き
  const float WalkLimitX = 2.05f;

  public AudioClip BallColSound;
  public int Hp { get; private set; } = 1;
  public bool IsLife { get; private set; }
  // 味方スライムがどちら側の仲間か（対戦モードでは上下両方にいる）
  public PaddleSide LifeSide { get; private set; }
  public Element Element { get; private set; }
  // ボールが当たってやられたか（モンスターに食べられた・弾に当たったときは false）
  public bool IsKilledByBall { get; private set; }
  // 倒されたとき・食べに行ったときに呼ぶ（呼び出し側で仲間が減る/敵を倒した処理をする）
  public Action<EnemyManager> Killed;
  public Action<EnemyManager> ReachedDefenseLine;

  bool isDead;
  SpriteRenderer spriteRenderer;
  readonly List<SpriteRenderer> pips = new List<SpriteRenderer>();
  float walkSpeed;
  float walkRange;
  float walkOriginX;
  float walkPhase;

  void Awake()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  public void ConfigureMonster(int hp, Element element, float walkSpeedValue, float walkRangeValue)
  {
    Hp = hp;
    IsLife = false;
    Element = element;
    walkSpeed = walkSpeedValue;
    walkRange = walkRangeValue;
    walkOriginX = transform.position.x;
    walkPhase = UnityEngine.Random.value * Mathf.PI * 2f;
    CreatePips();
    if (element != Element.None)
    {
      CreateBadge(element);
    }
  }

  public void ConfigureLife(PaddleSide side)
  {
    Hp = 1;
    IsLife = true;
    LifeSide = side;
    // 上側の仲間は下を向かせる
    spriteRenderer.flipY = side == PaddleSide.Top;
  }

  void Update()
  {
    if (walkSpeed <= 0f || isDead)
    {
      return;
    }
    walkPhase += walkSpeed * Time.deltaTime;
    Vector3 position = transform.position;
    float x = Mathf.Clamp(walkOriginX + Mathf.Sin(walkPhase) * walkRange, -WalkLimitX, WalkLimitX);
    position.x = PixelArt.Snap(new Vector3(x, 0f, 0f)).x;
    transform.position = position;
  }

  // 一段下へ迫ってくる。防衛ラインに着いたら仲間を食べに行く
  public void Advance(float step, float defenseLineY)
  {
    if (isDead)
    {
      return;
    }
    transform.position += Vector3.down * step;
    if (transform.position.y <= defenseLineY)
    {
      isDead = true;
      ReachedDefenseLine?.Invoke(this);
      Destroy(gameObject);
    }
  }

  // HPの数だけ小さな点を頭の上に出す（色ではなく点の数で硬さが分かるようにする）
  void CreatePips()
  {
    var sprite = PixelArt.WorldSprite("dot");
    Vector3 parentScale = transform.lossyScale;
    int count = Mathf.Min(Hp, MaxPips);
    for (int i = 0; i < count; i++)
    {
      var pip = new GameObject("HpPip").AddComponent<SpriteRenderer>();
      pip.sprite = sprite;
      pip.sortingOrder = spriteRenderer.sortingOrder + 1;
      pip.color = PixelArt.White;
      pip.transform.SetParent(transform, false);
      // 親が小さく縮められているので、その分を打ち消してワールドでの大きさをそろえる
      pip.transform.localScale = new Vector3(PipDots / parentScale.x, PipDots / parentScale.y, 1f);
      float x = (i - (count - 1) / 2f) * PipGap;
      pip.transform.localPosition = new Vector3(x / parentScale.x, PipOffsetY / parentScale.y, 0f);
      pips.Add(pip);
    }
  }

  // 体の色を変えると元の色と混ざって見分けにくいので、属性はマークで見せる
  void CreateBadge(Element element)
  {
    var badge = new GameObject("ElementBadge").AddComponent<SpriteRenderer>();
    badge.sprite = PixelArt.WorldSprite("badge_" + element.ToString().ToLower());
    badge.sortingOrder = spriteRenderer.sortingOrder + 1;
    badge.transform.SetParent(transform, false);
    Vector3 parentScale = transform.lossyScale;
    badge.transform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f);
    badge.transform.localPosition = new Vector3(BadgeOffset.x / parentScale.x, BadgeOffset.y / parentScale.y, 0f);
  }

  void UpdatePips()
  {
    for (int i = 0; i < pips.Count; i++)
    {
      pips[i].enabled = i < Hp;
    }
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    var ball = collision.gameObject.GetComponent<BallManager>();
    if (isDead || ball == null)
    {
      return;
    }
    if (IsLife)
    {
      IsKilledByBall = true;
      Kill();
      return;
    }
    TakeDamage(StageRunController.Instance.OnBallHitMonster(ball, this));
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
    UpdatePips();
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
    Killed?.Invoke(this);
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
    Color original = spriteRenderer.color;
    spriteRenderer.color = new Color(original.r, original.g, original.b, 0.3f);
    yield return new WaitForSeconds(FlashSeconds);
    spriteRenderer.color = original;
  }
}
