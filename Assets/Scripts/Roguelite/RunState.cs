using System.Collections.Generic;
using UnityEngine;

// いま挑戦中のステージの状態（ステージが終わると捨てる）
public static class RunState
{
  const int BaseLives = 3;
  const float WidePaddlePerUpgrade = 0.2f;
  const float WidePaddlePerSkill = 0.08f;
  const float BigBallPerUpgrade = 0.25f;
  const float SlowBallPerUpgrade = 0.1f;
  const float CoinBoostPerUpgrade = 0.25f;
  const float CoinBonusPerSkill = 0.15f;
  const float CriticalChancePerSkill = 0.1f;

  // 次に遊ぶステージ（マップやNEXTボタンで選ぶ）
  public static int SelectedStage = 1;

  public static int Stage { get; private set; }
  public static int Lives { get; private set; }
  public static int MaxLives { get; private set; }
  public static int CoinsEarned { get; private set; }
  public static int RerollsLeft { get; private set; }
  public static bool HasRevive { get; private set; }
  static readonly Dictionary<UpgradeId, int> upgradeLevels = new Dictionary<UpgradeId, int>();

  public static void Begin(int stage)
  {
    Stage = stage;
    upgradeLevels.Clear();
    MaxLives = BaseLives + Progress.GetSkillLevel(SkillId.ExtraLife);
    Lives = MaxLives;
    CoinsEarned = 0;
    RerollsLeft = Progress.GetSkillLevel(SkillId.Reroll);
    HasRevive = Progress.GetSkillLevel(SkillId.Revive) > 0;
  }

  public static int GetUpgradeLevel(UpgradeId id)
  {
    return upgradeLevels.TryGetValue(id, out int level) ? level : 0;
  }

  public static void AddUpgrade(UpgradeId id)
  {
    if (id == UpgradeId.Heal)
    {
      Lives = Mathf.Min(Lives + 1, MaxLives);
      return;
    }
    upgradeLevels[id] = GetUpgradeLevel(id) + 1;
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

  public static float CriticalChance => Progress.GetSkillLevel(SkillId.Critical) * CriticalChancePerSkill;

  public static int BallCount => 1 + GetUpgradeLevel(UpgradeId.MultiBall) + Progress.GetSkillLevel(SkillId.TwinBall);

  public static float PaddleWidthScale =>
    1f + GetUpgradeLevel(UpgradeId.WidePaddle) * WidePaddlePerUpgrade + Progress.GetSkillLevel(SkillId.WidePaddle) * WidePaddlePerSkill;

  public static float BallSizeScale => 1f + GetUpgradeLevel(UpgradeId.BigBall) * BigBallPerUpgrade;

  public static float BallMaxSpeedScale => 1f - GetUpgradeLevel(UpgradeId.SlowBall) * SlowBallPerUpgrade;

  public static bool HasFloorShield => GetUpgradeLevel(UpgradeId.Shield) > 0 || Progress.GetSkillLevel(SkillId.StartShield) > 0;
}
