using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// スキルツリー画面：コインを使ってパッシブスキルを解放・強化する
public class SkillTreeScreen
{
  const float ColumnWidth = 420f;
  const float NodeWidth = 390f;
  const float NodeHeight = 420f;
  const float TierGap = 120f;
  const float TreeTopY = 560f;
  const float IconScale = 12f;
  const float LinkWidth = 18f;

  readonly RectTransform root;
  readonly Action onClose;
  Text coinText;
  readonly List<Action> refreshers = new List<Action>();

  public SkillTreeScreen(Transform canvas, Action onClose)
  {
    this.onClose = onClose;
    root = PixelUI.Stretch(PixelUI.CreateRect("SkillTree", canvas));
    var background = PixelUI.CreateDimmer(root, 1f);
    background.color = PixelArt.Navy;

    var title = PixelUI.CreateText(root, "スキルツリー", PixelUI.LargeFont, PixelArt.LightBlue);
    PixelUI.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, TreeTopY + 330f), new Vector2(900f, 100f));
    var coinIcon = PixelUI.CreateIcon(root, "coin", 6f);
    PixelUI.Place(coinIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-120f, TreeTopY + 230f), coinIcon.rectTransform.sizeDelta);
    coinText = PixelUI.CreateText(root, "", PixelUI.MediumFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(60f, TreeTopY + 230f), new Vector2(300f, 80f));

    for (int branch = 0; branch < SkillTree.BranchNames.Length; branch++)
    {
      float x = (branch - 1) * ColumnWidth;
      var branchName = PixelUI.CreateText(root, SkillTree.BranchNames[branch], PixelUI.MediumFont, PixelArt.Pale);
      PixelUI.Place(branchName.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, TreeTopY + 110f), new Vector2(ColumnWidth, 80f));
    }
    foreach (var skill in SkillTree.All)
    {
      CreateNode(skill);
    }

    var close = PixelUI.CreateButton(root, "とじる", PixelUI.LargeFont, PixelArt.Teal, Close);
    PixelUI.Place((RectTransform)close.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, TreeTopY - 3 * (NodeHeight + TierGap) - 60f), new Vector2(600f, 150f));
    Refresh();
  }

  static Vector2 NodePosition(SkillDefinition skill)
  {
    return new Vector2((skill.Branch - 1) * ColumnWidth, TreeTopY - skill.Tier * (NodeHeight + TierGap) - NodeHeight / 2f);
  }

  void CreateNode(SkillDefinition skill)
  {
    Vector2 position = NodePosition(skill);
    var center = new Vector2(0.5f, 0.5f);

    // 前提スキルとの間をつなぐ線
    if (skill.Requires != null)
    {
      var link = PixelUI.CreatePanel(root, PixelArt.Gray, PixelArt.Gray);
      PixelUI.Place(link.rectTransform, center, position + new Vector2(0f, NodeHeight / 2f + TierGap / 2f), new Vector2(LinkWidth, TierGap));
      refreshers.Add(() => link.color = SkillTree.IsUnlocked(skill) ? (Color)PixelArt.LightBlue : (Color)PixelArt.Gray);
    }

    var node = PixelUI.CreateButton(root, "", PixelUI.SmallFont, PixelArt.Teal, () => LevelUp(skill));
    node.name = "Skill_" + skill.Id;
    var rect = PixelUI.Place((RectTransform)node.transform, center, position, new Vector2(NodeWidth, NodeHeight));
    var nodeImage = node.GetComponent<Image>();

    var icon = PixelUI.CreateIcon(rect, skill.Icon, IconScale);
    PixelUI.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -85f), icon.rectTransform.sizeDelta);
    var name = PixelUI.CreateText(rect, skill.Name, PixelUI.SmallFont, PixelArt.White);
    PixelUI.Place(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(NodeWidth - 20f, 50f));
    var description = PixelUI.CreateText(rect, skill.Description, 24, PixelArt.Pale);
    PixelUI.Place(description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -255f), new Vector2(NodeWidth - 30f, 70f));
    var status = PixelUI.CreateText(rect, "", PixelUI.SmallFont, PixelArt.Yellow);
    PixelUI.Place(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(NodeWidth - 20f, 60f));
    var lockIcon = PixelUI.CreateIcon(rect, "lock", 8f);
    PixelUI.Place(lockIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-50f, -50f), lockIcon.rectTransform.sizeDelta);

    refreshers.Add(() =>
    {
      bool isUnlocked = SkillTree.IsUnlocked(skill);
      int level = Progress.GetSkillLevel(skill.Id);
      int cost = SkillTree.NextCost(skill);
      lockIcon.gameObject.SetActive(!isUnlocked);
      nodeImage.sprite = PixelArt.FrameSprite(isUnlocked ? (level > 0 ? PixelArt.Teal : PixelArt.DarkTeal) : PixelArt.Gray, level >= skill.MaxLevel ? PixelArt.Yellow : PixelArt.Pale);
      string levelText = "Lv" + level + "/" + skill.MaxLevel;
      if (cost < 0)
      {
        status.text = levelText + "  MAX";
      }
      else
      {
        string color = Progress.Coins >= cost && isUnlocked ? "#FFE066" : "#FF5C5C";
        status.text = levelText + "  <color=" + color + ">" + cost + "</color>";
      }
    });
  }

  void LevelUp(SkillDefinition skill)
  {
    SkillTree.TryLevelUp(skill);
    Refresh();
  }

  void Refresh()
  {
    coinText.text = Progress.Coins.ToString();
    foreach (var refresh in refreshers)
    {
      refresh();
    }
  }

  void Close()
  {
    UnityEngine.Object.Destroy(root.GetComponentInParent<Canvas>().gameObject);
    onClose();
  }
}
