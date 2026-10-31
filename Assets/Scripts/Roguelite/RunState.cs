using System.Collections.Generic;
using UnityEngine;

// いま挑戦中のステージの状態（ステージが終わると捨てる）
public static class RunState
{
  const int BaseLives = 3;
  const float WidePaddlePerUpgrade = 0.15f;
  const float WidePaddlePerSkill = 0.08f;
  const float GigaPaddleBonus = 0.4f;
  const float BigBallPerUpgrade = 0.2f;
  const float CoinBoostPerUpgrade = 0.25f;
  const float CoinBonusPerSkill = 0.15f;
  const float CriticalChancePerSkill = 0.1f;
  const float CriticalChancePerUpgrade = 0.08f;
  const float SplitBaseChance = 0.12f;
  const float SplitChancePerLevel = 0.08f;
  const float BallStormSplitBonus = 0.15f;
  const float ExplosionBaseRadius = 0.75f;
  const float ExplosionRadiusPerLevel = 0.12f;
  const float BigBangRadiusMultiplier = 1.6f;
  const float ShotBaseInterval = 1.5f;
  const float ShotIntervalPerLevel = 0.15f;
  const float ThunderStormIntervalMultiplier = 0.7f;
  // レベルアップに必要な経験値は 3, 5, 7, 9… と少しずつ増える（序盤はすぐ上がって気持ちいい）
  const int FirstLevelXp = 3;
  const int XpStepPerLevel = 2;

  // 次に遊ぶステージ（マップやNEXTボタンで選ぶ）
  public static int SelectedStage = 1;

  public static int Stage { get; private set; }
  public static int Lives { get; private set; }
  public static int MaxLives { get; private set; }
  public static int CoinsEarned { get; private set; }
  public static int RerollsLeft { get; private set; }
  public static bool HasRevive { get; private set; }
  public static int Level { get; private set; }
  public static int Xp { get; private set; }
  public static int Kills { get; private set; }
  public static int BestCombo { get; private set; }
  public static int Evolutions { get; private set; }
  static readonly Dictionary<UpgradeId, int> upgradeLevels = new Dictionary<UpgradeId, int>();
  // 取った順に並べて、画面に持っている強化を並べるのに使う
  static readonly List<UpgradeId> ownedOrder = new List<UpgradeId>();

  public static void Begin(int stage)
  {
    Stage = stage;
    upgradeLevels.Clear();
    ownedOrder.Clear();
    MaxLives = BaseLives + Progress.GetSkillLevel(SkillId.ExtraLife);
    Lives = MaxLives;
    CoinsEarned = 0;
    RerollsLeft = Progress.GetSkillLevel(SkillId.Reroll);
    HasRevive = Progress.GetSkillLevel(SkillId.Revive) > 0;
    Level = 1;
    Xp = 0;
    Kills = 0;
    BestCombo = 0;
    Evolutions = 0;
  }

  public static IReadOnlyList<UpgradeId> OwnedUpgrades => ownedOrder;

  // 種類の上限に数えるのは、進化前の通常の強化だけ（回復は数えない）
  public static int OwnedKindCount
  {
    get
    {
      int count = 0;
      foreach (var id in ownedOrder)
      {
        if (!Upgrades.Get(id).IsEvolution) count++;
      }
      return count;
    }
  }

  public static int GetUpgradeLevel(UpgradeId id)
  {
    return upgradeLevels.TryGetValue(id, out int level) ? level : 0;
  }

  public static bool Has(UpgradeId id)
  {
    return GetUpgradeLevel(id) > 0;
  }

  public static void AddUpgrade(UpgradeId id)
  {
    if (id == UpgradeId.Heal)
    {
      Lives = Mathf.Min(Lives + 1, MaxLives);
      return;
    }
    if (!Has(id))
    {
      ownedOrder.Add(id);
    }
    upgradeLevels[id] = GetUpgradeLevel(id) + 1;
    if (Upgrades.Get(id).IsEvolution)
    {
      Evolutions++;
    }
  }

  public static int XpToNextLevel => FirstLevelXp + (Level - 1) * XpStepPerLevel;

  // 経験値を足して、レベルが上がった回数を返す
  public static int AddXp(int amount)
  {
    Xp += amount;
    int levelUps = 0;
    while (Xp >= XpToNextLevel)
    {
      Xp -= XpToNextLevel;
      Level++;
      levelUps++;
    }
    return levelUps;
  }

  public static void CountKill()
  {
    Kills++;
  }

  public static void RecordCombo(int combo)
  {
    BestCombo = Mathf.Max(BestCombo, combo);
  }

  public static void LoseLife()
  {
    Lives = Mathf.Max(0, Lives - 1);
  }

  // 全滅時に「ふっかつ」があれば1匹で立て直す
  public static bool TryRevive()
  {
    if (!HasRevive)
    {
      return false;
    }
    HasRevive = false;
    Lives = 1;
    return true;
  }

  // 広告を見て、全滅から1匹で立て直す
  public static void ReviveByAd()
  {
    Lives = 1;
  }

  public static bool TryUseReroll()
  {
    if (RerollsLeft <= 0)
    {
      return false;
    }
    RerollsLeft--;
    return true;
  }

  // コインはスキルと強化の倍率をかけてから記録する
  public static int AddCoins(int baseAmount)
  {
    int amount = Mathf.RoundToInt(baseAmount * CoinMultiplier);
    CoinsEarned += amount;
    return amount;
  }

  static float CoinMultiplier =>
    1f + GetUpgradeLevel(UpgradeId.CoinBoost) * CoinBoostPerUpgrade + Progress.GetSkillLevel(SkillId.CoinBonus) * CoinBonusPerSkill;

  public static int BallDamage => 1 + GetUpgradeLevel(UpgradeId.PowerBall) + Progress.GetSkillLevel(SkillId.Power);

  public static float CriticalChance =>
    Progress.GetSkillLevel(SkillId.Critical) * CriticalChancePerSkill + GetUpgradeLevel(UpgradeId.Critical) * CriticalChancePerUpgrade;

  public static int BallCount => 1 + GetUpgradeLevel(UpgradeId.MultiBall) + Progress.GetSkillLevel(SkillId.TwinBall);

  public static float PaddleWidthScale =>
    1f + GetUpgradeLevel(UpgradeId.WidePaddle) * WidePaddlePerUpgrade + Progress.GetSkillLevel(SkillId.WidePaddle) * WidePaddlePerSkill
    + (Has(UpgradeId.GigaPaddle) ? GigaPaddleBonus : 0f);

  public static float BallSizeScale => 1f + GetUpgradeLevel(UpgradeId.BigBall) * BigBallPerUpgrade;

  // 階層ごとのバリアの回数
  public static int ShieldCharges =>
    GetUpgradeLevel(UpgradeId.Shield) + (Progress.GetSkillLevel(SkillId.StartShield) > 0 ? 1 : 0);

  public static float SplitChanceAt(int level)
  {
    return level > 0 ? SplitBaseChance + level * SplitChancePerLevel : 0f;
  }

  public static float SplitChance => SplitChanceAt(GetUpgradeLevel(UpgradeId.Split)) + (Has(UpgradeId.BallStorm) ? BallStormSplitBonus : 0f);

  public static int PierceCount => GetUpgradeLevel(UpgradeId.Pierce);

  public static int ExplosionDamageAt(int level)
  {
    return 1 + level / 2;
  }

  public static int ExplosionDamage => ExplosionDamageAt(GetUpgradeLevel(UpgradeId.Explosion));

  public static float ExplosionRadius =>
    (ExplosionBaseRadius + GetUpgradeLevel(UpgradeId.Explosion) * ExplosionRadiusPerLevel) * (Has(UpgradeId.BigBang) ? BigBangRadiusMultiplier : 1f);

  public static int ChainTargets => GetUpgradeLevel(UpgradeId.Chain);

  public static float ShotInterval =>
    (ShotBaseInterval - GetUpgradeLevel(UpgradeId.Shot) * ShotIntervalPerLevel) * (Has(UpgradeId.ThunderStorm) ? ThunderStormIntervalMultiplier : 1f);

  // Lv1:1発、Lv2:2発、Lv4:3発
  public static int ShotWays
  {
    get
    {
      int level = GetUpgradeLevel(UpgradeId.Shot);
      if (level >= 4) return 3;
      if (level >= 2) return 2;
      return 1;
    }
  }
}
