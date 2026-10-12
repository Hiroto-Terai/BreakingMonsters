using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerManager : MonoBehaviour
{
  // プレイヤーの移動の速さ
  public float speed = 5f;
  // 画面の端からはみ出さない範囲
  const float LimitX = 1.8f;
  // 一時停止ボタンなどUIの上から始まったタッチではプレイヤーを動かさない
  bool isTouchOnUi;

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKey(KeyCode.RightArrow))
    {
      if (transform.position.x < LimitX)
      {
        transform.position += Vector3.right * Time.deltaTime * speed;
      }
    }
    else if (Input.GetKey(KeyCode.LeftArrow))
    {
      if (transform.position.x > -LimitX)
      {
        transform.position += Vector3.left * Time.deltaTime * speed;
      }
    }

    if (Input.touchCount > 0)
    {
      Touch touch = Input.GetTouch(0);
      if (touch.phase == TouchPhase.Began)
      {
        isTouchOnUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
      }
      // 一時停止中はプレイヤーを動かさない
      if (isTouchOnUi || Time.timeScale == 0f)
      {
        return;
      }
      MoveTo(Camera.main.ScreenToWorldPoint(touch.position).x);
    }
  }

  // 指のx座標にプレイヤーを移動する
  void MoveTo(float x)
  {
    Vector2 pos = this.transform.position;
    pos.x = Mathf.Clamp(x, -LimitX, LimitX);
    this.transform.position = pos;
  }
}
