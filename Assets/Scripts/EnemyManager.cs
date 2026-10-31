using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵スライムと味方スライムの両方に付いている。敵はHPが0になると消え、味方はボールや弾に当たると消える
public class EnemyManager : MonoBehaviour
{
  const float FlashSeconds = 0.08f;
  const float FlashAlpha = 0.3f;
  // HPの点（2x2ドット）を頭の上に並べる
  const float PipDots = 2f;
  const float PipGap = 0.15f;
  // 体のてっぺん（足元から約0.7）より少し上。体の光る所と重ならないようにする
  const float PipOffsetY = 0.78f;
  // 属性マーク（頭の右上）
  static readonly Vector2 BadgeOffset = new Vector2(0.32f, 0.36f);
  const int MaxPips = 5;
  // 横に歩くモンスターの動き
  const float WalkLimitX = 2.05f;

  public int Hp { get; private set; } = 1;
  public int MaxHp { get; private set; } = 1;
  // 金色のスライム：たまに出て、倒すとジェムとコインがたくさん出る
  public bool IsGolden { get; private set; }
  public bool IsBomb { get; private set; }
  // 当たったときに一瞬ふくらませて、手ごたえを見せる
  const float HitPunchScale = 1.2f;
  Vector3 baseScale;
  const float GoldenBlinkSeconds = 0.15f;
  // 金色に塗った絵に、少し暗い色と交互に掛けてきらきらさせる
  static readonly Color GoldenBlinkTint = new Color(1f, 0.85f, 0.6f);
  // 種類ごとに塗り分けた絵（Resources/Sprites/slime_<種類>.png。コマの名前は元の絵と同じ）。
  // 色を掛けるだけでは青い体の色が混ざってしまうので、絵ごと差しかえる。null なら元の青いスライムのまま
  const string SkinSpritesPath = "Sprites/slime_";
  static readonly Dictionary<string, Dictionary<string, Sprite>> skinSprites = new Dictionary<string, Dictionary<string, Sprite>>();
  string skin;
  // 倒されたときに飛び散る破片の色。体の地の色（Tools/PixelArt/sprites.py のパレットの "2"）に合わせる。爆弾は火花の色
  static readonly Color32 NormalBurstColor = new Color32(70, 140, 240, 255);
  static readonly Dictionary<string, Color32> SkinBurstColors = new Dictionary<string, Color32>
  {
    ["gold"] = new Color32(255, 204, 51, 255),
    ["bomb"] = new Color32(255, 150, 60, 255),
    ["fire"] = new Color32(245, 98, 44, 255),
    ["ice"] = new Color32(156, 222, 255, 255),
    ["thunder"] = new Color32(164, 116, 255, 255),
  };

  public Color BurstColor => skin != null && SkinBurstColors.TryGetValue(skin, out var color) ? color : NormalBurstColor;
  const float NumberOffsetY = 0.3f;
  const int BigNumberDamage = 4;
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

  public void ConfigureMonster(MonsterSpawn spawn)
  {
    Hp = spawn.Hp;
    MaxHp = spawn.Hp;
    IsGolden = spawn.IsGolden;
    IsBomb = spawn.IsBomb;
    baseScale = transform.localScale;
    IsLife = false;
    Element = spawn.Element;
    skin = SkinOf(spawn);
    walkSpeed = spawn.WalkSpeed;
    walkRange = spawn.WalkRange;
    walkOriginX = transform.position.x;
    walkPhase = UnityEngine.Random.value * Mathf.PI * 2f;
    CreatePips();
    // 爆弾は導火線つきの絵で分かるので、マークは属性のときだけ付ける
    if (Element != Element.None)
    {
      CreateBadge("badge_" + Element.ToString().ToLower());
    }
  }

  static string SkinOf(MonsterSpawn spawn)
  {
    if (spawn.IsGolden) return "gold";
    if (spawn.IsBomb) return "bomb";
    if (spawn.Element != Element.None) return spawn.Element.ToString().ToLower();
    return null;
  }

  // アニメーションが毎フレーム元の絵に戻すので、そのあとで同じコマの種類の絵に差しかえる
  void LateUpdate()
  {
    if (skin != null && !isDead)
    {
      spriteRenderer.sprite = SkinSprite(skin, spriteRenderer.sprite);
    }
  }

  static Sprite SkinSprite(string skinName, Sprite source)
  {
    if (!skinSprites.TryGetValue(skinName, out var sprites))
    {
      sprites = new Dictionary<string, Sprite>();
      foreach (var sprite in Resources.LoadAll<Sprite>(SkinSpritesPath + skinName))
      {
        sprites[sprite.name] = sprite;
      }
      skinSprites[skinName] = sprites;
    }
    return sprites[source.name];
  }

  public void ConfigureLife(PaddleSide side)
  {
    Hp = 1;
    IsLife = true;
    LifeSide = side;
    skin = "ally";
    // 上側の仲間は下を向かせる
    spriteRenderer.flipY = side == PaddleSide.Top;
  }

  void Update()
  {
    if (IsGolden && !isDead)
    {
      bool isBlink = Mathf.FloorToInt(Time.time / GoldenBlinkSeconds) % 2 == 0;
      spriteRenderer.color = isBlink ? GoldenBlinkTint : Color.white;
    }
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
    // 1発で倒せる相手に点を出しても情報にならず、画面がうるさくなるだけなので出さない
    if (MaxHp <= 1)
    {
      return;
    }
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
  void CreateBadge(string shapeKey)
  {
    var badge = new GameObject("ElementBadge").AddComponent<SpriteRenderer>();
    badge.sprite = PixelArt.WorldSprite(shapeKey);
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
      // 強化で増えたボールは仲間を傷つけない（たくさん出しても怖くないようにする）
      if (!ball.CanHurtLives)
      {
        return;
      }
      IsKilledByBall = true;
      Kill();
      return;
    }
    var hit = StageRunController.Instance.OnBallHitMonster(ball, this);
    TakeDamage(hit.Damage, hit.IsCritical);
    if (isDead)
    {
      ball.OnKilledMonster();
    }
  }

  public bool IsDead => isDead;

  public void TakeDamage(int damage, bool isCritical)
  {
    if (isDead)
    {
      return;
    }
    SoundEffects.Play(Sfx.MonsterHit);
    DamageNumbers.Spawn(transform.position + Vector3.up * NumberOffsetY, damage, isCritical ? (Color)PixelArt.Yellow : Color.white, isCritical || damage >= BigNumberDamage);
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
    Killed?.Invoke(this);
    Destroy(gameObject);
  }

  IEnumerator Flash()
  {
    spriteRenderer.color = new Color(1f, 1f, 1f, FlashAlpha);
    transform.localScale = baseScale * HitPunchScale;
    yield return new WaitForSeconds(FlashSeconds);
    spriteRenderer.color = Color.white;
    transform.localScale = baseScale;
  }
}
