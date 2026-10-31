using UnityEngine;

// 一時停止ボタンの位置。プレイ画面ではフィールドの上のHUDの帯に、それ以外はセーフエリアの左上に置く
public class OptionButtonPosition : MonoBehaviour
{
  const float MarginUnits = 30f;
  // 元のボタンは画面に対して大きいので、少し小さくする
  const float ButtonScale = 0.8f;

  void Start()
  {
    var rect = GetComponent<RectTransform>();
    rect.localScale = Vector3.one * ButtonScale;
    var canvas = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
    float unitsPerPixel = canvas.rect.height / Screen.height;
    Rect safe = Screen.safeArea;
    float x = safe.xMin * unitsPerPixel + MarginUnits;
    float topOffset;
    if (Playfield.IsFitted)
    {
      // ボタンの中心をHUDの帯の中心にそろえる（ボタンの基準点は左上）
      topOffset = (Screen.height - Playfield.HudCenterScreenY) * unitsPerPixel - rect.sizeDelta.y * ButtonScale / 2f;
    }
    else
    {
      topOffset = (Screen.height - safe.yMax) * unitsPerPixel + MarginUnits;
    }
    rect.anchoredPosition = new Vector2(x, -topOffset);
  }
}
