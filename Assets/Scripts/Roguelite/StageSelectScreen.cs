using UnityEngine;
using UnityEngine.UI;

// メニュー画面：100ステージのマップとスキルツリーへの入口
public class StageSelectScreen : MonoBehaviour
{
  const int Columns = 6;
  const float CellSize = 170f;
  const float CellGap = 22f;
  const float HeaderHeight = 100f;
  const float TeaserHeight = 60f;
  const float BottomButtonHeight = 120f;
  const float BottomMargin = 40f;
  const float StarScale = 3f;
  const float BadgeScale = 4f;
  const float LockScale = 6f;
  const float FadeSeconds = 0.3f;
  // 歯車ボタンの中心の高さ（OptionButtonPosition が左上のセーフエリアの内側に置く）
  const float GearCenterFromSafeTop = 90f;
  static readonly Color32 NextStageBorder = new Color32(255, 224, 102, 255);

  Text coinText;
  RectTransform root;
  GameObject skillBadge;

  void Start()
  {
    // 画面にある歯車ボタン（有効な OptionManager）と同じキャンバスに作る
    var gearCanvas = FindFirstObjectByType<OptionManager>().GetComponentInParent<Canvas>().rootCanvas;
    Build(gearCanvas.transform);
  }

  // 歯車ボタンと同じキャンバスの一番奥に作り、オプション画面が上に重なるようにする
  void Build(Transform canvas)
  {
    root = PixelUI.Stretch(PixelUI.CreateRect("StageSelect", canvas));
    root.SetSiblingIndex(0);
    float unitsPerPixel = ((RectTransform)canvas).rect.height / Screen.height;
    float safeTop = (Screen.height - Screen.safeArea.yMax) * unitsPerPixel;
    // 下のバナーはホームバーの上（セーフエリアの下端）から上向きに出る
    float safeBottom = Screen.safeArea.yMin * unitsPerPixel;
    float bannerUnits = BannerLayout.HeightPixels * unitsPerPixel;
    FieldFloor.CreateCovering(Camera.main);
    float headerY = -(safeTop + GearCenterFromSafeTop);

    var title = PixelUI.CreateText(root, "STAGE SELECT", PixelUI.MediumFont, PixelArt.LightBlue);
    PixelUI.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, headerY), new Vector2(700f, HeaderHeight));
    var coinIcon = PixelUI.CreateIcon(root, "coin", 5f);
    PixelUI.Place(coinIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-260f, headerY), coinIcon.rectTransform.sizeDelta);
    coinText = PixelUI.CreateText(root, "", PixelUI.SmallFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinText.rectTransform, new Vector2(1f, 1f), new Vector2(-120f, headerY), new Vector2(220f, HeaderHeight));

    float teaserY = headerY - HeaderHeight / 2f - TeaserHeight / 2f;
    CreateTeaser(teaserY);

    float buttonY = safeBottom + bannerUnits + BottomMargin + BottomButtonHeight / 2f;
    float listTop = -teaserY + TeaserHeight / 2f + CellGap;
    float listBottom = buttonY + BottomButtonHeight / 2f + CellGap;
    BuildStageList(listTop, listBottom);

    var skillButton = PixelUI.CreateButton(root, "スキルツリー", PixelUI.MediumFont, PixelArt.Purple, OpenSkillTree);
    PixelUI.Place((RectTransform)skillButton.transform, new Vector2(0.5f, 0f), new Vector2(-270f, buttonY), new Vector2(520f, BottomButtonHeight));
    // 強化できるスキルがあるときは「!」を出して、スキルツリーを開きたくさせる
    var badge = PixelUI.CreatePanel(skillButton.transform, PixelArt.Red, PixelArt.White);
    PixelUI.Place(badge.rectTransform, new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(56f, 56f));
    var badgeText = PixelUI.CreateText(badge.transform, "!", PixelUI.SmallFont, PixelArt.White);
    PixelUI.Stretch(badgeText.rectTransform);
    skillBadge = badge.gameObject;
    var versusButton = PixelUI.CreateButton(root, "たいせん", PixelUI.MediumFont, PixelArt.DarkRed, OpenVersus);
    PixelUI.Place((RectTransform)versusButton.transform, new Vector2(0.5f, 0f), new Vector2(290f, buttonY), new Vector2(440f, BottomButtonHeight));
    Refresh();
  }

  // 次のステージで新しく出る強化を見せて、先へ進みたくさせる
  void CreateTeaser(float y)
  {
    int nextStage = Mathf.Min(Progress.HighestCleared + 1, StageDatabase.StageCount);
    string message = "つぎは STAGE " + nextStage;
    for (int stage = nextStage; stage <= StageDatabase.StageCount; stage++)
    {
      var unlock = Upgrades.NextUnlock(stage - 1);
      if (unlock != null && stage > Progress.HighestCleared + 1)
      {
        message += "   STAGE " + stage + " で「" + unlock.Name + "」解放";
        break;
      }
    }
    var text = PixelUI.CreateText(root, message, PixelUI.SmallFont, PixelArt.Pale);
    PixelUI.Place(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(1200f, TeaserHeight));
  }

  public void Refresh()
  {
    coinText.text = Progress.Coins.ToString();
    skillBadge.SetActive(CanAffordAnySkill());
  }

  static bool CanAffordAnySkill()
  {
    foreach (var skill in SkillTree.All)
    {
      int cost = SkillTree.NextCost(skill);
      if (cost >= 0 && SkillTree.IsUnlocked(skill) && Progress.Coins >= cost)
      {
        return true;
      }
    }
    return false;
  }

  void BuildStageList(float topOffset, float bottomOffset)
  {
    // スクロールできる枠
    var viewport = PixelUI.CreateRect("StageList", root);
    viewport.anchorMin = new Vector2(0f, 0f);
    viewport.anchorMax = new Vector2(1f, 1f);
    viewport.offsetMin = new Vector2(0f, bottomOffset);
    viewport.offsetMax = new Vector2(0f, -topOffset);
    viewport.gameObject.AddComponent<RectMask2D>();
    viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

    int rows = Mathf.CeilToInt(StageDatabase.StageCount / (float)Columns);
    var content = PixelUI.CreateRect("Content", viewport);
    content.anchorMin = new Vector2(0.5f, 1f);
    content.anchorMax = new Vector2(0.5f, 1f);
    content.pivot = new Vector2(0.5f, 1f);
    content.sizeDelta = new Vector2(Columns * (CellSize + CellGap), rows * (CellSize + CellGap) + CellGap);

    var scroll = viewport.gameObject.AddComponent<ScrollRect>();
    scroll.content = content;
    scroll.horizontal = false;
    scroll.movementType = ScrollRect.MovementType.Clamped;
    scroll.scrollSensitivity = 40f;

    int nextStage = Mathf.Min(Progress.HighestCleared + 1, StageDatabase.StageCount);
    for (int stage = 1; stage <= StageDatabase.StageCount; stage++)
    {
      CreateStageCell(content, stage, stage == nextStage);
    }

    // 次に遊ぶステージが見える位置までスクロールしておく
    int nextRow = (nextStage - 1) / Columns;
    content.anchoredPosition = new Vector2(0f, Mathf.Max(0f, (nextRow - 2) * (CellSize + CellGap)));
  }

  void CreateStageCell(RectTransform content, int stage, bool isNext)
  {
    int index = stage - 1;
    var position = new Vector2(
      (index % Columns - (Columns - 1) / 2f) * (CellSize + CellGap),
      -(index / Columns) * (CellSize + CellGap) - CellSize / 2f - CellGap);
    bool isUnlocked = Progress.IsUnlocked(stage);
    StageType type = StageDatabase.GetType(stage);

    Color32 fill = type == StageType.Boss ? PixelArt.DarkRed : type == StageType.Hard ? PixelArt.Purple : PixelArt.Teal;
    if (!isUnlocked)
    {
      fill = PixelArt.Gray;
    }
    int capturedStage = stage;
    var button = PixelUI.CreateButton(content, isUnlocked ? stage.ToString() : "", PixelUI.MediumFont, fill, () => StartStage(capturedStage));
    button.name = "Stage_" + stage;
    button.interactable = isUnlocked;
    var rect = PixelUI.Place((RectTransform)button.transform, new Vector2(0.5f, 1f), position, new Vector2(CellSize, CellSize));
    if (isNext)
    {
      // 次に遊ぶステージは黄色い枠で目立たせる
      button.GetComponent<Image>().sprite = PixelArt.FrameSprite(fill, NextStageBorder);
      var play = PixelUI.CreateText(rect, "▶", PixelUI.SmallFont, PixelArt.Yellow);
      PixelUI.Place(play.rectTransform, new Vector2(0f, 1f), new Vector2(32f, -30f), new Vector2(50f, 50f));
    }

    // ボスはドクロ、ゴールデンラッシュはコイン、ハードは炎のマーク。
    // まだ遊べないステージにも出して「もうすぐボス」「もうすぐラッシュ」と先が楽しみになるようにする
    string badgeKey = BadgeOf(stage, type);
    if (badgeKey != null)
    {
      var badge = PixelUI.CreateIcon(rect, badgeKey, BadgeScale);
      PixelUI.Place(badge.rectTransform, new Vector2(1f, 1f), new Vector2(-26f, -26f), badge.rectTransform.sizeDelta);
    }

    if (!isUnlocked)
    {
      var lockIcon = PixelUI.CreateIcon(rect, "lock", LockScale);
      PixelUI.Place(lockIcon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, lockIcon.rectTransform.sizeDelta);
      return;
    }
    ((RectTransform)button.GetComponentInChildren<Text>().transform).anchoredPosition = new Vector2(0f, 14f);

    int stars = Progress.GetStars(stage);
    for (int i = 0; i < 3; i++)
    {
      var star = PixelUI.CreateIcon(rect, i < stars ? "star" : "star_empty", StarScale);
      PixelUI.Place(star.rectTransform, new Vector2(0.5f, 0f), new Vector2((i - 1) * 32f, 28f), star.rectTransform.sizeDelta);
    }
  }

  static string BadgeOf(int stage, StageType type)
  {
    if (type == StageType.Boss) return "skull";
    if (StageDatabase.HasGoldenRush(stage)) return "coin";
    if (type == StageType.Hard) return "flame";
    return null;
  }

  void StartStage(int stage)
  {
    if (FadeManager.Instance.IsFading)
    {
      return;
    }
    RunState.SelectedStage = stage;
    FadeManager.Instance.LoadScene("StagePlay", FadeSeconds);
  }

  void OpenSkillTree()
  {
    new SkillTreeScreen(PixelUI.CreateOverlayCanvas("SkillTreeCanvas", 100).transform, Refresh);
  }

  void OpenVersus()
  {
    new VersusSetupScreen(PixelUI.CreateOverlayCanvas("VersusSetupCanvas", 100).transform, () => SoundEffects.Play(Sfx.Button));
  }
}
