using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// レベルアップ時の「強化を3つから1つ選ぶ」画面。持っている強化も上に並べて、組み合わせを考えやすくする
public class UpgradeChoicePanel
{
  const float PanelWidth = 1060f;
  const float CardWidth = 980f;
  const float CardHeight = 220f;
  const float CardGap = 22f;
  const float IconScale = 11f;
  const float OwnedIconScale = 5f;
  const float OwnedIconGap = 110f;
  const float HeaderHeight = 330f;
  const float RerollAreaHeight = 150f;
  const float BottomMargin = 40f;
  static readonly Color32 EvolutionGold = new Color32(250, 190, 60, 255);
  static readonly Color32 EvolutionFill = new Color32(90, 50, 20, 255);

  readonly RectTransform root;

  public UpgradeChoicePanel(Transform canvas, string title, string subtitle, List<UpgradeDefinition> choices, int rerollsLeft, Action<UpgradeDefinition> onPick, Action onReroll)
  {
    root = PixelUI.Stretch(PixelUI.CreateRect("UpgradeChoicePanel", canvas));
    PixelUI.CreateDimmer(root, 0.75f);

    float cardsHeight = choices.Count * CardHeight + (choices.Count - 1) * CardGap;
    float panelHeight = HeaderHeight + cardsHeight + (rerollsLeft > 0 ? RerollAreaHeight : 0f) + BottomMargin;
    var panel = PixelUI.CreatePanel(root, PixelArt.Navy, PixelArt.Green);
    var center = new Vector2(0.5f, 0.5f);
    PixelUI.Place(panel.rectTransform, center, Vector2.zero, new Vector2(PanelWidth, panelHeight));

    float top = panelHeight / 2f;
    var titleText = PixelUI.CreateText(panel.transform, title, PixelUI.LargeFont, PixelArt.Green);
    PixelUI.Place(titleText.rectTransform, center, new Vector2(0f, top - 70f), new Vector2(PanelWidth, 90f));
    var subtitleText = PixelUI.CreateText(panel.transform, subtitle, PixelUI.SmallFont, PixelArt.Pale);
    PixelUI.Place(subtitleText.rectTransform, center, new Vector2(0f, top - 140f), new Vector2(PanelWidth, 60f));
    CreateOwnedRow(panel.transform, top - 235f);

    float y = top - HeaderHeight - CardHeight / 2f;
    foreach (var choice in choices)
    {
      CreateCard(panel.transform, choice, y, onPick);
      y -= CardHeight + CardGap;
    }

    if (rerollsLeft > 0)
    {
      var reroll = PixelUI.CreateButton(panel.transform, "えらびなおす (" + rerollsLeft + ")", PixelUI.SmallFont, PixelArt.Purple, () => onReroll());
      PixelUI.Place((RectTransform)reroll.transform, center, new Vector2(0f, -top + BottomMargin + RerollAreaHeight / 2f), new Vector2(560f, 100f));
    }
  }

  // 今持っている強化とレベル（何を伸ばせば進化できるか見えるように）
  static void CreateOwnedRow(Transform parent, float y)
  {
    var owned = RunState.OwnedUpgrades;
    if (owned.Count == 0)
    {
      var none = PixelUI.CreateText(parent, "まだ強化がない", PixelUI.TinyFont, PixelArt.Gray);
      PixelUI.Place(none.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(PanelWidth, 60f));
      return;
    }
    for (int i = 0; i < owned.Count; i++)
    {
      var definition = Upgrades.Get(owned[i]);
      float x = (i - (owned.Count - 1) / 2f) * OwnedIconGap;
      var icon = PixelUI.CreateIcon(parent, definition.Icon, OwnedIconScale);
      PixelUI.Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, y + 14f), icon.rectTransform.sizeDelta);
      string level = definition.IsEvolution ? "<color=#FFE066>★</color>" : "Lv" + RunState.GetUpgradeLevel(definition.Id);
      var label = PixelUI.CreateText(parent, level, PixelUI.TinyFont, PixelArt.Pale);
      PixelUI.Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, y - 34f), new Vector2(OwnedIconGap, 30f));
    }
  }

  void CreateCard(Transform parent, UpgradeDefinition choice, float y, Action<UpgradeDefinition> onPick)
  {
    bool isEvolution = choice.IsEvolution;
    var card = PixelUI.CreateButton(parent, "", PixelUI.SmallFont, isEvolution ? EvolutionFill : PixelArt.Teal, () => onPick(choice));
    card.name = "Card_" + choice.Id;
    if (isEvolution)
    {
      card.GetComponent<Image>().sprite = PixelArt.FrameSprite(EvolutionFill, EvolutionGold);
    }
    var rect = PixelUI.Place((RectTransform)card.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(CardWidth, CardHeight));

    var icon = PixelUI.CreateIcon(rect, choice.Icon, IconScale);
    PixelUI.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(105f, 0f), icon.rectTransform.sizeDelta);

    var name = PixelUI.CreateText(rect, choice.Name + " " + LevelTag(choice), PixelUI.MediumFont, PixelArt.White, TextAnchor.MiddleLeft);
    PixelUI.Place(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(570f, 60f), new Vector2(740f, 64f));
    string description = choice.Describe(RunState.GetUpgradeLevel(choice.Id) + 1);
    var descriptionText = PixelUI.CreateText(rect, description, PixelUI.SmallFont, PixelArt.Pale, TextAnchor.MiddleLeft);
    PixelUI.Place(descriptionText.rectTransform, new Vector2(0f, 0.5f), new Vector2(570f, 0f), new Vector2(740f, 52f));
    // 進化につながる強化には、組み合わせの相手を書いて「ねらって組む」楽しさを出す
    string hint = isEvolution
      ? Upgrades.Get(choice.EvolvesFrom).Name + " + " + Upgrades.Get(choice.Partner).Name + " の進化"
      : Upgrades.EvolutionHint(choice);
    if (hint != null)
    {
      var hintText = PixelUI.CreateText(rect, hint, PixelUI.SmallFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
      PixelUI.Place(hintText.rectTransform, new Vector2(0f, 0.5f), new Vector2(570f, -58f), new Vector2(740f, 52f));
    }
  }

  // 「NEW!」「Lv2→3」「進化!」のどれか
  static string LevelTag(UpgradeDefinition choice)
  {
    if (choice.IsEvolution) return "<color=#FAC03C>進化!</color>";
    if (choice.Id == UpgradeId.Heal) return "";
    int level = RunState.GetUpgradeLevel(choice.Id);
    if (level == 0) return "<color=#6EDC78>NEW!</color>";
    string next = level + 1 >= choice.MaxLevel ? "MAX" : (level + 1).ToString();
    return "<color=#FFE066>Lv" + level + "→" + next + "</color>";
  }

  public void Close()
  {
    UnityEngine.Object.Destroy(root.gameObject);
  }
}
