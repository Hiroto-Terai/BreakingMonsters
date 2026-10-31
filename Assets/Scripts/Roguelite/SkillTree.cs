// コインで解放する永続のパッシブスキル（スキルツリー）
public enum SkillId { Power, TwinBall, Critical, ExtraLife, StartShield, Revive, WidePaddle, CoinBonus, Reroll }

public class SkillDefinition
{
  public SkillId Id;
  public string Name;
  public string Description;
  public string Icon;
  // 列（0:攻撃 1:防御 2:サポート）と段（0が一番上）
  public int Branch;
  public int Tier;
  public int[] Costs;
  // この スキルを1レベル以上にすると解放される（null なら最初から解放）
  public SkillId? Requires;

  public int MaxLevel => Costs.Length;
}

public static class SkillTree
{
  public static readonly string[] BranchNames = { "こうげき", "まもり", "サポート" };

  public static readonly SkillDefinition[] All =
  {
    new SkillDefinition { Id = SkillId.Power, Name = "ちからだま", Description = "ボールの攻撃力+1", Icon = "icon_power", Branch = 0, Tier = 0, Costs = new[] { 60, 220 } },
    new SkillDefinition { Id = SkillId.TwinBall, Name = "ツインボール", Description = "ボール2個でスタート", Icon = "icon_multiball", Branch = 0, Tier = 1, Costs = new[] { 300 }, Requires = SkillId.Power },
    new SkillDefinition { Id = SkillId.Critical, Name = "かいしん", Description = "10%で2倍ダメージ", Icon = "icon_crit", Branch = 0, Tier = 2, Costs = new[] { 150, 300, 500 }, Requires = SkillId.TwinBall },

    new SkillDefinition { Id = SkillId.ExtraLife, Name = "なかま+1", Description = "スライムが1匹増える", Icon = "icon_lives", Branch = 1, Tier = 0, Costs = new[] { 80, 260 } },
    new SkillDefinition { Id = SkillId.StartShield, Name = "はじまりのバリア", Description = "各階層をバリア付きで始める", Icon = "icon_shield", Branch = 1, Tier = 1, Costs = new[] { 240 }, Requires = SkillId.ExtraLife },
    new SkillDefinition { Id = SkillId.Revive, Name = "ふっかつ", Description = "全滅しても1回だけ1匹で復活", Icon = "icon_revive", Branch = 1, Tier = 2, Costs = new[] { 500 }, Requires = SkillId.StartShield },

    new SkillDefinition { Id = SkillId.WidePaddle, Name = "ひろいバー", Description = "バーの幅+8%", Icon = "icon_wide", Branch = 2, Tier = 0, Costs = new[] { 50, 120, 220 } },
    new SkillDefinition { Id = SkillId.CoinBonus, Name = "きんうん", Description = "もらえるコイン+15%", Icon = "icon_coin", Branch = 2, Tier = 1, Costs = new[] { 80, 160, 300 }, Requires = SkillId.WidePaddle },
    new SkillDefinition { Id = SkillId.Reroll, Name = "えらびなおし", Description = "強化の3択を引き直せる回数+1", Icon = "icon_reroll", Branch = 2, Tier = 2, Costs = new[] { 150, 350 }, Requires = SkillId.CoinBonus },
  };

  public static SkillDefinition Get(SkillId id)
  {
    return System.Array.Find(All, definition => definition.Id == id);
  }

  public static bool IsUnlocked(SkillDefinition skill)
  {
    return skill.Requires == null || Progress.GetSkillLevel(skill.Requires.Value) > 0;
  }

  // 次のレベルの値段。最大レベルなら -1
  public static int NextCost(SkillDefinition skill)
  {
    int level = Progress.GetSkillLevel(skill.Id);
    return level >= skill.MaxLevel ? -1 : skill.Costs[level];
  }

  public static bool TryLevelUp(SkillDefinition skill)
  {
    int cost = NextCost(skill);
    if (cost < 0 || !IsUnlocked(skill) || !Progress.TrySpendCoins(cost))
    {
      return false;
    }
    Progress.SetSkillLevel(skill.Id, Progress.GetSkillLevel(skill.Id) + 1);
    return true;
  }
}
