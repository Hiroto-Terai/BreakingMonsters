using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopBarPosition : MonoBehaviour
{
  private Camera _mainCamera;

  // Start is called before the first frame update
  void Start()
  {
    // カメラオブジェクトを取得
    GameObject obj = GameObject.Find("Main Camera");
    _mainCamera = obj.GetComponent<Camera>();
    if (Screen.currentResolution.height <= 1334)
    {
      this.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 1.06f, 10f));
    }
    else
    {
      this.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 1.09f, 10f));
    }
  }
}
