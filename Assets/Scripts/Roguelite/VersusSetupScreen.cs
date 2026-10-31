using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 対戦の準備画面：自分と相手のキャラクター、相手（CPU か 2人目）、CPU の強さを選ぶ
public class VersusSetupScreen
{
  const float CardWidth = 360f;
  const float CardHeight = 330f;
  const float CardGap = 30f;
  const float IconScale = 12f;
  const float FadeSeconds = 0.3f;

  static readonly string[] LevelNames = { "よわい", "ふつう", "つよい" };
  static readonly float[] LevelValues = { 0.3f, 0.6f, 1f };

  readonly RectTransform root;
  readonly Action onClose;
  readonly List<Action> refreshers = new List<Action>();

  public VersusSetupScreen(Transform canvas, Action onClose)
  {
    this.onClose = onClose;
    root = PixelUI.Stretch(PixelUI.CreateRect("VersusSetup", canvas));
    var background = PixelUI.CreateDimmer(root, 1f);
    background.color = PixelArt.Navy;
    var center = new Vector2(0.5f, 0.5f);

    var title = PixelUI.CreateText(root, "たいせん", PixelUI.LargeFont, PixelArt.LightBlue);
    PixelUI.Place(title.rectTransform, center, new Vector2(0f, 900f), new Vector2(900f, 100f));

    CreateSectionTitle("P1（下）のキャラ", 760f);
    CreateCharacterRow(560f, () => VersusSettings.Player1, id => VersusSettings.Player1 = id);

    CreateSectionTitle("あいて", 330f);
    CreateToggle(new Vector2(-230f, 220f), "CPU", () => VersusSettings.IsPlayer2Cpu, () => VersusSettings.IsPlayer2Cpu = true);
    CreateToggle(new Vector2(230f, 220f), "2人で1台", () => !VersusSettings.IsPlayer2Cpu, () => VersusSettings.IsPlayer2Cpu = false);

    CreateSectionTitle("あいて（上）のキャラ", 90f);
    CreateCharacterRow(-110f, () => VersusSettings.Player2, id => VersusSettings.Player2 = id);

    CreateSectionTitle("CPUのつよさ", -330f);
    for (int i = 0; i < LevelNames.Length; i++)
    {
      float value = LevelValues[i];
      CreateToggle(new Vector2((i - 1) * 400f, -440f), LevelNames[i], () => Mathf.Approximately(VersusSettings.CpuLevel, value), () => VersusSettings.CpuLevel = value);
    }

    var start = PixelUI.CreateButton(root, "スタート", PixelUI.LargeFont, PixelArt.Purple, StartMatch);
    PixelUI.Place((RectTransform)start.transform, center, new Vector2(0f, -680f), new Vector2(700f, 160f));
    var close = PixelUI.CreateButton(root, "とじる", PixelUI.MediumFont, PixelArt.Teal, Close);
    PixelUI.Place((RectTransform)close.transform, center, new Vector2(0f, -880f), new Vector2(400f, 120f));
    Refresh();
  }

  void CreateSectionTitle(string text, float y)
  {
    var label = PixelUI.CreateText(root, text, PixelUI.MediumFont, PixelArt.Pale);
    PixelUI.Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1200f, 80f));
  }

  void CreateCharacterRow(float y, Func<CharacterId> getSelected, Action<CharacterId> select)
  {
    for (int i = 0; i < Characters.All.Length; i++)
    {
      var character = Characters.All[i];
      var card = PixelUI.CreateButton(root, "", PixelUI.SmallFont, PixelArt.DarkTeal, () => { select(character.Id); Refresh(); });
      card.name = "Character_" + character.Id;
      var rect = PixelUI.Place((RectTransform)card.transform, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * (CardWidth + CardGap), y), new Vector2(CardWidth, CardHeight));
      var icon = PixelUI.CreateIcon(rect, character.Icon, IconScale);
      PixelUI.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -85f), icon.rectTransform.sizeDelta);
      var name = PixelUI.CreateText(rect, character.Name, PixelUI.SmallFont, PixelArt.White);
      PixelUI.Place(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -195f), new Vector2(CardWidth - 20f, 50f));
      var description = PixelUI.CreateText(rect, character.Description, 24, PixelArt.Pale);
      PixelUI.Place(description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -265f), new Vector2(CardWidth - 30f, 70f));
      var image = card.GetComponent<Image>();
      refreshers.Add(() => image.sprite = PixelArt.FrameSprite(getSelected() == character.Id ? PixelArt.Teal : PixelArt.DarkTeal, getSelected() == character.Id ? PixelArt.Yellow : PixelArt.Gray));
    }
  }

  void CreateToggle(Vector2 position, string label, Func<bool> isSelected, Action select)
  {
    var button = PixelUI.CreateButton(root, label, PixelUI.MediumFont, PixelArt.DarkTeal, () => { select(); Refresh(); });
    button.name = "Toggle_" + label;
    PixelUI.Place((RectTransform)button.transform, new Vector2(0.5f, 0.5f), position, new Vector2(380f, 120f));
    var image = button.GetComponent<Image>();
    refreshers.Add(() => image.sprite = PixelArt.FrameSprite(isSelected() ? PixelArt.Teal : PixelArt.DarkTeal, isSelected() ? PixelArt.Yellow : PixelArt.Gray));
  }

  void Refresh()
  {
    foreach (var refresh in refreshers)
    {
      refresh();
    }
  }

  void StartMatch()
  {
    if (FadeManager.Instance.IsFading)
    {
      return;
    }
    FadeManager.Instance.LoadScene("VersusPlay", FadeSeconds);
  }

  void Close()
  {
    UnityEngine.Object.Destroy(root.GetComponentInParent<Canvas>().gameObject);
    onClose();
  }
}
