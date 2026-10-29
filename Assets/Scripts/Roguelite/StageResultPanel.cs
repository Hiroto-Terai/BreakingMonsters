using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// ステージクリア／ゲームオーバー時の結果画面
public class StageResultPanel
{
  const float PanelWidth = 1150f;
  const float PanelHeight = 1250f;
  const float StarScale = 18f;
  const float StarGap = 190f;
  const float ButtonWidth = 340f;
  const float ButtonHeight = 140f;
  const float DoubleCoinsButtonWidth = 640f;
  const float DoubleCoinsButtonHeight = 110f;

  Text coinsText;
  Text totalText;
  Button doubleCoinsButton;

  public struct Result
  {
    public bool IsClear;
    public int Stage;
    public int Stars;
    public int FloorsCleared;
    public int FloorCount;
    public int CoinsEarned;
  }

  public StageResultPanel(Transform canvas, Result result, UnityAction onNext, UnityAction onRetry, UnityAction onMap, UnityAction onDoubleCoins)
  {
    var root = PixelUI.Stretch(PixelUI.CreateRect("StageResultPanel", canvas));
    PixelUI.CreateDimmer(root, 0.7f);
    var panel = PixelUI.CreatePanel(root, PixelArt.Navy, result.IsClear ? PixelArt.Yellow : PixelArt.Red);
    PixelUI.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelWidth, PanelHeight));
    var center = new Vector2(0.5f, 0.5f);

    string title = result.IsClear ? "STAGE " + result.Stage + " CLEAR!" : "GAME OVER";
    var titleText = PixelUI.CreateText(panel.transform, title, PixelUI.LargeFont, result.IsClear ? PixelArt.Yellow : PixelArt.Red);
    PixelUI.Place(titleText.rectTransform, center, new Vector2(0f, 480f), new Vector2(PanelWidth, 110f));

    if (result.IsClear)
    {
      for (int i = 0; i < 3; i++)
      {
        var star = PixelUI.CreateIcon(panel.transform, i < result.Stars ? "star" : "star_empty", StarScale);
        PixelUI.Place(star.rectTransform, center, new Vector2((i - 1) * StarGap, 300f), star.rectTransform.sizeDelta);
      }
    }
    else
    {
      var floors = PixelUI.CreateText(panel.transform, "かいそう " + (result.FloorsCleared + 1) + "/" + result.FloorCount + " でやられた", PixelUI.MediumFont, PixelArt.Pale);
      PixelUI.Place(floors.rectTransform, center, new Vector2(0f, 300f), new Vector2(PanelWidth, 90f));
    }

    var coinIcon = PixelUI.CreateIcon(panel.transform, "coin", 10f);
    PixelUI.Place(coinIcon.rectTransform, center, new Vector2(-200f, 110f), coinIcon.rectTransform.sizeDelta);
    coinsText = PixelUI.CreateText(panel.transform, "+" + result.CoinsEarned, PixelUI.HugeFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinsText.rectTransform, center, new Vector2(110f, 110f), new Vector2(500f, 130f));
    totalText = PixelUI.CreateText(panel.transform, "もちコイン " + Progress.Coins, PixelUI.SmallFont, PixelArt.Pale);
    PixelUI.Place(totalText.rectTransform, center, new Vector2(0f, -10f), new Vector2(PanelWidth, 60f));

    // 広告でコイン2倍にできるときは、ヒントの代わりにボタンを出す
    if (onDoubleCoins != null)
    {
      doubleCoinsButton = PixelUI.CreateButton(panel.transform, "広告を見て コイン2倍", PixelUI.SmallFont, PixelArt.Purple, onDoubleCoins);
      PixelUI.Place((RectTransform)doubleCoinsButton.transform, center, new Vector2(0f, -110f), new Vector2(DoubleCoinsButtonWidth, DoubleCoinsButtonHeight));
    }
    else
    {
      var hint = PixelUI.CreateText(panel.transform, "コインでスキルツリーを強化しよう", PixelUI.SmallFont, PixelArt.LightBlue);
      PixelUI.Place(hint.rectTransform, center, new Vector2(0f, -110f), new Vector2(PanelWidth, 60f));
    }

    bool hasNext = result.IsClear && result.Stage < StageDatabase.StageCount;
    float buttonY = -400f;
    if (hasNext)
    {
      var next = PixelUI.CreateButton(panel.transform, "NEXT", PixelUI.LargeFont, PixelArt.Teal, onNext);
      PixelUI.Place((RectTransform)next.transform, center, new Vector2(0f, -250f), new Vector2(ButtonWidth * 2f, ButtonHeight));
    }
    var retry = PixelUI.CreateButton(panel.transform, "RETRY", PixelUI.MediumFont, PixelArt.Teal, onRetry);
    PixelUI.Place((RectTransform)retry.transform, center, new Vector2(-200f, buttonY), new Vector2(ButtonWidth, ButtonHeight));
    var map = PixelUI.CreateButton(panel.transform, "MAP", PixelUI.MediumFont, PixelArt.Teal, onMap);
    PixelUI.Place((RectTransform)map.transform, center, new Vector2(200f, buttonY), new Vector2(ButtonWidth, ButtonHeight));
  }

  public void HideDoubleCoinsButton()
  {
    doubleCoinsButton.gameObject.SetActive(false);
  }

  public void ShowDoubledCoins(int coinsEarned)
  {
    coinsText.text = "+" + coinsEarned;
    totalText.text = "もちコイン " + Progress.Coins;
  }
}
