using UnityEngine;

// 経験値のジェム。倒したモンスターから飛び散り、少しすると自分のバーに吸い寄せられて集まる
public class XpGem : MonoBehaviour
{
  const float PopSpeed = 2.5f;
  const float PopDrag = 4f;
  const float WaitBeforeAttractSeconds = 0.35f;
  const float AttractAcceleration = 30f;
  const float MaxAttractSpeed = 16f;
  const float CollectDistance = 0.35f;
  const int GemSortingOrder = 8;

  Vector3 velocity;
  // スナップ前のなめらかな位置（ドットにそろえた表示位置とは別に持つ）
  Vector3 position;
  float attractSpeed;
  float age;
  Transform target;
  System.Action onCollected;

  public static void Spawn(Vector3 position, Transform target, System.Action onCollected)
  {
    var gameObject = new GameObject("XpGem");
    gameObject.transform.position = position;
    var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = PixelArt.WorldSprite("gem");
    spriteRenderer.sortingOrder = GemSortingOrder;
    var gem = gameObject.AddComponent<XpGem>();
    gem.position = position;
    gem.target = target;
    gem.onCollected = onCollected;
    float angle = Random.Range(20f, 160f) * Mathf.Deg2Rad;
    gem.velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * PopSpeed * Random.Range(0.6f, 1.2f);
  }

  // 階層が終わったら、残っているジェムをまとめて回収させる
  public static void AttractAllNow()
  {
    foreach (var gem in FindObjectsByType<XpGem>(FindObjectsSortMode.None))
    {
      gem.age = WaitBeforeAttractSeconds;
    }
  }

  void Update()
  {
    // 階層クリア後のスロー中も止まって見えないよう、実時間で動かす
    float deltaTime = Time.unscaledDeltaTime;
    if (Time.timeScale == 0f || target == null)
    {
      return;
    }
    age += deltaTime;
    if (age < WaitBeforeAttractSeconds)
    {
      velocity = Vector3.Lerp(velocity, Vector3.zero, PopDrag * deltaTime);
    }
    else
    {
      // 回り込んで行き過ぎないよう、まっすぐ近づけながら速くしていく
      attractSpeed = Mathf.Min(attractSpeed + AttractAcceleration * deltaTime, MaxAttractSpeed);
      position = Vector3.MoveTowards(position, target.position, attractSpeed * deltaTime);
      if (Vector3.Distance(position, target.position) <= CollectDistance)
      {
        onCollected?.Invoke();
        Destroy(gameObject);
        return;
      }
      transform.position = PixelArt.Snap(position);
      return;
    }
    position += velocity * deltaTime;
    transform.position = PixelArt.Snap(position);
  }
}
