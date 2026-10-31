using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
  const float GoDisplaySeconds = 0.6f;

  public Text CountText;
  float countdown;
  int count;
  float goTimer;

  // カウントダウンが終わってボールを放ってよいか
  public bool IsFinished { get; private set; }

  // Start is called before the first frame update
  void Start()
  {
    Restart();
  }

  const float FullCountdownStart = 4f;

  // 3, 2, 1, GO! を最初からやり直す（階層が変わるたびに使う）
  public void Restart()
  {
    Restart(FullCountdownStart);
  }

  // startValue が 4 なら「3,2,1」、2.5 なら「2,1」と短くなる
  public void Restart(float startValue)
  {
    countdown = startValue;
    count = 0;
    goTimer = 0f;
    IsFinished = false;
  }

  // Update is called once per frame
  void Update()
  {
    if (IsFinished)
    {
      // 「GO!」を少しだけ表示してから消す
      if (goTimer > 0f)
      {
        goTimer -= Time.deltaTime;
        if (goTimer <= 0f)
        {
          CountText.text = "";
        }
      }
      return;
    }

    countdown -= Time.deltaTime;
    if (countdown <= 1)
    {
      IsFinished = true;
      goTimer = GoDisplaySeconds;
      CountText.text = "GO!";
      SoundEffects.Play(Sfx.CountdownGo);
      Time.timeScale = 1f;
      return;
    }
    // 始まった瞬間（4.0）や一時停止中でも「4」を出さないよう、残り秒数を切り上げて数える
    int nextCount = Mathf.CeilToInt(countdown - 1f);
    // 数字が変わった瞬間だけ鳴らす
    if (nextCount != count)
    {
      SoundEffects.Play(Sfx.Countdown);
    }
    count = nextCount;
    CountText.text = count.ToString();
  }
}
