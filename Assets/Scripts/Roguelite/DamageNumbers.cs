using System.Collections;
using UnityEngine;

// 当たった場所にダメージの数字をドットで出す（ヴァンサバのように、たくさん出るほど気持ちいい）
public class DamageNumbers : MonoBehaviour
{
  const float DigitWidth = 0.2f;
  const float LifeSeconds = 0.55f;
  const float RiseDistance = 0.45f;
  const int FadeSteps = 3;
  // 一度に出せる数の上限（多すぎると重くなり、読めなくもなる）
  const int MaxActive = 40;
  const int NumberSortingOrder = 20;

  static DamageNumbers runner;
  static int activeCount;

  static DamageNumbers Runner
  {
    get
    {
      if (runner == null)
      {
        runner = new GameObject("DamageNumbers").AddComponent<DamageNumbers>();
        activeCount = 0;
      }
      return runner;
    }
  }

  // isBig はかいしんなど目立たせたい数字（2倍の大きさ）
  public static void Spawn(Vector3 position, int value, Color color, bool isBig)
  {
    if (activeCount >= MaxActive || value <= 0)
    {
      return;
    }
    activeCount++;
    Runner.StartCoroutine(Show(position, value, color, isBig ? 2f : 1f));
  }

  static IEnumerator Show(Vector3 position, int value, Color color, float scale)
  {
    var root = new GameObject("DamageNumber").transform;
    string digits = value.ToString();
    var renderers = new SpriteRenderer[digits.Length];
    float width = DigitWidth * scale;
    for (int i = 0; i < digits.Length; i++)
    {
      var digit = new GameObject("Digit").AddComponent<SpriteRenderer>();
      digit.sprite = PixelArt.WorldSprite("digit_" + digits[i]);
      digit.sortingOrder = NumberSortingOrder;
      digit.transform.SetParent(root, false);
      digit.transform.localScale = Vector3.one * scale;
      digit.transform.localPosition = new Vector3((i - (digits.Length - 1) / 2f) * width, 0f, 0f);
      renderers[i] = digit;
    }
    // 重なって読めなくならないよう、少し横にずらす
    Vector3 start = position + new Vector3(Random.Range(-0.15f, 0.15f), 0.2f, 0f);
    float time = 0f;
    while (time < LifeSeconds)
    {
      time += Time.unscaledDeltaTime;
      float progress = time / LifeSeconds;
      root.position = PixelArt.Snap(start + Vector3.up * RiseDistance * Mathf.Sqrt(progress));
      float alpha = 1f - Mathf.Floor(progress * FadeSteps) / FadeSteps;
      foreach (var renderer in renderers)
      {
        renderer.color = new Color(color.r, color.g, color.b, alpha);
      }
      yield return null;
    }
    Destroy(root.gameObject);
    activeCount--;
  }
}
