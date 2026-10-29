using UnityEngine;
using UnityEngine.Events;

// 全滅したときに「広告を見て ふっかつ」するか聞く画面
public class RevivePanel
{
  const float PanelWidth = 1150f;
  const float PanelHeight = 900f;
  const float ButtonWidth = 900f;
  const float ButtonHeight = 150f;

  readonly RectTransform root;

  public RevivePanel(Transform canvas, UnityAction onWatchAd, UnityAction onGiveUp)
  {
    root = PixelUI.Stretch(PixelUI.CreateRect("RevivePanel", canvas));
    PixelUI.CreateDimmer(root, 0.7f);
    var panel = PixelUI.CreatePanel(root, PixelArt.Navy, PixelArt.Green);
    var center = new Vector2(0.5f, 0.5f);
    PixelUI.Place(panel.rectTransform, center, Vector2.zero, new Vector2(PanelWidth, PanelHeight));

    var title = PixelUI.CreateText(panel.transform, "ぜんめつ…", PixelUI.LargeFont, PixelArt.Red);
    PixelUI.Place(title.rectTransform, center, new Vector2(0f, 320f), new Vector2(PanelWidth, 110f));
    var message = PixelUI.CreateText(panel.transform, "広告を見ると\nなかま1匹で ふっかつできる", PixelUI.MediumFont, PixelArt.Pale);
    PixelUI.Place(message.rectTransform, center, new Vector2(0f, 140f), new Vector2(PanelWidth, 180f));

    var watch = PixelUI.CreateButton(panel.transform, "広告を見て ふっかつ", PixelUI.MediumFont, PixelArt.Teal, onWatchAd);
    PixelUI.Place((RectTransform)watch.transform, center, new Vector2(0f, -100f), new Vector2(ButtonWidth, ButtonHeight));
    var giveUp = PixelUI.CreateButton(panel.transform, "あきらめる", PixelUI.MediumFont, PixelArt.Gray, onGiveUp);
    PixelUI.Place((RectTransform)giveUp.transform, center, new Vector2(0f, -290f), new Vector2(ButtonWidth, ButtonHeight));
  }

  public void Close()
  {
    Object.Destroy(root.gameObject);
  }
}
