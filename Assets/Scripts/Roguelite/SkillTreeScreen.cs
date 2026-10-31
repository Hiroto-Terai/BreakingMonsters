using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// スキルツリー画面：コインを使ってパッシブスキルを解放・強化する
public class SkillTreeScreen
{
  const float ColumnWidth = 410f;
  const float NodeWidth = 380f;
  const float NodeHeight = 320f;
  const float TierGap = 70f;
  const float TreeTopY = 520f;
  const float IconScale = 8f;
  const float LinkWidth = 14f;

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

    var title = PixelUI.CreateText(root, "スキルツリー", PixelUI.MediumFont, PixelArt.LightBlue);
    PixelUI.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, TreeTopY + 260f), new Vector2(900f, 90f));
    var coinIcon = PixelUI.CreateIcon(root, "coin", 5f);
    PixelUI.Place(coinIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-100f, TreeTopY + 180f), coinIcon.rectTransform.sizeDelta);
    coinText = PixelUI.CreateText(root, "", PixelUI.SmallFont, PixelArt.Yellow, TextAnchor.MiddleLeft);
    PixelUI.Place(coinText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(60f, TreeTopY + 180f), new Vector2(300f, 70f));

    for (int branch = 0; branch < SkillTree.BranchNames.Length; branch++)
    {
      float x = (branch - 1) * ColumnWidth;
      var branchName = PixelUI.CreateText(root, SkillTree.BranchNames[branch], PixelUI.SmallFont, PixelArt.Pale);
      PixelUI.Place(branchName.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, TreeTopY + 70f), new Vector2(ColumnWidth, 60f));
    }
    foreach (var skill in SkillTree.All)
    {
      CreateNode(skill);
    }

    var close = PixelUI.CreateButton(root, "とじる", PixelUI.MediumFont, PixelArt.Teal, Close);
    PixelUI.Place((RectTransform)close.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, TreeTopY - 3 * (NodeHeight + TierGap) - 30f), new Vector2(480f, 110f));
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
    PixelUI.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -60f), icon.rectTransform.sizeDelta);
    var name = PixelUI.CreateText(rect, skill.Name, PixelUI.SmallFont, PixelArt.White);
    PixelUI.Place(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -135f), new Vector2(NodeWidth - 20f, 46f));
    var description = PixelUI.CreateText(rect, skill.Description, PixelUI.SmallFont, PixelArt.Pale);
    PixelUI.Place(description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -185f), new Vector2(NodeWidth - 30f, 56f));
    var status = PixelUI.CreateText(rect, "", PixelUI.SmallFont, PixelArt.Yellow);
    PixelUI.Place(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(NodeWidth - 20f, 46f));
    var lockIcon = PixelUI.CreateIcon(rect, "lock", 6f);
    PixelUI.Place(lockIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-40f, -40f), lockIcon.rectTransform.sizeDelta);

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
    if (SkillTree.TryLevelUp(skill))
    {
      SoundEffects.Play(Sfx.UpgradePick);
    }
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
