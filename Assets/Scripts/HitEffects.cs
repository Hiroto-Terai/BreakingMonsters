using System.Collections;
using System.Collections.Generic;
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

  // ドメインリロード無効時もエディタ再生ごとに初期値から始めるため
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetStatics()
  {
    nextFreezeAllowedAt = 0f;
    shake = null;
    shakeDots = 0;
  }

  // シーンが切り替わるとこの入れ物ごとコルーチンが消えるので、止めた時間や揺れの途中状態を元に戻す
  void OnDestroy()
  {
    if (Time.timeScale > 0f && Time.timeScale < 1f)
    {
      Time.timeScale = 1f;
    }
    shake = null;
    shakeDots = 0;
  }

  static float SteppedFade(float progress)
  {
    return 1f - Mathf.Floor(progress * FadeSteps) / FadeSteps;
  }

  // 破片や残像は1回倒すだけで十数個出るので、作っては消すのをやめて使い回す（スマホで動きが引っかからないようにする）。
  // 入れ物の子にしておくので、シーンが切り替わると入れ物と一緒に消える
  readonly Stack<SpriteRenderer> idleRenderers = new Stack<SpriteRenderer>();

  static SpriteRenderer TakeRenderer()
  {
    var pool = Runner.idleRenderers;
    var spriteRenderer = pool.Count > 0 ? pool.Pop() : CreatePooledRenderer();
    spriteRenderer.gameObject.SetActive(true);
    return spriteRenderer;
  }

  static SpriteRenderer CreatePooledRenderer()
  {
    var spriteRenderer = new GameObject("EffectPiece").AddComponent<SpriteRenderer>();
    spriteRenderer.transform.SetParent(Runner.transform, false);
    return spriteRenderer;
  }

  static void ReturnRenderer(SpriteRenderer spriteRenderer)
  {
    spriteRenderer.gameObject.SetActive(false);
    Runner.idleRenderers.Push(spriteRenderer);
  }

  public static void Burst(Vector3 position, Color color)
  {
    for (int i = 0; i < BurstPieceCount; i++)
    {
      float angle = (360f / BurstPieceCount * i + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
      Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * BurstSpeed * Random.Range(0.6f, 1.2f);
      Runner.StartCoroutine(MovePiece(position, velocity, color, BurstSeconds));
    }
  }

  // たくさん倒したときは破片も増やして、画面をにぎやかにする
  public static void BigBurst(Vector3 position, Color color, int pieces)
  {
    for (int i = 0; i < pieces; i++)
    {
      float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
      Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * BurstSpeed * Random.Range(0.5f, 1.8f);
      Runner.StartCoroutine(MovePiece(position, velocity, color, BurstSeconds * Random.Range(0.8f, 1.5f)));
    }
  }

  static IEnumerator MovePiece(Vector3 position, Vector3 velocity, Color color, float seconds)
  {
    var spriteRenderer = TakeRenderer();
    var piece = spriteRenderer.transform;
    piece.localScale = Vector3.one * BurstPieceDots;
    spriteRenderer.sprite = PixelArt.WorldSprite("dot");
    spriteRenderer.sortingOrder = 10;
    float time = 0f;
    while (time < seconds)
    {
      time += Time.deltaTime;
      position += velocity * Time.deltaTime;
      piece.position = PixelArt.Snap(position);
      spriteRenderer.color = new Color(color.r, color.g, color.b, SteppedFade(time / seconds));
      yield return null;
    }
    ReturnRenderer(spriteRenderer);
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
      Runner.StartCoroutine(MovePiece(point, Vector3.zero, color, BurstSeconds));
    }
  }

  // ボールの通った跡に、薄いコピーを少しだけ残す
  public static void Afterimage(SpriteRenderer source, Vector3 position, Color color)
  {
    Runner.StartCoroutine(ShowAfterimage(source, position, color));
  }

  static IEnumerator ShowAfterimage(SpriteRenderer source, Vector3 position, Color color)
  {
    var spriteRenderer = TakeRenderer();
    spriteRenderer.transform.position = PixelArt.Snap(position);
    spriteRenderer.transform.localScale = source.transform.lossyScale;
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
    ReturnRenderer(spriteRenderer);
  }

  public static void Shake()
  {
    Shake(ShakeMaxDots, ShakeSeconds);
  }

  // 揺れの強さ（ドット数）と長さを選べる。揺れている最中に強い揺れが来たら、そちらで上書きする
  public static void Shake(int maxDots, float seconds)
  {
    if (shake != null)
    {
      if (maxDots <= shakeDots)
      {
        return;
      }
      Runner.StopCoroutine(shake);
      Camera.main.transform.position = shakeOrigin;
    }
    shakeDots = maxDots;
    shakeOrigin = Camera.main.transform.position;
    shake = Runner.StartCoroutine(ShakeCamera(Camera.main.transform, maxDots, seconds));
  }

  static Coroutine shake;
  static int shakeDots;
  static Vector3 shakeOrigin;

  static IEnumerator ShakeCamera(Transform cameraTransform, int maxDots, float seconds)
  {
    Vector3 origin = shakeOrigin;
    float time = 0f;
    // ヒットストップ中も揺らしたいので、実時間で進める
    while (time < seconds)
    {
      time += Time.unscaledDeltaTime;
      int dots = Mathf.CeilToInt(maxDots * (1f - time / seconds));
      Vector3 offset = new Vector3(Random.Range(-dots, dots + 1), Random.Range(-dots, dots + 1), 0f) * PixelArt.WorldPixelSize;
      cameraTransform.position = origin + offset;
      yield return null;
    }
    cameraTransform.position = origin;
    shake = null;
    shakeDots = 0;
  }

  const float FreezeTimeScale = 0.02f;
  const float FreezeCooldownSeconds = 0.25f;
  static float nextFreezeAllowedAt;

  // ヒットストップ：一瞬だけ時間をほぼ止めて、当たりの重さを出す。一時停止やスロー中は何もしない
  public static void Freeze(float seconds)
  {
    // 続けて止めるとカクカクして見えるので、止めたあとは少し間をあける
    if (Time.timeScale != 1f || Time.unscaledTime < nextFreezeAllowedAt)
    {
      return;
    }
    nextFreezeAllowedAt = Time.unscaledTime + seconds + FreezeCooldownSeconds;
    Time.timeScale = FreezeTimeScale;
    Runner.StartCoroutine(RestoreTime(seconds, FreezeTimeScale));
  }

  // スローモーション（最後の1体を倒したときなど）
  public static void SlowMotion(float timeScale, float seconds)
  {
    if (Time.timeScale != 1f)
    {
      return;
    }
    Time.timeScale = timeScale;
    Runner.StartCoroutine(RestoreTime(seconds, timeScale));
  }

  // 途中で一時停止などに変わっていたら、そちらを優先して戻さない
  static IEnumerator RestoreTime(float seconds, float expectedTimeScale)
  {
    yield return new WaitForSecondsRealtime(seconds);
    if (Mathf.Approximately(Time.timeScale, expectedTimeScale))
    {
      Time.timeScale = 1f;
    }
  }

  const int RingPieceCount = 16;
  const float RingSeconds = 0.25f;

  // 爆発の輪：中心から外へ広がるドットの輪
  public static void Ring(Vector3 center, float radius, Color color)
  {
    for (int i = 0; i < RingPieceCount; i++)
    {
      float angle = 360f / RingPieceCount * i * Mathf.Deg2Rad;
      Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (radius / RingSeconds);
      Runner.StartCoroutine(MovePiece(center, velocity, color, RingSeconds));
    }
  }

  const float FlashSeconds = 0.12f;
  static SpriteRenderer flashOverlay;

  // 画面全体を一瞬光らせる（レベルアップ・進化・フィーバー）
  public static void Flash(Color color)
  {
    if (flashOverlay == null)
    {
      flashOverlay = new GameObject("ScreenFlash").AddComponent<SpriteRenderer>();
      flashOverlay.sprite = PixelArt.WorldSprite("dot");
      flashOverlay.sortingOrder = 50;
      // ドット1つを画面より大きく引き伸ばして全体を覆う
      flashOverlay.transform.localScale = Vector3.one * 2000f;
    }
    flashOverlay.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0f);
    Runner.StartCoroutine(FadeFlash(color));
  }

  static IEnumerator FadeFlash(Color color)
  {
    float time = 0f;
    while (time < FlashSeconds)
    {
      time += Time.unscaledDeltaTime;
      flashOverlay.color = new Color(color.r, color.g, color.b, color.a * SteppedFade(time / FlashSeconds));
      yield return null;
    }
    flashOverlay.color = Color.clear;
  }

  public static void Vibrate()
  {
#if UNITY_IOS || UNITY_ANDROID
    Handheld.Vibrate();
#endif
  }
}
