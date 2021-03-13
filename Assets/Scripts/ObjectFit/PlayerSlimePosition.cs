using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlimePosition : MonoBehaviour
{
  private Camera _mainCamera;

  // Start is called before the first frame update
  void Start()
  {
    // カメラオブジェクトを取得します
    GameObject obj = GameObject.Find("Main Camera");
    _mainCamera = obj.GetComponent<Camera>();
    this.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height * 1 / 8, 10f));
  }
}
