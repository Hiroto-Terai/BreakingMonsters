﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopBarPosition : MonoBehaviour
{
  private Camera _mainCamera;

  // Start is called before the first frame update
  void Start()
  {
    // カメラオブジェクトを取得します
    GameObject obj = GameObject.Find("Main Camera");
    _mainCamera = obj.GetComponent<Camera>();
    this.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height, 10f));

    // this.transform.localPosition = new Vector2(0, Screen.currentResolution.height / 2);
  }

  // Update is called once per frame
  void Update()
  {

  }
}