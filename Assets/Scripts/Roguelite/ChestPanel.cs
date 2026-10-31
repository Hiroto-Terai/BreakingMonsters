using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 階層クリアの宝箱。アイコンがルーレットのように回ってから、もらえる強化が決まる（ヴァンサバの宝箱）
public class ChestPanel : MonoBehaviour
{
  const float PanelWidth = 900f;
  const float PanelHeight = 760f;
  const float SpinSeconds = 1.1f;
  const float FirstSpinStep = 0.05f;
  const float LastSpinStep = 0.2f;
  const float IconScale = 18f;
  const float ChestIconScale = 14f;
  static readonly Color32 Gold = new Color32(250, 190, 60, 255);

  UpgradeDefinition reward;
  Action<UpgradeDefinition> onReceive;
  Image icon;
  Text nameText;
  Text detailText;
  Button receiveButton;

  public static ChestPanel Open(Transform canvas, UpgradeDefinition reward, Action<UpgradeDefinition> onReceive)
  {
    var root = PixelUI.Stretch(PixelUI.CreateRect("ChestPanel", canvas));
    PixelUI.CreateDimmer(root, 0.75f);
    var panel = PixelUI.CreatePanel(root, PixelArt.Navy, Gold);
    var center = new Vector2(0.5f, 0.5f);
    PixelUI.Place(panel.rectTransform, center, Vector2.zero, new Vector2(PanelWidth, PanelHeight));

    var chest = root.gameObject.AddComponent<ChestPanel>();
    chest.reward = reward;
    chest.onReceive = onReceive;
    var title = PixelUI.CreateText(panel.transform, "たからばこ!", PixelUI.LargeFont, Gold);
    PixelUI.Place(title.rectTransform, center, new Vector2(0f, 280f), new Vector2(PanelWidth, 90f));
    chest.icon = PixelUI.CreateIcon(panel.transform, "icon_chest", ChestIconScale);
    PixelUI.Place(chest.icon.rectTransform, center, new Vector2(0f, 90f), chest.icon.rectTransform.sizeDelta);
    chest.nameText = PixelUI.CreateText(panel.transform, "", PixelUI.MediumFont, PixelArt.White);
    PixelUI.Place(chest.nameText.rectTransform, center, new Vector2(0f, -80f), new Vector2(PanelWidth, 70f));
    chest.detailText = PixelUI.CreateText(panel.transform, "", PixelUI.SmallFont, PixelArt.Pale);
    PixelUI.Place(chest.detailText.rectTransform, center, new Vector2(0f, -150f), new Vector2(PanelWidth - 60f, 60f));
    chest.receiveButton = PixelUI.CreateButton(panel.transform, "うけとる", PixelUI.MediumFont, PixelArt.Teal, chest.Receive);
    PixelUI.Place((RectTransform)chest.receiveButton.transform, center, new Vector2(0f, -280f), new Vector2(480f, 120f));
    chest.receiveButton.gameObject.SetActive(false);
    chest.StartCoroutine(chest.Spin());
    return chest;
  }

  // だんだん遅くなるルーレット。止まったら当たりを見せる
  IEnumerator Spin()
  {
    SoundEffects.Play(Sfx.ChestOpen);
    var all = Upgrades.All;
    float elapsed = 0f;
    int index = 0;
    while (elapsed < SpinSeconds)
    {
      float step = Mathf.Lerp(FirstSpinStep, LastSpinStep, elapsed / SpinSeconds);
      SetIcon(all[index % all.Length].Icon);
      index++;
      SoundEffects.Play(Sfx.Button);
      yield return new WaitForSecondsRealtime(step);
      elapsed += step;
    }
    SetIcon(reward.Icon);
    bool isEvolution = reward.IsEvolution;
    SoundEffects.Play(isEvolution ? Sfx.Evolve : Sfx.UpgradePick);
    nameText.text = reward.Name + (isEvolution ? " <color=#FAC03C>進化!</color>" : LevelTag());
    detailText.text = reward.Describe(RunState.GetUpgradeLevel(reward.Id) + 1);
    receiveButton.gameObject.SetActive(true);
  }

  string LevelTag()
  {
    if (reward.Id == UpgradeId.Heal) return "";
    int level = RunState.GetUpgradeLevel(reward.Id);
    return level == 0 ? " <color=#6EDC78>NEW!</color>" : " <color=#FFE066>Lv" + level + "→" + (level + 1) + "</color>";
  }

  void SetIcon(string key)
  {
    var sprite = PixelArt.UiSprite(key);
    icon.sprite = sprite;
    icon.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height) * IconScale;
  }

  void Receive()
  {
    receiveButton.interactable = false;
    onReceive(reward);
    Destroy(gameObject);
  }
}
