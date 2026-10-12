using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// プレイ中のスコア・コンボ・経過時間を管理し、クリア/ゲームオーバー時に結果を表示する
public class GameSession : MonoBehaviour
{
  public static GameSession Instance { get; private set; }

  const int ScorePerMonster = 100;
  const int ScorePerRemainingSlime = 500;
  const int TimeBonusPerSecond = 10;
  const float TimeBonusLimitSeconds = 90f;
  const int HudFontSize = 64;
  const int PopupFontSize = 52;
  const int ResultFontSize = 56;
  const int ResultStarsFontSize = 96;
  const float TitleLiftY = 260f;
  const float ResultTopFromTitleY = 170f;
  const float NextButtonGapY = 40f;
  const float ComboPopupLiftY = 60f;

  public int Score { get; private set; }
  public bool IsEnded { get; private set; }
  int combo;
  float elapsedSeconds;

  GameClear gameClear;
  GameOver gameOver;
  CountDown countDown;
  Text scoreText;
  Font font;

  void Awake()
  {
    Instance = this;
  }

  void OnDestroy()
  {
    if (Instance == this)
    {
      Instance = null;
    }
  }

  void Start()
  {
    gameClear = FindFirstObjectByType<GameClear>();
    gameOver = FindFirstObjectByType<GameOver>();
    countDown = FindFirstObjectByType<CountDown>();
    font = gameClear.gameClearText.font;
    CreateScoreHud();
  }

  void Update()
  {
    if (!IsEnded && countDown.IsFinished)
    {
      elapsedSeconds += Time.deltaTime;
    }
  }

  // 一時停止ボタンと同じ高さの右側にスコアを表示する
  void CreateScoreHud()
  {
    var pauseButton = FindFirstObjectByType<OptionButtonPosition>().GetComponent<RectTransform>();
    scoreText = CreateText(pauseButton.parent, "SCORE 0", HudFontSize, TextAnchor.MiddleRight);
    // 一時停止画面の暗幕より奥に描かれるよう、一時停止ボタンのすぐ後ろの順番に置く
    scoreText.transform.SetSiblingIndex(pauseButton.GetSiblingIndex() + 1);
    var rect = scoreText.rectTransform;
    rect.anchorMin = new Vector2(1f, pauseButton.anchorMin.y);
    rect.anchorMax = new Vector2(1f, pauseButton.anchorMax.y);
    rect.pivot = new Vector2(1f, 0.5f);
    rect.sizeDelta = new Vector2(600f, 80f);
    rect.anchoredPosition = new Vector2(-pauseButton.anchoredPosition.x, pauseButton.anchoredPosition.y);
  }

  Text CreateText(Transform parent, string message, int fontSize, TextAnchor alignment)
  {
    var textObject = new GameObject("Text_" + message.Split(' ')[0], typeof(RectTransform));
    textObject.transform.SetParent(parent, false);
    var text = textObject.AddComponent<Text>();
    text.font = font;
    text.fontSize = fontSize;
    text.alignment = alignment;
    text.color = Color.white;
    text.horizontalOverflow = HorizontalWrapMode.Overflow;
    text.verticalOverflow = VerticalWrapMode.Overflow;
    text.raycastTarget = false;
    text.text = message;
    return text;
  }

  public void OnMonsterDefeated(Vector3 worldPosition)
  {
    combo++;
    int gained = ScorePerMonster * combo;
    Score += gained;
    scoreText.text = "SCORE " + Score;
    string popup = combo >= 2 ? "+" + gained + "  " + combo + " COMBO!" : "+" + gained;
    ShowPopup(popup, worldPosition);
    HitEffects.Burst(worldPosition, new Color(0.55f, 0.85f, 1f));
  }

  public void OnSlimeLost(Vector3 worldPosition)
  {
    combo = 0;
    HitEffects.Burst(worldPosition, new Color(1f, 0.45f, 0.45f));
    HitEffects.Shake();
    if (OptionManager.isSePlaying)
    {
      HitEffects.Vibrate();
    }
  }

  public void OnPaddleHit()
  {
    combo = 0;
  }

  void ShowPopup(string message, Vector3 worldPosition)
  {
    var canvas = scoreText.canvas;
    var canvasRect = canvas.GetComponent<RectTransform>();
    Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, eventCamera, out Vector2 localPoint);

    var popup = CreateText(canvasRect, message, PopupFontSize, TextAnchor.MiddleCenter);
    popup.color = new Color(1f, 0.92f, 0.4f);
    popup.rectTransform.anchoredPosition = localPoint;
    popup.gameObject.AddComponent<FloatingText>().Lift = ComboPopupLiftY;
  }

  // クリア/ゲームオーバーは同じフレームに両方起きうるので、先に呼ばれた方だけを有効にする
  public bool TryEnd(bool isClear)
  {
    if (IsEnded)
    {
      return false;
    }
    IsEnded = true;

    int remainingSlimes = gameOver.transform.childCount;
    int stars = isClear ? remainingSlimes : 0;
    int timeBonus = isClear ? Mathf.Max(0, Mathf.RoundToInt((TimeBonusLimitSeconds - elapsedSeconds) * TimeBonusPerSecond)) : 0;
    int slimeBonus = isClear ? remainingSlimes * ScorePerRemainingSlime : 0;
    Score += timeBonus + slimeBonus;
    scoreText.text = "SCORE " + Score;

    string modeScene = SceneManager.GetActiveScene().name;
    bool isNewBest = isClear && GameRecords.SaveIfBest(modeScene, Score, stars);
    ShowResult(isClear, stars, timeBonus + slimeBonus, isNewBest);
    return true;
  }

  void ShowResult(bool isClear, int stars, int bonus, bool isNewBest)
  {
    var title = isClear ? gameClear.gameClearText : gameOver.gameOverText;
    // 星は大きく、内訳は1行にまとめてボタンと重ならないようにする
    string message = isClear
      ? "<size=" + ResultStarsFontSize + "><color=#FFE066>" + GameRecords.StarsText(stars) + "</color></size>\nSCORE " + Score + "\n<size=" + (ResultFontSize * 3 / 4) + ">TIME " + elapsedSeconds.ToString("F1") + "s  BONUS +" + bonus + "</size>"
      : "SCORE " + Score;
    if (isNewBest)
    {
      message += "\n<color=#FFE066>NEW BEST!</color>";
    }

    Vector2 titlePosition = title.rectTransform.anchoredPosition + new Vector2(0f, TitleLiftY);
    title.rectTransform.anchoredPosition = titlePosition;
    var result = CreateText(title.transform.parent, message, ResultFontSize, TextAnchor.UpperCenter);
    result.supportRichText = true;
    result.transform.SetSiblingIndex(title.transform.GetSiblingIndex() + 1);
    var rect = result.rectTransform;
    rect.anchorMin = title.rectTransform.anchorMin;
    rect.anchorMax = title.rectTransform.anchorMax;
    rect.pivot = new Vector2(0.5f, 1f);
    rect.sizeDelta = new Vector2(1000f, 400f);
    rect.anchoredPosition = titlePosition + new Vector2(0f, -ResultTopFromTitleY);

    if (isClear)
    {
      CreateNextButton();
    }
  }

  // RETRY ボタンを複製して、次の難易度へ進むボタンを作る
  void CreateNextButton()
  {
    string nextScene = GameRecords.GetNextModeScene(SceneManager.GetActiveScene().name);
    if (nextScene == null)
    {
      return;
    }
    var retry = gameClear.loadSceneCanvas.transform.Find("Retry").GetComponent<RectTransform>();
    var menu = gameClear.loadSceneCanvas.transform.Find("GoToMenu").GetComponent<RectTransform>();
    var next = Instantiate(retry.gameObject, retry.parent).GetComponent<RectTransform>();
    next.name = "Next";
    next.anchoredPosition = (retry.anchoredPosition + menu.anchoredPosition) / 2f + new Vector2(0f, -(retry.rect.height + NextButtonGapY));
    next.GetComponentInChildren<Text>().text = "NEXT";

    // RETRY/Menu ボタンは EventTrigger のクリックで動くので、複製したクリック処理を差し替える
    var optionManager = next.GetComponent<OptionManager>();
    var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
    click.callback.AddListener(_ => optionManager.GoToScene(nextScene));
    next.GetComponent<EventTrigger>().triggers = new List<EventTrigger.Entry> { click };
  }
}
