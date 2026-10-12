using System.Collections.Generic;

// ステージ中に階層クリアで選べる強化（ステージが終わるとリセット）
public enum UpgradeId { MultiBall, PowerBall, WidePaddle, BigBall, Heal, Shield, CoinBoost, SlowBall }

public class UpgradeDefinition
{
  public UpgradeId Id;
  public string Name;
  public string Description;
  public string Icon;
  public int MaxLevel;
}

public static class Upgrades
{
  public const int ChoiceCount = 3;

  public static readonly UpgradeDefinition[] All =
  {
    new UpgradeDefinition { Id = UpgradeId.MultiBall, Name = "マルチボール", Description = "ボールが1つ増える", Icon = "icon_multiball", MaxLevel = 2 },
    new UpgradeDefinition { Id = UpgradeId.PowerBall, Name = "パワーボール", Description = "ボールの攻撃力+1", Icon = "icon_power", MaxLevel = 3 },
    new UpgradeDefinition { Id = UpgradeId.WidePaddle, Name = "ワイドバー", Description = "バーの幅が20%広がる", Icon = "icon_wide", MaxLevel = 3 },
    new UpgradeDefinition { Id = UpgradeId.BigBall, Name = "ビッグボール", Description = "ボールが25%大きくなる", Icon = "icon_bigball", MaxLevel = 2 },
    new UpgradeDefinition { Id = UpgradeId.Heal, Name = "なかま回復", Description = "やられたスライムが1匹もどる", Icon = "icon_heal", MaxLevel = 99 },
    new UpgradeDefinition { Id = UpgradeId.Shield, Name = "バリア", Description = "階層ごとに1回ボールをはね返す", Icon = "icon_shield", MaxLevel = 1 },
    new UpgradeDefinition { Id = UpgradeId.CoinBoost, Name = "コインざくざく", Description = "もらえるコイン+25%", Icon = "icon_coin", MaxLevel = 3 },
    new UpgradeDefinition { Id = UpgradeId.SlowBall, Name = "スロー", Description = "ボールの最高速度-10%", Icon = "icon_slow", MaxLevel = 2 },
  };

  public static UpgradeDefinition Get(UpgradeId id)
  {
    return System.Array.Find(All, definition => definition.Id == id);
  }

  // まだ取れる強化の中から、重ならないように3つ選ぶ
  public static List<UpgradeDefinition> Roll(System.Random random)
  {
    var candidates = new List<UpgradeDefinition>();
    foreach (var definition in All)
    {
      if (RunState.GetUpgradeLevel(definition.Id) >= definition.MaxLevel) continue;
      // 回復はやられた仲間がいるときだけ候補にする
      if (definition.Id == UpgradeId.Heal && RunState.Lives >= RunState.MaxLives) continue;
      candidates.Add(definition);
    }
    var result = new List<UpgradeDefinition>();
    while (result.Count < ChoiceCount && candidates.Count > 0)
    {
      int index = random.Next(candidates.Count);
      result.Add(candidates[index]);
      candidates.RemoveAt(index);
    }
    return result;
  }
}
