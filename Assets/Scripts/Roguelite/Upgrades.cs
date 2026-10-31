using System;
using System.Collections.Generic;

// ステージ中にレベルアップや宝箱で強くなる強化（ステージが終わるとリセット）。
// どれもレベルが上がるほど強くなり、最大レベルにして相棒の強化も持っていると「進化」できる
public enum UpgradeId
{
  MultiBall, PowerBall, WidePaddle, BigBall, Heal, Shield, CoinBoost,
  Split, Pierce, Explosion, Chain, Shot, Critical,
  // 進化
  BallStorm, Meteor, BigBang, ThunderStorm, GigaPaddle,
}

public class UpgradeDefinition
{
  public UpgradeId Id;
  public string Name;
  public string Icon;
  public int MaxLevel;
  // 次のレベルにしたときの効果の説明（引数は次のレベル）
  public Func<int, string> Describe;
  // このステージ番号に着いたら、選べるようになる
  public int UnlockStage = 1;
  public bool IsEvolution;
  // 進化の条件：EvolvesFrom を最大レベルにして、Partner を1つでも持っている
  public UpgradeId EvolvesFrom;
  public UpgradeId Partner;
}

public static class Upgrades
{
  public const int ChoiceCount = 3;
  // 同時に持てる強化の種類（ビルドを絞らせて、組み合わせを考える楽しさを出す）
  public const int MaxOwnedKinds = 6;

  public static readonly UpgradeDefinition[] All =
  {
    new UpgradeDefinition { Id = UpgradeId.MultiBall, Name = "マルチボール", Icon = "icon_multiball", MaxLevel = 4, Describe = lv => "ボール+1（ぜんぶで" + (lv + 1) + "こ）" },
    new UpgradeDefinition { Id = UpgradeId.PowerBall, Name = "パワーボール", Icon = "icon_power", MaxLevel = 4, Describe = lv => "こうげき力+1" },
    new UpgradeDefinition { Id = UpgradeId.WidePaddle, Name = "ワイドバー", Icon = "icon_wide", MaxLevel = 4, Describe = lv => "バーの はば+15%" },
    new UpgradeDefinition { Id = UpgradeId.BigBall, Name = "ビッグボール", Icon = "icon_bigball", MaxLevel = 3, Describe = lv => "ボールが20%大きくなる" },
    new UpgradeDefinition { Id = UpgradeId.Heal, Name = "なかま回復", Icon = "icon_heal", MaxLevel = 99, Describe = lv => "やられた なかまが1匹もどる" },
    new UpgradeDefinition { Id = UpgradeId.Shield, Name = "バリア", Icon = "icon_shield", MaxLevel = 3, Describe = lv => "かいそうごとに" + lv + "回 ボールをはね返す" },
    new UpgradeDefinition { Id = UpgradeId.CoinBoost, Name = "コインざくざく", Icon = "icon_coin", MaxLevel = 3, Describe = lv => "もらえるコイン+25%" },
    new UpgradeDefinition { Id = UpgradeId.Split, Name = "ぶんれつ", Icon = "icon_split", MaxLevel = 5, UnlockStage = 2, Describe = lv => "当てると " + Percent(RunState.SplitChanceAt(lv)) + "で ミニボールがふえる" },
    new UpgradeDefinition { Id = UpgradeId.Shot, Name = "ショット", Icon = "icon_shot", MaxLevel = 5, UnlockStage = 3, Describe = lv => lv == 1 ? "バーから 弾をうつ" : lv == 2 || lv == 4 ? "弾の数+1" : "うつ はやさUP" },
    new UpgradeDefinition { Id = UpgradeId.Pierce, Name = "つらぬき", Icon = "icon_pierce", MaxLevel = 5, UnlockStage = 4, Describe = lv => "たおすと そのまま" + lv + "体まで つらぬく" },
    new UpgradeDefinition { Id = UpgradeId.Explosion, Name = "ばくはつ", Icon = "icon_explosion", MaxLevel = 4, UnlockStage = 5, Describe = lv => "たおすと まわりが ばくはつ（" + RunState.ExplosionDamageAt(lv) + "ダメージ）" },
    new UpgradeDefinition { Id = UpgradeId.Chain, Name = "いなずま", Icon = "icon_chain", MaxLevel = 4, UnlockStage = 6, Describe = lv => "当てると 近くの" + lv + "体に 電撃" },
    new UpgradeDefinition { Id = UpgradeId.Critical, Name = "かいしん", Icon = "icon_crit", MaxLevel = 5, UnlockStage = 8, Describe = lv => "かいしん率+8%（3ばいダメージ）" },

    new UpgradeDefinition { Id = UpgradeId.BallStorm, Name = "ボールのあらし", Icon = "icon_storm", MaxLevel = 1, IsEvolution = true, EvolvesFrom = UpgradeId.MultiBall, Partner = UpgradeId.Split, Describe = lv => "打ち返すたびに ミニボールが2こ出る" },
    new UpgradeDefinition { Id = UpgradeId.Meteor, Name = "メテオ", Icon = "icon_meteor", MaxLevel = 1, IsEvolution = true, EvolvesFrom = UpgradeId.PowerBall, Partner = UpgradeId.Pierce, Describe = lv => "スマッシュで 3ばいの火の玉になる" },
    new UpgradeDefinition { Id = UpgradeId.BigBang, Name = "ビッグバン", Icon = "icon_bigbang", MaxLevel = 1, IsEvolution = true, EvolvesFrom = UpgradeId.Explosion, Partner = UpgradeId.Critical, Describe = lv => "ばくはつが 大きくなって 連鎖する" },
    new UpgradeDefinition { Id = UpgradeId.ThunderStorm, Name = "サンダーストーム", Icon = "icon_thunderstorm", MaxLevel = 1, IsEvolution = true, EvolvesFrom = UpgradeId.Chain, Partner = UpgradeId.Shot, Describe = lv => "弾が 3体に 電撃をとばす" },
    new UpgradeDefinition { Id = UpgradeId.GigaPaddle, Name = "ギガバー", Icon = "icon_gigapaddle", MaxLevel = 1, IsEvolution = true, EvolvesFrom = UpgradeId.WidePaddle, Partner = UpgradeId.Shield, Describe = lv => "バーが大きくなり レーザーをうつ" },
  };

  static string Percent(float ratio)
  {
    return Math.Round(ratio * 100f) + "%";
  }

  public static UpgradeDefinition Get(UpgradeId id)
  {
    return Array.Find(All, definition => definition.Id == id);
  }

  // この強化が進化に関わるなら「○○と組み合わせて進化」の説明を返す（関わらなければ null）
  public static string EvolutionHint(UpgradeDefinition definition)
  {
    foreach (var evolution in All)
    {
      if (!evolution.IsEvolution || RunState.Has(evolution.Id)) continue;
      if (evolution.EvolvesFrom == definition.Id)
      {
        return "★" + Get(evolution.Partner).Name + "と組むと「" + evolution.Name + "」";
      }
      if (evolution.Partner == definition.Id)
      {
        return "★" + Get(evolution.EvolvesFrom).Name + "MAXで「" + evolution.Name + "」";
      }
    }
    return null;
  }

  public static bool IsUnlocked(UpgradeDefinition definition)
  {
    return definition.UnlockStage <= Progress.HighestCleared + 1;
  }

  public static bool CanEvolve(UpgradeDefinition evolution)
  {
    if (!evolution.IsEvolution || RunState.GetUpgradeLevel(evolution.Id) > 0)
    {
      return false;
    }
    var source = Get(evolution.EvolvesFrom);
    return RunState.GetUpgradeLevel(source.Id) >= source.MaxLevel && RunState.GetUpgradeLevel(evolution.Partner) > 0;
  }

  // 次のステージで新しく出るようになる強化（結果画面で「つぎは これが出る」と見せる）
  public static UpgradeDefinition NextUnlock(int clearedStage)
  {
    foreach (var definition in All)
    {
      // 最初から選べる強化（UnlockStage = 1）は「新しく出る」ものとして見せない
      if (!definition.IsEvolution && definition.UnlockStage > 1 && definition.UnlockStage == clearedStage + 1)
      {
        return definition;
      }
    }
    return null;
  }

  static List<UpgradeDefinition> Candidates()
  {
    var candidates = new List<UpgradeDefinition>();
    bool isFull = RunState.OwnedKindCount >= MaxOwnedKinds;
    foreach (var definition in All)
    {
      int level = RunState.GetUpgradeLevel(definition.Id);
      if (definition.IsEvolution || level >= definition.MaxLevel || !IsUnlocked(definition)) continue;
      // 回復はやられた仲間がいるときだけ候補にする
      if (definition.Id == UpgradeId.Heal && RunState.Lives >= RunState.MaxLives) continue;
      // 種類がいっぱいなら、持っているものを伸ばすしかない
      if (isFull && level == 0 && definition.Id != UpgradeId.Heal) continue;
      candidates.Add(definition);
    }
    return candidates;
  }

  // 3択を作る。進化できるものがあれば必ず先頭に出す。持っている強化は少し出やすくして、育てる流れを作る
  public static List<UpgradeDefinition> Roll(Random random)
  {
    var result = new List<UpgradeDefinition>();
    foreach (var definition in All)
    {
      if (CanEvolve(definition) && result.Count < ChoiceCount)
      {
        result.Add(definition);
      }
    }
    var candidates = Candidates();
    while (result.Count < ChoiceCount && candidates.Count > 0)
    {
      int index = PickWeighted(candidates, random);
      result.Add(candidates[index]);
      candidates.RemoveAt(index);
    }
    return result;
  }

  // 宝箱の中身：進化できるならそれ、進化の相方が足りないならその相方、なければ持っている強化のどれかを1レベル上げる（なければ新しいもの）
  public static UpgradeDefinition RollChest(Random random)
  {
    foreach (var definition in All)
    {
      if (CanEvolve(definition))
      {
        return definition;
      }
    }
    var candidates = Candidates().FindAll(definition => definition.Id != UpgradeId.Heal);
    var partner = candidates.Find(IsMissingPartner);
    if (partner != null)
    {
      return partner;
    }
    var owned = candidates.FindAll(definition => RunState.GetUpgradeLevel(definition.Id) > 0);
    var pool = owned.Count > 0 ? owned : candidates;
    return pool.Count > 0 ? pool[random.Next(pool.Count)] : Get(UpgradeId.Heal);
  }

  const int OwnedWeight = 3;
  const int NewWeight = 2;
  const int EvolutionPartnerWeight = 8;

  // 元の強化を最大にしたのに相方を持っていないと進化できないので、相方をぐっと出やすくして「はまる」瞬間を作る
  static bool IsMissingPartner(UpgradeDefinition definition)
  {
    if (RunState.GetUpgradeLevel(definition.Id) > 0)
    {
      return false;
    }
    foreach (var evolution in All)
    {
      if (!evolution.IsEvolution || evolution.Partner != definition.Id || RunState.Has(evolution.Id))
      {
        continue;
      }
      var source = Get(evolution.EvolvesFrom);
      if (RunState.GetUpgradeLevel(source.Id) >= source.MaxLevel)
      {
        return true;
      }
    }
    return false;
  }

  static int Weight(UpgradeDefinition definition)
  {
    if (IsMissingPartner(definition))
    {
      return EvolutionPartnerWeight;
    }
    return RunState.GetUpgradeLevel(definition.Id) > 0 ? OwnedWeight : NewWeight;
  }

  static int PickWeighted(List<UpgradeDefinition> candidates, Random random)
  {
    int total = 0;
    foreach (var definition in candidates)
    {
      total += Weight(definition);
    }
    int roll = random.Next(total);
    for (int i = 0; i < candidates.Count; i++)
    {
      roll -= Weight(candidates[i]);
      if (roll < 0)
      {
        return i;
      }
    }
    return candidates.Count - 1;
  }
}
