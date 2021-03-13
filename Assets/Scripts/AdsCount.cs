using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsCount : MonoBehaviour
{
  // インタースティシャル広告をゲームクリア/オーバー3回に1回表示するためのカウント
  public static int adsCount = 0;

  // Start is called before the first frame update
  void Start()
  {
    DontDestroyOnLoad(this);
  }
}
