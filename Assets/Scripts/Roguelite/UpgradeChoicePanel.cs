using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 階層クリア時に出す「強化を3つから1つ選ぶ」画面
public class UpgradeChoicePanel
{
  const float PanelWidth = 1200f;
  const float CardWidth = 1100f;
  const float CardHeight = 260f;
  const float CardGap = 30f;
  const float IconScale = 16f;

  readonly RectTransform root;

  public UpgradeChoicePanel(Transform canvas, string title, string subtitle, List<UpgradeDefinition> choices, int rerollsLeft, Action<UpgradeDefinition> onPick, Action onReroll)
  {
    root = PixelUI.Stretch(PixelUI.CreateRect("UpgradeChoicePanel", canvas));
    PixelUI.CreateDimmer(root, 0.75f);

    float cardsHeight = choices.Count * CardHeight + (choices.Count - 1) * CardGap;
    // えらびなおしボタンがあるときだけ下に場所を空ける
    float panelHeight = cardsHeight + (rerollsLeft > 0 ? 520f : 360f);
    var panel = PixelUI.CreatePanel(root, PixelArt.Navy, PixelArt.LightBlue);
    PixelUI.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelWidth, panelHeight));

    float top = panelHeight / 2f;
    var titleText = PixelUI.CreateText(panel.transform, title, PixelUI.LargeFont, PixelArt.Yellow);
    PixelUI.Place(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, top - 90f), new Vector2(PanelWidth, 100f));
    var subtitleText = PixelUI.CreateText(panel.transform, subtitle, PixelUI.MediumFont, PixelArt.Pale);
    PixelUI.Place(subtitleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, top - 190f), new Vector2(PanelWidth, 80f));

    float y = top - 280f - CardHeight / 2f;
    foreach (var choice in choices)
    {
      CreateCard(panel.transform, choice, y, onPick);
      y -= CardHeight + CardGap;
    }

    if (rerollsLeft > 0)
    {
      var reroll = PixelUI.CreateButton(panel.transform, "えらびなおす (" + rerollsLeft + ")", PixelUI.MediumFont, PixelArt.Purple, () => onReroll());
      PixelUI.Place((RectTransform)reroll.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -top + 100f), new Vector2(700f, 120f));
    }
  }

  void CreateCard(Transform parent, UpgradeDefinition choice, float y, Action<UpgradeDefinition> onPick)
  {
    var card = PixelUI.CreateButton(parent, "", PixelUI.MediumFont, PixelArt.Teal, () => onPick(choice));
    card.name = "Card_" + choice.Id;
    var rect = PixelUI.Place((RectTransform)card.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(CardWidth, CardHeight));

    var icon = PixelUI.CreateIcon(rect, choice.Icon, IconScale);
    PixelUI.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(130f, 0f), icon.rectTransform.sizeDelta);

    int nextLevel = RunState.GetUpgradeLevel(choice.Id) + 1;
    string levelText = choice.MaxLevel > 1 && choice.Id != UpgradeId.Heal ? "  <color=#FFE066>Lv" + nextLevel + "</color>" : "";
    var name = PixelUI.CreateText(rect, choice.Name + levelText, PixelUI.MediumFont, PixelArt.White, TextAnchor.MiddleLeft);
    PixelUI.Place(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(620f, 50f), new Vector2(760f, 80f));
    var description = PixelUI.CreateText(rect, choice.Description, PixelUI.SmallFont, PixelArt.Pale, TextAnchor.MiddleLeft);
    PixelUI.Place(description.rectTransform, new Vector2(0f, 0.5f), new Vector2(620f, -50f), new Vector2(760f, 80f));
  }

  public void Close()
  {
    UnityEngine.Object.Destroy(root.gameObject);
  }
}
