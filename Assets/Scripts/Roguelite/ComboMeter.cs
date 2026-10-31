using System;

// 連続で倒した数（コンボ）。少しのあいだ倒さないと途切れる。一定数ごとに「フィーバー」になる
public class ComboMeter
{
  const float KeepSeconds = 2.2f;
  const int FeverEvery = 25;
  const float FeverSeconds = 6f;

  public int Count { get; private set; }
  public bool IsFever => feverTimer > 0f;

  readonly Action<int> onChanged;
  readonly Action onFeverStart;
  readonly Action onFeverEnd;
  float keepTimer;
  float feverTimer;

  public ComboMeter(Action<int> onChanged, Action onFeverStart, Action onFeverEnd)
  {
    this.onChanged = onChanged;
    this.onFeverStart = onFeverStart;
    this.onFeverEnd = onFeverEnd;
  }

  public void AddKill()
  {
    Count++;
    keepTimer = KeepSeconds;
    if (Count % FeverEvery == 0)
    {
      feverTimer = FeverSeconds;
      onFeverStart();
    }
    onChanged(Count);
  }

  public void Break()
  {
    if (Count == 0)
    {
      return;
    }
    Count = 0;
    onChanged(Count);
  }

  public void Tick(float deltaTime)
  {
    if (Count > 0)
    {
      keepTimer -= deltaTime;
      if (keepTimer <= 0f)
      {
        Break();
      }
    }
    if (feverTimer > 0f)
    {
      feverTimer -= deltaTime;
      if (feverTimer <= 0f)
      {
        onFeverEnd();
      }
    }
  }

  public void EndFever()
  {
    if (feverTimer > 0f)
    {
      feverTimer = 0f;
      onFeverEnd();
    }
  }
}
