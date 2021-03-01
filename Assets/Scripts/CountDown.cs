using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
  public Text CountText;
  float countdown;
  int count;
  bool isCalledOnce;

  // Start is called before the first frame update
  void Start()
  {
    // update関数内で一回だけ呼び出すためのフラグを初期化
    isCalledOnce = false;
    countdown = 4f;
  }

  // Update is called once per frame
  void Update()
  {
    if (countdown >= 1)
    {
      countdown -= Time.deltaTime;
      if (countdown <= 1 && !isCalledOnce)
      {
        isCalledOnce = true;
        CountText.text = "";
        Time.timeScale = 1f;
        return;
      }
      count = (int)countdown;
      CountText.text = count.ToString();
    }

  }
}
