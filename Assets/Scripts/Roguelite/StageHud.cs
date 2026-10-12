using UnityEngine;
using UnityEngine.UI;

// プレイ中の上部表示（ステージ番号・階層・コイン・ボスのHP）とポップアップ
public class StageHud
{
  const float HudHeight = 90f;
  const float BossBarWidth = 900f;
  const float BossBarHeight = 42f;
  const float BossBarOffsetY = -110f;
  const float CoinIconScale = 6f;
  const float PopupLift = 72f;

  readonly Text stageText;
  readonly Text coinText;
  readonly RectTransform bossBar;
  readonly RectTransform bossBarFill;
  readonly Text bossHpText;
  readonly RectTransform popupParent;

  // 一時停止ボタンと同じ高さに並べる。一時停止画面の暗幕より奥に描かれるよう、ボタンのすぐ後ろの順番に置く
  public StageHud(RectTransform pauseButton)
  {
    var parent = pauseButton.parent;
    popupParent = (RectTransform)parent;
    int order = pauseButton.GetSiblingIndex() + 1;
    Vector2 anchor = new Vector2(0.5f, pauseButton.anchorMin.y);
    float y = pauseButton.anchoredPosition.y;

    stageText = PixelUI.CreateText(parent, "", PixelUI.MediumFont, PixelArt.White);
    PixelUI.Place(stageText.rectTransform, anchor, new Vector2(0f, y), new Vector2(800f, HudHeight));
    stageText.transform.SetSiblingIndex(order);

    var coinIcon = PixelUI.CreateIcon(parent, "coin", CoinIconScale);
    PixelUI.Place(coinIcon.rectTransform, new Vector2(1f, anchor.y), new Vector2(-300f, y), coinIcon.rectTransform.sizeDelta);
    coinIcon.transform.SetSiblingIndex(order);
    coinText = PixelUI.CreateText(parent, "0", PixelUI.MediumFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinText.rectTransform, new Vector2(1f, anchor.y), new Vector2(-140f, y), new Vector2(240f, HudHeight));
    coinText.transform.SetSiblingIndex(order);

    var barFrame = PixelUI.CreatePanel(parent, PixelArt.DarkRed, PixelArt.Pale);
    bossBar = barFrame.rectTransform;
    PixelUI.Place(bossBar, anchor, new Vector2(0f, y + BossBarOffsetY), new Vector2(BossBarWidth, BossBarHeight));
    bossBar.SetSiblingIndex(order);
    var fill = PixelUI.CreatePanel(bossBar, PixelArt.Red, PixelArt.Red);
    bossBarFill = fill.rectTransform;
    bossBarFill.anchorMin = new Vector2(0f, 0f);
    bossBarFill.anchorMax = new Vector2(0f, 1f);
    bossBarFill.pivot = new Vector2(0f, 0.5f);
    bossBarFill.anchoredPosition = new Vector2(6f, 0f);
    bossHpText = PixelUI.CreateText(bossBar, "", PixelUI.SmallFont, PixelArt.White);
    PixelUI.Stretch(bossHpText.rectTransform);
    bossBar.gameObject.SetActive(false);
  }

  public void SetStage(int stage, int floorIndex, int floorCount)
  {
    string label = "STAGE " + stage;
    switch (StageDatabase.GetType(stage))
    {
      case StageType.Hard: label += " <color=#FF963C>HARD</color>"; break;
      case StageType.Boss: label += " <color=#FF5C5C>BOSS</color>"; break;
    }
    stageText.text = label + "  " + (floorIndex + 1) + "/" + floorCount;
  }

  public void SetCoins(int coins)
  {
    coinText.text = coins.ToString();
  }

  public void ShowBossHp(int hp, int maxHp)
  {
    bossBar.gameObject.SetActive(hp > 0);
    float ratio = maxHp > 0 ? (float)hp / maxHp : 0f;
    // ドット1つ(6ユニット)単位で減らす
    float width = Mathf.Round((BossBarWidth - 12f) * ratio / 6f) * 6f;
    bossBarFill.sizeDelta = new Vector2(width, -12f);
    bossHpText.text = "BOSS " + hp + "/" + maxHp;
  }

  public void HideBossHp()
  {
    bossBar.gameObject.SetActive(false);
  }

  // ワールド上の位置にポップアップ文字を出す
  public void Popup(string message, Vector3 worldPosition, Color color)
  {
    var canvas = popupParent.GetComponentInParent<Canvas>();
    Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
    RectTransformUtility.ScreenPointToLocalPointInRectangle(popupParent, screenPoint, eventCamera, out Vector2 localPoint);
    var popup = PixelUI.CreateText(popupParent, message, PixelUI.MediumFont, color);
    popup.horizontalOverflow = HorizontalWrapMode.Overflow;
    popup.rectTransform.anchoredPosition = localPoint;
    popup.gameObject.AddComponent<FloatingText>().Lift = PopupLift;
  }
}
