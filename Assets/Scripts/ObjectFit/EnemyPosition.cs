using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPosition : MonoBehaviour
{
  private Camera _mainCamera;

  // Start is called before the first frame update
  void Start()
  {
    // カメラオブジェクトを取得します
    GameObject obj = GameObject.Find("Main Camera");
    _mainCamera = obj.GetComponent<Camera>();
    // iPhone6, 6S, 7, 8, SE2などの画面サイズ対応
    if (Screen.currentResolution.height <= 1334)
    {
      this.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height * 2.6f / 4, 10f));
    }
    else
    {
      this.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height * 2.5f / 4, 10f));
    }

  }
}
