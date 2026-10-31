using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// ステージクリア／ゲームオーバー時の結果画面。記録と「次に出てくるもの」を見せて、もう1回やりたくさせる
public class StageResultPanel
{
  const float PanelWidth = 1060f;
  const float PanelHeight = 1180f;
  const float StarScale = 14f;
  const float StarGap = 160f;
  const float ButtonWidth = 300f;
  const float ButtonHeight = 120f;
  const float DoubleCoinsButtonWidth = 600f;
  const float DoubleCoinsButtonHeight = 96f;
  const float StatRowHeight = 50f;

  public struct Result
  {
    public bool IsClear;
    public int Stage;
    public int Stars;
    public int FloorsCleared;
    public int FloorCount;
    public int CoinsEarned;
    public int Level;
    public int Kills;
    public int BestCombo;
    public int Evolutions;
    // 負けたときに残っていたモンスターの数（あと少しだったと分かるように）
    public int MonstersLeft;
    public UpgradeDefinition NextUnlock;
  }

  Text coinsText;
  Text totalText;
  Button doubleCoinsButton;

  public StageResultPanel(Transform canvas, Result result, UnityAction onNext, UnityAction onRetry, UnityAction onMap, UnityAction onDoubleCoins)
  {
    var root = PixelUI.Stretch(PixelUI.CreateRect("StageResultPanel", canvas));
    PixelUI.CreateDimmer(root, 0.7f);
    var panel = PixelUI.CreatePanel(root, PixelArt.Navy, result.IsClear ? PixelArt.Yellow : PixelArt.Red);
    var center = new Vector2(0.5f, 0.5f);
    PixelUI.Place(panel.rectTransform, center, Vector2.zero, new Vector2(PanelWidth, PanelHeight));

    string title = result.IsClear ? "STAGE " + result.Stage + " CLEAR!" : "GAME OVER";
    var titleText = PixelUI.CreateText(panel.transform, title, PixelUI.LargeFont, result.IsClear ? PixelArt.Yellow : PixelArt.Red);
    PixelUI.Place(titleText.rectTransform, center, new Vector2(0f, 500f), new Vector2(PanelWidth, 100f));

    if (result.IsClear)
    {
      for (int i = 0; i < 3; i++)
      {
        var star = PixelUI.CreateIcon(panel.transform, i < result.Stars ? "star" : "star_empty", StarScale);
        PixelUI.Place(star.rectTransform, center, new Vector2((i - 1) * StarGap, 360f), star.rectTransform.sizeDelta);
      }
    }
    else
    {
      string message = result.MonstersLeft > 0 && result.MonstersLeft <= 3
        ? "<color=#FFE066>あと " + result.MonstersLeft + " 体だった!</color>"
        : "かいそう " + (result.FloorsCleared + 1) + "/" + result.FloorCount + " でやられた";
      var floors = PixelUI.CreateText(panel.transform, message, PixelUI.MediumFont, PixelArt.Pale);
      PixelUI.Place(floors.rectTransform, center, new Vector2(0f, 360f), new Vector2(PanelWidth, 80f));
    }

    CreateStats(panel.transform, result, 230f);

    var coinIcon = PixelUI.CreateIcon(panel.transform, "coin", 8f);
    PixelUI.Place(coinIcon.rectTransform, center, new Vector2(-170f, 30f), coinIcon.rectTransform.sizeDelta);
    coinsText = PixelUI.CreateText(panel.transform, "+" + result.CoinsEarned, PixelUI.LargeFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinsText.rectTransform, center, new Vector2(110f, 30f), new Vector2(420f, 110f));
    totalText = PixelUI.CreateText(panel.transform, "もちコイン " + Progress.Coins, PixelUI.SmallFont, PixelArt.Pale);
    PixelUI.Place(totalText.rectTransform, center, new Vector2(0f, -45f), new Vector2(PanelWidth, 50f));

    // 広告でコイン2倍にできるときはボタンを、できないときは次の楽しみを見せる
    if (onDoubleCoins != null)
    {
      doubleCoinsButton = PixelUI.CreateButton(panel.transform, "広告を見て コイン2倍", PixelUI.SmallFont, PixelArt.Purple, onDoubleCoins);
      PixelUI.Place((RectTransform)doubleCoinsButton.transform, center, new Vector2(0f, -135f), new Vector2(DoubleCoinsButtonWidth, DoubleCoinsButtonHeight));
    }
    CreateTeaser(panel.transform, result, -240f);

    bool hasNext = result.IsClear && result.Stage < StageDatabase.StageCount;
    float buttonY = -480f;
    if (hasNext)
    {
      var next = PixelUI.CreateButton(panel.transform, "NEXT ▶", PixelUI.LargeFont, PixelArt.Green, onNext);
      next.GetComponentInChildren<Text>().color = PixelArt.Navy;
      PixelUI.Place((RectTransform)next.transform, center, new Vector2(0f, -350f), new Vector2(ButtonWidth * 2.2f, ButtonHeight));
    }
    var retry = PixelUI.CreateButton(panel.transform, "RETRY", PixelUI.SmallFont, hasNext ? PixelArt.Teal : PixelArt.Green, onRetry);
    PixelUI.Place((RectTransform)retry.transform, center, new Vector2(-170f, hasNext ? buttonY : -380f), new Vector2(ButtonWidth, hasNext ? 96f : ButtonHeight));
    var map = PixelUI.CreateButton(panel.transform, "MAP", PixelUI.SmallFont, PixelArt.Teal, onMap);
    PixelUI.Place((RectTransform)map.transform, center, new Vector2(170f, hasNext ? buttonY : -380f), new Vector2(ButtonWidth, hasNext ? 96f : ButtonHeight));
  }

  static void CreateStats(Transform parent, Result result, float top)
  {
    string[] lines =
    {
      "とどいたレベル  <color=#6EDC78>Lv" + result.Level + "</color>",
      "たおした数  " + result.Kills + "     さいこうコンボ  <color=#FF963C>" + result.BestCombo + "</color>",
    };
    if (result.Evolutions > 0)
    {
      lines[0] += "     <color=#FAC03C>進化 " + result.Evolutions + "回</color>";
    }
    for (int i = 0; i < lines.Length; i++)
    {
      var text = PixelUI.CreateText(parent, lines[i], PixelUI.SmallFont, PixelArt.Pale);
      PixelUI.Place(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, top - i * StatRowHeight), new Vector2(PanelWidth, StatRowHeight));
    }
  }

  static void CreateTeaser(Transform parent, Result result, float y)
  {
    string message;
    string upcoming = result.IsClear ? StageDatabase.UpcomingEvent(result.Stage) : null;
    if (result.IsClear && result.NextUnlock != null)
    {
      message = "<color=#6EDC78>NEW!</color> つぎから「" + result.NextUnlock.Name + "」が出るようになる";
      var icon = PixelUI.CreateIcon(parent, result.NextUnlock.Icon, 5f);
      PixelUI.Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-470f, y), icon.rectTransform.sizeDelta);
    }
    else if (upcoming != null)
    {
      message = "<color=#FF963C>" + upcoming + "</color>";
    }
    else if (result.IsClear)
    {
      message = "コインでスキルツリーを強化しよう";
    }
    else
    {
      message = "強化の組み合わせで「進化」をねらおう";
    }
    var text = PixelUI.CreateText(parent, message, PixelUI.SmallFont, PixelArt.LightBlue);
    PixelUI.Place(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(20f, y), new Vector2(PanelWidth - 120f, 50f));
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
