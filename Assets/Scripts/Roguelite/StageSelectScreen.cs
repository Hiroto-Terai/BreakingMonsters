using UnityEngine;
using UnityEngine.UI;

// メニュー画面：100ステージのマップとスキルツリーへの入口
public class StageSelectScreen : MonoBehaviour
{
  const int Columns = 5;
  const float CellSize = 220f;
  const float CellGap = 30f;
  const float HeaderOffsetY = -330f;
  const float ListTopOffset = -470f;
  const float ListBottomOffset = 520f;
  const float SkillButtonY = 360f;
  const float StarScale = 4f;
  const float BadgeScale = 5f;
  const float LockScale = 8f;
  const float FadeSeconds = 0.3f;

  public AudioClip ButtonSound;
  Text coinText;
  RectTransform root;

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

    var title = PixelUI.CreateText(root, "STAGE SELECT", PixelUI.LargeFont, PixelArt.LightBlue);
    PixelUI.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, HeaderOffsetY), new Vector2(900f, 100f));

    var coinIcon = PixelUI.CreateIcon(root, "coin", 6f);
    PixelUI.Place(coinIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-300f, HeaderOffsetY), coinIcon.rectTransform.sizeDelta);
    coinText = PixelUI.CreateText(root, "", PixelUI.MediumFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinText.rectTransform, new Vector2(1f, 1f), new Vector2(-140f, HeaderOffsetY), new Vector2(240f, 90f));

    BuildStageList();

    var skillButton = PixelUI.CreateButton(root, "スキルツリー", PixelUI.LargeFont, PixelArt.Purple, OpenSkillTree);
    PixelUI.Place((RectTransform)skillButton.transform, new Vector2(0.5f, 0f), new Vector2(0f, SkillButtonY), new Vector2(800f, 160f));
    Refresh();
  }

  public void Refresh()
  {
    coinText.text = Progress.Coins.ToString();
  }

  void BuildStageList()
  {
    // スクロールできる枠
    var viewport = PixelUI.CreateRect("StageList", root);
    viewport.anchorMin = new Vector2(0f, 0f);
    viewport.anchorMax = new Vector2(1f, 1f);
    viewport.offsetMin = new Vector2(0f, ListBottomOffset);
    viewport.offsetMax = new Vector2(0f, ListTopOffset);
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

    for (int stage = 1; stage <= StageDatabase.StageCount; stage++)
    {
      CreateStageCell(content, stage);
    }

    // 次に遊ぶステージが見える位置までスクロールしておく
    int nextStage = Mathf.Min(Progress.HighestCleared + 1, StageDatabase.StageCount);
    int nextRow = (nextStage - 1) / Columns;
    content.anchoredPosition = new Vector2(0f, Mathf.Max(0f, (nextRow - 1) * (CellSize + CellGap)));
  }

  void CreateStageCell(RectTransform content, int stage)
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
    var button = PixelUI.CreateButton(content, isUnlocked ? stage.ToString() : "", PixelUI.LargeFont, fill, () => StartStage(capturedStage));
    button.name = "Stage_" + stage;
    button.interactable = isUnlocked;
    var rect = PixelUI.Place((RectTransform)button.transform, new Vector2(0.5f, 1f), position, new Vector2(CellSize, CellSize));

    if (!isUnlocked)
    {
      var lockIcon = PixelUI.CreateIcon(rect, "lock", LockScale);
      PixelUI.Place(lockIcon.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, lockIcon.rectTransform.sizeDelta);
      return;
    }

    // ボスはドクロ、ハードは炎のマーク
    if (type != StageType.Normal)
    {
      var badge = PixelUI.CreateIcon(rect, type == StageType.Boss ? "skull" : "flame", BadgeScale);
      PixelUI.Place(badge.rectTransform, new Vector2(1f, 1f), new Vector2(-34f, -34f), badge.rectTransform.sizeDelta);
    }
    ((RectTransform)button.GetComponentInChildren<Text>().transform).anchoredPosition = new Vector2(0f, 18f);

    int stars = Progress.GetStars(stage);
    for (int i = 0; i < 3; i++)
    {
      var star = PixelUI.CreateIcon(rect, i < stars ? "star" : "star_empty", StarScale);
      PixelUI.Place(star.rectTransform, new Vector2(0.5f, 0f), new Vector2((i - 1) * 40f, 36f), star.rectTransform.sizeDelta);
    }
  }

  void StartStage(int stage)
  {
    if (FadeManager.Instance.IsFading)
    {
      return;
    }
    PlayButtonSound();
    RunState.SelectedStage = stage;
    FadeManager.Instance.LoadScene("StagePlay", FadeSeconds);
  }

  void OpenSkillTree()
  {
    PlayButtonSound();
    new SkillTreeScreen(PixelUI.CreateOverlayCanvas("SkillTreeCanvas", 100).transform, () => { PlayButtonSound(); Refresh(); });
  }

  void PlayButtonSound()
  {
    if (OptionManager.isSePlaying && ButtonSound != null)
    {
      AudioSource.PlayClipAtPoint(ButtonSound, Camera.main.transform.position);
    }
  }
}
