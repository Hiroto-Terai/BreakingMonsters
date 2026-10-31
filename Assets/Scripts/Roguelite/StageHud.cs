using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// プレイ中の表示（ステージ番号・コイン・経験値・コンボ・ボスのHP・持っている強化）とポップアップ
public class StageHud
{
  const float HudHeight = 70f;
  const float BossBarWidth = 760f;
  const float BossBarHeight = 36f;
  const float BossBarOffsetY = -70f;
  const float CoinIconScale = 5f;
  const float PopupLift = 72f;
  const float XpBarWidth = 1000f;
  const float XpBarHeight = 18f;
  const float XpBarOffsetY = -52f;
  // 一時停止ボタンと重ならないよう、経験値バーは右に寄せる
  const float XpBarCenterX = 100f;
  const float LevelTextWidth = 100f;
  const float ComboTextWidth = 900f;
  const float ComboBumpScale = 1.35f;
  const float ComboBumpSeconds = 0.12f;
  const float OwnedIconScale = 4f;
  const float OwnedIconGap = 96f;
  // 持っている強化は、仲間スライムの下の空いている所に並べる
  const float OwnedRowWorldY = -4.42f;
  const int MinComboToShow = 3;

  readonly Text stageText;
  readonly Text coinText;
  readonly Text levelText;
  readonly RectTransform xpFill;
  readonly RectTransform bossBar;
  readonly RectTransform bossBarFill;
  readonly Text bossHpText;
  readonly RectTransform popupParent;
  readonly Text comboText;
  readonly Text introTitle;
  readonly Text introMessage;
  readonly RectTransform ownedRow;
  float comboBumpTimer;

  // 一時停止ボタンと同じ高さに並べる。一時停止画面の暗幕より奥に描かれるよう、ボタンのすぐ後ろの順番に置く
  public StageHud(RectTransform pauseButton)
  {
    var parent = pauseButton.parent;
    popupParent = (RectTransform)parent;
    int order = pauseButton.GetSiblingIndex() + 1;
    Vector2 anchor = new Vector2(0.5f, pauseButton.anchorMin.y);
    // ボタンの基準点が端にあっても、文字はボタンの縦の真ん中にそろえる
    float y = pauseButton.anchoredPosition.y - pauseButton.sizeDelta.y * pauseButton.localScale.y * (pauseButton.pivot.y - 0.5f);

    stageText = PixelUI.CreateText(parent, "", PixelUI.SmallFont, PixelArt.White);
    PixelUI.Place(stageText.rectTransform, anchor, new Vector2(0f, y), new Vector2(700f, HudHeight));
    stageText.transform.SetSiblingIndex(order);

    var coinIcon = PixelUI.CreateIcon(parent, "coin", CoinIconScale);
    PixelUI.Place(coinIcon.rectTransform, new Vector2(1f, anchor.y), new Vector2(-260f, y), coinIcon.rectTransform.sizeDelta);
    coinIcon.transform.SetSiblingIndex(order);
    coinText = PixelUI.CreateText(parent, "0", PixelUI.SmallFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinText.rectTransform, new Vector2(1f, anchor.y), new Vector2(-120f, y), new Vector2(220f, HudHeight));
    coinText.transform.SetSiblingIndex(order);

    // 経験値バー：たまるほど右に伸び、いっぱいでレベルアップ
    var xpFrame = PixelUI.CreatePanel(parent, PixelArt.Navy, PixelArt.Gray);
    PixelUI.Place(xpFrame.rectTransform, anchor, new Vector2(XpBarCenterX, y + XpBarOffsetY), new Vector2(XpBarWidth, XpBarHeight));
    xpFrame.transform.SetSiblingIndex(order);
    var fill = PixelUI.CreatePanel(xpFrame.rectTransform, PixelArt.Green, PixelArt.Green);
    xpFill = fill.rectTransform;
    xpFill.anchorMin = new Vector2(0f, 0f);
    xpFill.anchorMax = new Vector2(0f, 1f);
    xpFill.pivot = new Vector2(0f, 0.5f);
    xpFill.anchoredPosition = new Vector2(4f, 0f);
    levelText = PixelUI.CreateText(parent, "Lv1", PixelUI.SmallFont, PixelArt.Green, TextAnchor.MiddleRight);
    PixelUI.Place(levelText.rectTransform, anchor, new Vector2(XpBarCenterX - XpBarWidth / 2f - LevelTextWidth / 2f - 10f, y + XpBarOffsetY), new Vector2(LevelTextWidth, 50f));
    levelText.transform.SetSiblingIndex(order);

    var barFrame = PixelUI.CreatePanel(parent, PixelArt.DarkRed, PixelArt.Pale);
    bossBar = barFrame.rectTransform;
    PixelUI.Place(bossBar, anchor, new Vector2(0f, y + XpBarOffsetY + BossBarOffsetY), new Vector2(BossBarWidth, BossBarHeight));
    bossBar.SetSiblingIndex(order);
    var bossFill = PixelUI.CreatePanel(bossBar, PixelArt.Red, PixelArt.Red);
    bossBarFill = bossFill.rectTransform;
    bossBarFill.anchorMin = new Vector2(0f, 0f);
    bossBarFill.anchorMax = new Vector2(0f, 1f);
    bossBarFill.pivot = new Vector2(0f, 0.5f);
    bossBarFill.anchoredPosition = new Vector2(6f, 0f);
    bossHpText = PixelUI.CreateText(bossBar, "", PixelUI.SmallFont, PixelArt.White);
    PixelUI.Stretch(bossHpText.rectTransform);
    bossBar.gameObject.SetActive(false);

    // コンボ数：フィールドのまん中より少し上に大きく出す
    comboText = PixelUI.CreateText(parent, "", PixelUI.LargeFont, PixelArt.Yellow);
    comboText.horizontalOverflow = HorizontalWrapMode.Overflow;
    PixelUI.Place(comboText.rectTransform, new Vector2(0.5f, 0.5f), WorldToCanvas(new Vector3(0f, 0.6f, 0f)), new Vector2(ComboTextWidth, 120f));
    comboText.transform.SetSiblingIndex(order);

    // ステージの最初の紹介（カウントダウンの間だけ出す）
    introTitle = PixelUI.CreateText(parent, "", PixelUI.LargeFont, PixelArt.White);
    introTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
    PixelUI.Place(introTitle.rectTransform, new Vector2(0.5f, 0.5f), WorldToCanvas(new Vector3(0f, -0.4f, 0f)), new Vector2(ComboTextWidth, 100f));
    introTitle.transform.SetSiblingIndex(order);
    introMessage = PixelUI.CreateText(parent, "", PixelUI.SmallFont, PixelArt.Yellow);
    PixelUI.Place(introMessage.rectTransform, new Vector2(0.5f, 0.5f), WorldToCanvas(new Vector3(0f, -1.05f, 0f)), new Vector2(1100f, 140f));
    introMessage.transform.SetSiblingIndex(order);

    ownedRow = PixelUI.CreateRect("OwnedUpgrades", parent);
    PixelUI.Place(ownedRow, new Vector2(0.5f, 0.5f), WorldToCanvas(new Vector3(0f, OwnedRowWorldY, 0f)), new Vector2(XpBarWidth, 80f));
    ownedRow.SetSiblingIndex(order);
  }

  // ワールド座標を、このキャンバスの中央基準の位置に変える
  Vector2 WorldToCanvas(Vector3 worldPosition)
  {
    var canvas = popupParent.GetComponentInParent<Canvas>().rootCanvas;
    var canvasRect = (RectTransform)canvas.transform;
    Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 local);
    return local;
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

  public void ShowIntro(string title, string message)
  {
    introTitle.text = title;
    introMessage.text = message ?? "";
  }

  public void HideIntro()
  {
    introTitle.text = "";
    introMessage.text = "";
  }

  public void SetCoins(int coins)
  {
    coinText.text = coins.ToString();
  }

  public void SetXp(int level, int xp, int xpToNext)
  {
    levelText.text = "Lv" + level;
    float ratio = xpToNext > 0 ? Mathf.Clamp01((float)xp / xpToNext) : 0f;
    // ドット1つ(6ユニット)単位で伸ばす
    float width = Mathf.Round((XpBarWidth - 8f) * ratio / 6f) * 6f;
    xpFill.sizeDelta = new Vector2(width, -8f);
  }

  public void SetCombo(int combo, bool isFever)
  {
    if (combo < MinComboToShow)
    {
      comboText.text = "";
      return;
    }
    comboText.text = combo + " COMBO" + (isFever ? "\n<color=#FF963C>FEVER!!</color>" : "");
    comboText.color = ComboColor(combo);
    comboBumpTimer = ComboBumpSeconds;
  }

  // コンボが伸びるほど、黄色 → オレンジ → 赤と熱い色になる
  static Color ComboColor(int combo)
  {
    if (combo >= 40) return PixelArt.Red;
    if (combo >= 15) return PixelArt.Orange;
    return PixelArt.Yellow;
  }

  // コンボ表示がふくらんで戻る動き
  public void Tick(float unscaledDeltaTime)
  {
    if (comboBumpTimer > 0f)
    {
      comboBumpTimer -= unscaledDeltaTime;
      float scale = comboBumpTimer > 0f ? Mathf.Lerp(1f, ComboBumpScale, comboBumpTimer / ComboBumpSeconds) : 1f;
      comboText.rectTransform.localScale = Vector3.one * scale;
    }
  }

  public void SetOwnedUpgrades(IReadOnlyList<UpgradeId> owned)
  {
    foreach (Transform child in ownedRow)
    {
      Object.Destroy(child.gameObject);
    }
    for (int i = 0; i < owned.Count; i++)
    {
      var definition = Upgrades.Get(owned[i]);
      float x = (i - (owned.Count - 1) / 2f) * OwnedIconGap;
      var icon = PixelUI.CreateIcon(ownedRow, definition.Icon, OwnedIconScale);
      PixelUI.Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, 8f), icon.rectTransform.sizeDelta);
      string level = definition.IsEvolution ? "<color=#FFE066>★</color>" : "Lv" + RunState.GetUpgradeLevel(definition.Id);
      var label = PixelUI.CreateText(ownedRow, level, PixelUI.TinyFont, PixelArt.Pale);
      PixelUI.Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, -32f), new Vector2(OwnedIconGap, 30f));
    }
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
    Popup(message, worldPosition, color, PixelUI.MediumFont);
  }

  public void Popup(string message, Vector3 worldPosition, Color color, int fontSize)
  {
    var canvas = popupParent.GetComponentInParent<Canvas>();
    Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
    RectTransformUtility.ScreenPointToLocalPointInRectangle(popupParent, screenPoint, eventCamera, out Vector2 localPoint);
    var popup = PixelUI.CreateText(popupParent, message, fontSize, color);
    popup.horizontalOverflow = HorizontalWrapMode.Overflow;
    popup.rectTransform.anchoredPosition = localPoint;
    popup.gameObject.AddComponent<FloatingText>().Lift = PopupLift;
  }
}
