using UnityEngine;

// 端末に保存する進行状況（コイン・クリア状況・スキルレベル）
public static class Progress
{
  const string CoinsKey = "rl_coins";
  const string ClearedKey = "rl_cleared_stage";
  const string StarsKeyPrefix = "rl_stars_";
  const string SkillKeyPrefix = "rl_skill_";

  public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

  // クリア済みの一番大きいステージ番号（0なら未クリア）
  public static int HighestCleared => PlayerPrefs.GetInt(ClearedKey, 0);

  public static bool IsUnlocked(int stage)
  {
    return stage <= HighestCleared + 1;
  }

  public static int GetStars(int stage)
  {
    return PlayerPrefs.GetInt(StarsKeyPrefix + stage, 0);
  }

  public static void AddCoins(int amount)
  {
    PlayerPrefs.SetInt(CoinsKey, Coins + amount);
    PlayerPrefs.Save();
  }

  public static bool TrySpendCoins(int amount)
  {
    if (Coins < amount)
    {
      return false;
    }
    PlayerPrefs.SetInt(CoinsKey, Coins - amount);
    PlayerPrefs.Save();
    return true;
  }

  public static void MarkCleared(int stage, int stars)
  {
    if (stage > HighestCleared)
    {
      PlayerPrefs.SetInt(ClearedKey, stage);
    }
    if (stars > GetStars(stage))
    {
      PlayerPrefs.SetInt(StarsKeyPrefix + stage, stars);
    }
    PlayerPrefs.Save();
  }

  public static int GetSkillLevel(SkillId id)
  {
    return PlayerPrefs.GetInt(SkillKeyPrefix + id, 0);
  }

  public static void SetSkillLevel(SkillId id, int level)
  {
    PlayerPrefs.SetInt(SkillKeyPrefix + id, level);
    PlayerPrefs.Save();
  }
}
