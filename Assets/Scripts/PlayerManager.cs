using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// プレイヤーのバーを指（またはキー・マウス）で動かす
public class PlayerManager : MonoBehaviour
{
  // キーで動かすときの速さ
  public float speed = 8f;
  // 画面の端からはみ出さない範囲（バーの幅が変わると変える）
  public float LimitX = 1.8f;
  // 1台で2人で遊ぶとき：上側のプレイヤーか、画面の自分の側の半分だけで操作するか
  public bool IsTopPlayer;
  public bool UsesHalfScreen;

  const int NoFinger = -1;
  int fingerId = NoFinger;
  Vector3 lastMousePosition;

  // Update is called once per frame
  void Update()
  {
    // 一時停止中はプレイヤーを動かさない
    if (Time.timeScale == 0f)
    {
      return;
    }
    MoveByKeys();
    if (Input.touchCount > 0)
    {
      MoveByTouch();
    }
    else if (!Input.touchSupported)
    {
      MoveByMouse();
    }
  }

  // エディタやPCでは、マウスカーソルの位置にバーがついてくる（クリック不要）
  void MoveByMouse()
  {
    Vector3 mouse = Input.mousePosition;
    bool hasMoved = mouse != lastMousePosition;
    lastMousePosition = mouse;
    bool isInsideScreen = mouse.x >= 0f && mouse.x <= Screen.width && mouse.y >= 0f && mouse.y <= Screen.height;
    // キーで動かしているときは、止まっているカーソルに引き戻されないようにする
    if (!hasMoved || !isInsideScreen || (UsesHalfScreen && !IsInMyArea(mouse)))
    {
      return;
    }
    MoveTo(Camera.main.ScreenToWorldPoint(mouse).x);
  }

  void MoveByKeys()
  {
    KeyCode right = IsTopPlayer ? KeyCode.D : KeyCode.RightArrow;
    KeyCode left = IsTopPlayer ? KeyCode.A : KeyCode.LeftArrow;
    if (Input.GetKey(right))
    {
      MoveTo(transform.position.x + Time.deltaTime * speed);
    }
    else if (Input.GetKey(left))
    {
      MoveTo(transform.position.x - Time.deltaTime * speed);
    }
  }

  // 自分の指を1本だけ追いかける（2人で遊ぶときに相手の指で動かないようにする）
  void MoveByTouch()
  {
    for (int i = 0; i < Input.touchCount; i++)
    {
      Touch touch = Input.GetTouch(i);
      if (fingerId == NoFinger && touch.phase == TouchPhase.Began && IsInMyArea(touch.position) && !IsPointerOverUi(touch.fingerId))
      {
        fingerId = touch.fingerId;
      }
      if (touch.fingerId != fingerId)
      {
        continue;
      }
      if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
      {
        fingerId = NoFinger;
        continue;
      }
      MoveTo(Camera.main.ScreenToWorldPoint(touch.position).x);
    }
  }

  bool IsInMyArea(Vector2 screenPosition)
  {
    if (!UsesHalfScreen)
    {
      return true;
    }
    bool isUpperHalf = screenPosition.y > Screen.height / 2f;
    return isUpperHalf == IsTopPlayer;
  }

  // 一時停止ボタンなどUIの上から始まったタッチではプレイヤーを動かさない
  static bool IsPointerOverUi(int pointerId)
  {
    return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
  }

  // 指のx座標にプレイヤーを移動する
  void MoveTo(float x)
  {
    Vector2 pos = this.transform.position;
    pos.x = Mathf.Clamp(x, -LimitX, LimitX);
    this.transform.position = pos;
  }
}
