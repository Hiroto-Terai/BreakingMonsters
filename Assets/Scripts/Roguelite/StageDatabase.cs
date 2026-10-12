using System.Collections.Generic;
using UnityEngine;

public enum StageType { Normal, Hard, Boss }

// 1つの階層の中身（敵の配置や強さ）
public class FloorData
{
  public readonly List<MonsterSpawn> Monsters = new List<MonsterSpawn>();
  public bool IsBossFloor;
  public int BossHp;
  public float BossMoveSpeed;
  public float BossShotInterval;
  public float BulletSpeed;
  public bool HasEnemyBar;
  public float EnemyBarSpeed;
  public float EnemyBarLimitX;
  public float BallMinSpeed;
  public float BallMaxSpeed;
  public bool IsBallFading;
}

public struct MonsterSpawn
{
  // x は横位置、y は敵の親オブジェクトからの高さ
  public Vector2 Offset;
  public int Hp;
}

// ステージ番号から、ステージ・階層の内容を決める（100ステージを手作業で作らずに済むように）
public static class StageDatabase
{
  public const int StageCount = 100;
  const int HardInterval = 3;
  const int BossInterval = 7;
  const int MaxMonsters = 15;

  // 敵を置ける段（敵の親オブジェクトからの高さ）。上の段から順に埋める（3段目は敵バーより手前で、狙いやすいが数が増える）
  static readonly float[] RowOffsetY = { 0f, 0.5f, -1.25f };
  static readonly float[] ColumnX = { -2f, -1f, 0f, 1f, 2f };

  public static StageType GetType(int stage)
  {
    // 7と3の両方の倍数（21, 42...）はボスを優先する
    if (stage % BossInterval == 0) return StageType.Boss;
    if (stage % HardInterval == 0) return StageType.Hard;
    return StageType.Normal;
  }

  public static int GetFloorCount(int stage)
  {
    if (stage <= 33) return 3;
    if (stage <= 66) return 4;
    return 5;
  }

  // 0（ステージ1）〜1（ステージ100）の難しさ
  static float Difficulty(int stage)
  {
    return (stage - 1) / (float)(StageCount - 1);
  }

  public static FloorData GetFloor(int stage, int floorIndex)
  {
    StageType type = GetType(stage);
    int floorCount = GetFloorCount(stage);
    float t = Difficulty(stage);
    bool isHard = type == StageType.Hard;
    var random = new System.Random(stage * 7919 + floorIndex * 104729);

    var floor = new FloorData
    {
      IsBossFloor = type == StageType.Boss && floorIndex == floorCount - 1,
      BallMinSpeed = 5f + t * 1.5f,
      BallMaxSpeed = 10f + t * 2f,
      IsBallFading = isHard && stage >= 6,
      HasEnemyBar = true,
      EnemyBarSpeed = 2.5f + t * 2.5f + (isHard ? 1f : 0f),
      EnemyBarLimitX = isHard ? 1.4f : 1.8f,
    };

    if (floor.IsBossFloor)
    {
      floor.HasEnemyBar = false;
      floor.BossHp = 20 + stage;
      floor.BossMoveSpeed = 1f + t * 1.5f;
      floor.BossShotInterval = Mathf.Max(1.2f, 2.6f - t * 1.4f);
      floor.BulletSpeed = 2.5f + t * 2f;
      return floor;
    }

    int count = 3 + Mathf.RoundToInt(t * 9) + floorIndex + (isHard ? 2 : 0);
    count = Mathf.Min(count, MaxMonsters);
    int maxHp = 1 + Mathf.FloorToInt(t * 3) + (isHard ? 1 : 0);
    foreach (var position in PickPositions(count, random))
    {
      floor.Monsters.Add(new MonsterSpawn { Offset = position, Hp = random.Next(1, maxHp + 1) });
    }
    return floor;
  }

  // 段ごとにランダムな順番で場所を選ぶ（同じステージは毎回同じ配置になる）
  static List<Vector2> PickPositions(int count, System.Random random)
  {
    var result = new List<Vector2>();
    foreach (float y in RowOffsetY)
    {
      var row = new List<Vector2>();
      foreach (float x in ColumnX)
      {
        row.Add(new Vector2(x, y));
      }
      for (int i = row.Count - 1; i > 0; i--)
      {
        int j = random.Next(i + 1);
        (row[i], row[j]) = (row[j], row[i]);
      }
      foreach (var position in row)
      {
        if (result.Count >= count) return result;
        result.Add(position);
      }
    }
    return result;
  }

  // ステージ全体の報酬コイン。階層ごとに少しずつもらえ、残りはクリア時のボーナス
  public static int GetStageReward(int stage)
  {
    float reward = 20 + stage * 3;
    switch (GetType(stage))
    {
      case StageType.Hard: reward *= 1.5f; break;
      case StageType.Boss: reward *= 2.5f; break;
    }
    return Mathf.RoundToInt(reward);
  }

  public static int GetFloorReward(int stage)
  {
    return Mathf.CeilToInt(GetStageReward(stage) * 0.6f / GetFloorCount(stage));
  }

  public static int GetClearBonus(int stage)
  {
    return GetStageReward(stage) - GetFloorReward(stage) * GetFloorCount(stage);
  }
}
