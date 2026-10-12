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
    countdown = 4f;
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
      Time.timeScale = 1f;
      return;
    }
    count = (int)countdown;
    CountText.text = count.ToString();
  }
}
