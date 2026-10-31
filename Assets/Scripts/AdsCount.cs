using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsCount : MonoBehaviour
{
  // インタースティシャル広告をゲームクリア/オーバー3回に1回表示するためのカウント
  public static int adsCount = 0;

  // ドメインリロード無効時もエディタ再生ごとに初期値から始めるため
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetStatics()
  {
    adsCount = 0;
  }

  // Start is called before the first frame update
  void Start()
  {
    DontDestroyOnLoad(this);
  }
}
