using System.Collections;
using UnityEngine;

// 当たったときの演出（ドットの破片・画面の揺れ・残像・振動）。動きはすべてドットの格子にそろえる
public class HitEffects : MonoBehaviour
{
  const int BurstPieceCount = 10;
  // 破片は2x2ドットの四角
  const float BurstPieceDots = 2f;
  const float BurstSpeed = 3f;
  const float BurstSeconds = 0.4f;
  // 色の薄れ方も段階的にする
  const int FadeSteps = 3;
  const float ShakeSeconds = 0.25f;
  const int ShakeMaxDots = 3;
  const float AfterimageSeconds = 0.12f;
  const float AfterimageAlpha = 0.45f;

  static HitEffects runner;

  // コルーチンを動かすための入れ物を、必要になったときにシーンへ1つだけ作る
  static HitEffects Runner
  {
    get
    {
      if (runner == null)
      {
        runner = new GameObject("HitEffects").AddComponent<HitEffects>();
      }
      return runner;
    }
  }

  static float SteppedFade(float progress)
  {
    return 1f - Mathf.Floor(progress * FadeSteps) / FadeSteps;
  }

  public static void Burst(Vector3 position, Color color)
  {
    for (int i = 0; i < BurstPieceCount; i++)
    {
      float angle = (360f / BurstPieceCount * i + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
      Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * BurstSpeed * Random.Range(0.6f, 1.2f);
      Runner.StartCoroutine(MovePiece(position, velocity, color));
    }
  }

  static IEnumerator MovePiece(Vector3 position, Vector3 velocity, Color color)
  {
    var piece = new GameObject("BurstPiece");
    piece.transform.localScale = Vector3.one * BurstPieceDots;
    var spriteRenderer = piece.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("dot");
    spriteRenderer.sortingOrder = 10;
    float time = 0f;
    while (time < BurstSeconds)
    {
      time += Time.deltaTime;
      position += velocity * Time.deltaTime;
      piece.transform.position = PixelArt.Snap(position);
      spriteRenderer.color = new Color(color.r, color.g, color.b, SteppedFade(time / BurstSeconds));
      yield return null;
    }
    Destroy(piece);
  }

  // 2点の間にドットの線を一瞬だけ出す（かみなりの連鎖など）
  public static void Line(Vector3 from, Vector3 to, Color color)
  {
    const float DotSpacing = 0.15f;
    int count = Mathf.Max(2, Mathf.CeilToInt(Vector3.Distance(from, to) / DotSpacing));
    for (int i = 0; i <= count; i++)
    {
      // 少しジグザグさせて稲妻らしくする
      Vector3 point = Vector3.Lerp(from, to, i / (float)count) + (Vector3)(Random.insideUnitCircle * 0.06f);
      Runner.StartCoroutine(MovePiece(point, Vector3.zero, color));
    }
  }

  // ボールの通った跡に、薄いコピーを少しだけ残す
  public static void Afterimage(SpriteRenderer source, Vector3 position, Color color)
  {
    Runner.StartCoroutine(ShowAfterimage(source, position, color));
  }

  static IEnumerator ShowAfterimage(SpriteRenderer source, Vector3 position, Color color)
  {
    var ghost = new GameObject("Afterimage");
    ghost.transform.position = PixelArt.Snap(position);
    ghost.transform.localScale = source.transform.lossyScale;
    var spriteRenderer = ghost.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = source.sprite;
    spriteRenderer.sortingOrder = source.sortingOrder - 1;
    float baseAlpha = AfterimageAlpha * source.color.a;
    float time = 0f;
    while (time < AfterimageSeconds)
    {
      time += Time.deltaTime;
      spriteRenderer.color = new Color(color.r, color.g, color.b, baseAlpha * SteppedFade(time / AfterimageSeconds));
      yield return null;
    }
    Destroy(ghost);
  }

  public static void Shake()
  {
    Runner.StartCoroutine(ShakeCamera(Camera.main.transform));
  }

  static IEnumerator ShakeCamera(Transform cameraTransform)
  {
    Vector3 origin = cameraTransform.position;
    float time = 0f;
    while (time < ShakeSeconds)
    {
      time += Time.deltaTime;
      int dots = Mathf.CeilToInt(ShakeMaxDots * (1f - time / ShakeSeconds));
      Vector3 offset = new Vector3(Random.Range(-dots, dots + 1), Random.Range(-dots, dots + 1), 0f) * PixelArt.WorldPixelSize;
      cameraTransform.position = origin + offset;
      yield return null;
    }
    cameraTransform.position = origin;
  }

  public static void Vibrate()
  {
#if UNITY_IOS || UNITY_ANDROID
    Handheld.Vibrate();
#endif
  }
}
