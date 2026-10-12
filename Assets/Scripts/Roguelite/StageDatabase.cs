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
  public AiStyle EnemyStyle;
  // 敵のバーの強さ（0〜1）
  public float EnemyLevel;
  public float EnemyBarLimitX;
  // 何回打ち返すごとにモンスターが一段迫ってくるか（0なら迫らない）
  public int AdvanceEveryHits;
  public float AdvanceStep;
  public float BallMinSpeed;
  public float BallMaxSpeed;
  public bool IsBallFading;
}

public struct MonsterSpawn
{
  // x は横位置、y は敵の親オブジェクトからの高さ
  public Vector2 Offset;
  public int Hp;
  public Element Element;
  // 横に歩く速さ（0なら歩かない）と歩く幅
  public float WalkSpeed;
  public float WalkRange;
}

// ステージ番号から、ステージ・階層の内容を決める（100ステージを手作業で作らずに済むように）
public static class StageDatabase
{
  public const int StageCount = 100;
  const int HardInterval = 3;
  const int BossInterval = 7;
  const int MaxMonsters = 15;
  const float AdvanceStepY = 0.3f;
  // 属性モンスター・歩くモンスターが出始めるステージと、出る割合の上限
  const int ElementStartStage = 4;
  const int WalkerStartStage = 5;
  const float MaxSpecialRatio = 0.5f;
  const int AdvanceStartStage = 3;
  const int AdvanceNormalStage = 10;
  const int SlowAdvanceEveryHits = 6;
  const float MonsterWidth = 0.9f;
  const float MinWalkRange = 0.2f;
  const float MaxWalkRange = 0.8f;

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
      BallMaxSpeed = 7.5f + t * 4.5f,
      IsBallFading = isHard && stage >= 6,
      HasEnemyBar = true,
      EnemyStyle = GetEnemyStyle(stage),
      // 対戦の「つよい」ほどにはしない（ステージ100でも 0.8 くらい）
      EnemyLevel = Mathf.Clamp01(t * 0.65f + (isHard ? 0.12f : 0f) + floorIndex * 0.01f),
      EnemyBarLimitX = isHard ? 1.4f : 1.8f,
      AdvanceEveryHits = GetAdvanceEveryHits(stage, isHard),
      AdvanceStep = AdvanceStepY,
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
    float elementRatio = Mathf.Clamp01((stage - ElementStartStage + 1) / 30f) * MaxSpecialRatio;
    float walkerRatio = Mathf.Clamp01((stage - WalkerStartStage + 1) / 40f) * MaxSpecialRatio;
    var positions = PickPositions(count, random);
    foreach (var position in positions)
    {
      var spawn = new MonsterSpawn { Offset = position, Hp = random.Next(1, maxHp + 1) };
      if (random.NextDouble() < elementRatio)
      {
        spawn.Element = (Element)random.Next(1, 4);
      }
      float walkRange = FreeWalkRange(position, positions);
      if (walkRange >= MinWalkRange && random.NextDouble() < walkerRatio)
      {
        spawn.WalkSpeed = 0.8f + (float)random.NextDouble() * 0.8f;
        spawn.WalkRange = walkRange;
      }
      floor.Monsters.Add(spawn);
    }
    return floor;
  }

  // 同じ段の隣のモンスターと重ならずに歩ける幅（隣も歩くかもしれないので、すき間の半分まで）
  static float FreeWalkRange(Vector2 position, List<Vector2> all)
  {
    float nearest = MaxWalkRange * 2f + MonsterWidth;
    foreach (var other in all)
    {
      if (other != position && Mathf.Approximately(other.y, position.y))
      {
        nearest = Mathf.Min(nearest, Mathf.Abs(other.x - position.x));
      }
    }
    return Mathf.Min(MaxWalkRange, (nearest - MonsterWidth) / 2f);
  }

  // 序盤は迫ってこない → ゆっくり迫る → ふつうに迫る、と少しずつ慣れてもらう
  static int GetAdvanceEveryHits(int stage, bool isHard)
  {
    if (stage < AdvanceStartStage) return 0;
    if (stage < AdvanceNormalStage) return SlowAdvanceEveryHits;
    return isHard ? 3 : 4;
  }

  // 序盤は守るだけの相手、ハードは狙ってくる相手、それ以外は守り型とフェイント型を交互にする
  static AiStyle GetEnemyStyle(int stage)
  {
    if (stage < 4) return AiStyle.Guard;
    if (GetType(stage) == StageType.Hard) return AiStyle.Sniper;
    return stage % 2 == 0 ? AiStyle.Trick : AiStyle.Guard;
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
