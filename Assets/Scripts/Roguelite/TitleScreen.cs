using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// タイトル画面をにぎやかにする：床の模様、ぷるぷる動く王冠のボス、いろいろな種類のスライムがはねる列。
// どんなモンスターと戦うゲームなのかが、最初の画面でひと目で分かるようにする
// カメラとキャンバスの大きさが決まってから並べる
[DefaultExecutionOrder(100)]
public class TitleScreen : MonoBehaviour
{
  [SerializeField] Text logo;
  [SerializeField] Text startLabel;

  // ロゴの下端から「タップでスタート」の上端までを帯として、その中の位置（下が0、上が1）に並べる。
  // 文字の位置は画面の縦横比で変わるので、画面の割合ではなく文字を基準にする
  const float ParadeInBand = 0.08f;
  const float BossInBand = 0.3f;
  const float BossHeightInBand = 0.62f;
  const float BossWidthRatio = 0.45f;
  const float ParadeLeft = 0.12f;
  const float ParadeRight = 0.88f;
  // 1コマ（16ドット）の横幅を、となりとの間かくの何割にするか。体は12ドットなので、これで重ならずに並ぶ
  const float SlimeWidthToGap = 0.9f;
  const float HopHeight = 0.25f;
  const float HopSpeed = 5f;
  // 着地の瞬間だけ、つぶれたコマにする
  const float SquashBelowHeight = 0.04f;
  const float BossFrameSeconds = 0.45f;
  const string SkinSpritesPath = "Sprites/slime_";
  const string NormalFrame = "slime 0";
  const string SquashFrame = "slime 1";
  const string AllySkin = "ally";
  static readonly string[] ParadeSkins = { "fire", "ice", AllySkin, "gold", "thunder", "bomb" };

  class Hopper
  {
    public SpriteRenderer Renderer;
    public Sprite Normal;
    public Sprite Squash;
    public float BaseY;
    public float Phase;
  }

  readonly List<Hopper> hoppers = new List<Hopper>();
  SpriteRenderer boss;
  Sprite bossFrameA;
  Sprite bossFrameB;

  void Start()
  {
    var camera = Camera.main;
    float height = camera.orthographicSize * 2f;
    float width = height * camera.aspect;
    FieldFloor.CreateCovering(camera);

    float bandBottom = ScreenYToWorld(camera, GlyphEdgeScreenY(startLabel, 1f));
    float bandTop = ScreenYToWorld(camera, GlyphEdgeScreenY(logo, -1f));
    float band = bandTop - bandBottom;
    CreateBoss(bandBottom + band * BossInBand, new Vector2(width * BossWidthRatio, band * BossHeightInBand));
    CreateParade(camera, width, bandBottom + band * ParadeInBand);
  }

  // 文字は上下の真ん中にそろえてあるので、真ん中から見た目の高さの半分だけ上（direction=1）か下（-1）が文字の端
  static float GlyphEdgeScreenY(Text text, float direction)
  {
    return text.rectTransform.position.y + direction * text.preferredHeight * text.canvas.scaleFactor / 2f;
  }

  static float ScreenYToWorld(Camera camera, float screenY)
  {
    return camera.ScreenToWorldPoint(new Vector3(0f, screenY, 0f)).y;
  }

  void CreateBoss(float bottomY, Vector2 maxSize)
  {
    var bottomPivot = new Vector2(0.5f, 0f);
    bossFrameA = PixelArt.WorldSprite("boss", bottomPivot);
    bossFrameB = PixelArt.WorldSprite("boss_b", bottomPivot);
    boss = new GameObject("TitleBoss").AddComponent<SpriteRenderer>();
    boss.sprite = bossFrameA;
    Vector2 bossSize = bossFrameA.bounds.size;
    float scale = Mathf.Min(maxSize.x / bossSize.x, maxSize.y / bossSize.y);
    boss.transform.localScale = Vector3.one * scale;
    boss.transform.position = new Vector3(Camera.main.transform.position.x, bottomY, 0f);
  }

  void CreateParade(Camera camera, float screenWidth, float bottomY)
  {
    float gap = screenWidth * (ParadeRight - ParadeLeft) / (ParadeSkins.Length - 1);
    for (int i = 0; i < ParadeSkins.Length; i++)
    {
      var frames = new Dictionary<string, Sprite>();
      foreach (var sprite in Resources.LoadAll<Sprite>(SkinSpritesPath + ParadeSkins[i]))
      {
        frames[sprite.name] = sprite;
      }
      Vector2 frameSize = frames[NormalFrame].bounds.size;
      float scale = gap * SlimeWidthToGap / frameSize.x;
      float x = camera.ViewportToWorldPoint(new Vector3(Mathf.Lerp(ParadeLeft, ParadeRight, i / (ParadeSkins.Length - 1f)), 0f, 0f)).x;
      var renderer = new GameObject("TitleSlime_" + ParadeSkins[i]).AddComponent<SpriteRenderer>();
      renderer.transform.localScale = Vector3.one * scale;
      var position = new Vector3(x, bottomY, 0f);
      if (ParadeSkins[i] == AllySkin)
      {
        // 仲間の絵は、180度回して置くプレハブに合わせて逆さに保存してあるので、ここでも回して立たせる（足元が上にくる分を持ち上げる）
        renderer.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        position.y += frameSize.y * scale;
      }
      renderer.transform.position = position;
      hoppers.Add(new Hopper
      {
        Renderer = renderer,
        Normal = frames[NormalFrame],
        Squash = frames[SquashFrame],
        BaseY = position.y,
        // となり同士が少しずつずれて跳ねる、ウェーブのような動き
        Phase = i * 0.6f,
      });
    }
  }

  void Update()
  {
    float time = Time.unscaledTime;
    boss.sprite = Mathf.FloorToInt(time / BossFrameSeconds) % 2 == 0 ? bossFrameA : bossFrameB;
    foreach (var hopper in hoppers)
    {
      float lift = Mathf.Abs(Mathf.Sin(time * HopSpeed + hopper.Phase)) * HopHeight;
      hopper.Renderer.sprite = lift < SquashBelowHeight ? hopper.Squash : hopper.Normal;
      Vector3 position = hopper.Renderer.transform.position;
      hopper.Renderer.transform.position = PixelArt.Snap(new Vector3(position.x, hopper.BaseY + lift, 0f));
    }
  }
}
