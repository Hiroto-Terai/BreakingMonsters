using UnityEngine;

// モードごとのベスト記録（スコア・星）を端末に保存する
public static class GameRecords
{
  public static readonly string[] ModeScenes = { "EasyModePlay", "NormalModePlay", "HardModePlay" };

  public static int GetBestScore(string modeScene)
  {
    return PlayerPrefs.GetInt("best_score_" + modeScene, 0);
  }

  public static int GetBestStars(string modeScene)
  {
    return PlayerPrefs.GetInt("best_stars_" + modeScene, 0);
  }

  // 更新があれば true を返す
  public static bool SaveIfBest(string modeScene, int score, int stars)
  {
    bool isNewBest = score > GetBestScore(modeScene);
    if (isNewBest)
    {
      PlayerPrefs.SetInt("best_score_" + modeScene, score);
    }
    if (stars > GetBestStars(modeScene))
    {
      PlayerPrefs.SetInt("best_stars_" + modeScene, stars);
    }
    PlayerPrefs.Save();
    return isNewBest;
  }

  // 次の難易度のシーン名。最後のモードなら null
  public static string GetNextModeScene(string modeScene)
  {
    int index = System.Array.IndexOf(ModeScenes, modeScene);
    if (index < 0 || index + 1 >= ModeScenes.Length)
    {
      return null;
    }
    return ModeScenes[index + 1];
  }

  public static string StarsText(int stars)
  {
    return new string('★', stars) + new string('☆', 3 - stars);
  }
}
