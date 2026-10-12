using UnityEngine;
using UnityEngine.UI;

// メニュー画面の各モードボタンの下に、ベスト記録（星・スコア）を表示する
public class MenuRecords : MonoBehaviour
{
  const int RecordFontSize = 44;
  const float RecordGapY = 16f;

  void Start()
  {
    foreach (var mode in FindObjectsByType<SelectMode>(FindObjectsSortMode.None))
    {
      ShowRecord(mode);
    }
  }

  void ShowRecord(SelectMode mode)
  {
    var button = mode.GetComponent<RectTransform>();
    var buttonText = mode.GetComponentInChildren<Text>();
    int stars = GameRecords.GetBestStars(mode.nextScene);
    int score = GameRecords.GetBestScore(mode.nextScene);

    var recordObject = new GameObject("Record_" + mode.nextScene, typeof(RectTransform));
    recordObject.transform.SetParent(button.parent, false);
    // オプション画面の暗幕より奥に描かれるよう、ボタンのすぐ後ろの順番に置く
    recordObject.transform.SetSiblingIndex(button.GetSiblingIndex() + 1);
    var record = recordObject.AddComponent<Text>();
    record.font = buttonText.font;
    record.fontSize = RecordFontSize;
    record.alignment = TextAnchor.UpperCenter;
    record.color = new Color(0.55f, 0.85f, 1f);
    record.horizontalOverflow = HorizontalWrapMode.Overflow;
    record.verticalOverflow = VerticalWrapMode.Overflow;
    record.raycastTarget = false;
    record.text = score > 0 ? GameRecords.StarsText(stars) + "\n" + score : "NEW";

    var rect = record.rectTransform;
    rect.anchorMin = button.anchorMin;
    rect.anchorMax = button.anchorMax;
    rect.pivot = new Vector2(0.5f, 1f);
    rect.sizeDelta = new Vector2(button.rect.width, 80f);
    rect.anchoredPosition = button.anchoredPosition + new Vector2(0f, -(button.rect.height * (1f - button.pivot.y) + RecordGapY));
  }
}
