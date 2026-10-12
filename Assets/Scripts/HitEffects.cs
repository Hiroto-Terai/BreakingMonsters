using System.Collections;
using UnityEngine;

// 当たったときの演出（破片が飛び散る・画面の揺れ・振動）
public class HitEffects : MonoBehaviour
{
  const int BurstPieceCount = 10;
  const float BurstPieceSize = 0.12f;
  const float BurstSpeed = 3f;
  const float BurstSeconds = 0.4f;
  const float ShakeSeconds = 0.25f;
  const float ShakeStrength = 0.12f;

  static HitEffects runner;
  static Sprite pieceSprite;

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

  static Sprite PieceSprite
  {
    get
    {
      if (pieceSprite == null)
      {
        pieceSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
      }
      return pieceSprite;
    }
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
    piece.transform.position = position;
    piece.transform.localScale = Vector3.one * BurstPieceSize;
    var spriteRenderer = piece.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PieceSprite;
    spriteRenderer.sortingOrder = 10;
    float time = 0f;
    while (time < BurstSeconds)
    {
      time += Time.deltaTime;
      piece.transform.position += velocity * Time.deltaTime;
      spriteRenderer.color = new Color(color.r, color.g, color.b, 1f - time / BurstSeconds);
      yield return null;
    }
    Destroy(piece);
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
      float strength = ShakeStrength * (1f - time / ShakeSeconds);
      cameraTransform.position = origin + (Vector3)(Random.insideUnitCircle * strength);
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
