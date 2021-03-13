using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
  // プレイヤーの移動の速さ
  public float speed = 5f;
  Rigidbody2D myRigidBody2d;

  Vector2 tf;

  private float deltaX, deltaY;

  // Start is called before the first frame update
  void Start()
  {
    myRigidBody2d = GetComponent<Rigidbody2D>();
    tf = transform.position;
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKey(KeyCode.RightArrow))
    {
      if (transform.position.x < 1.8)
      {
        transform.position += Vector3.right * Time.deltaTime * speed;
      }
    }
    else if (Input.GetKey(KeyCode.LeftArrow))
    {
      if (transform.position.x > -1.8)
      {
        transform.position += Vector3.left * Time.deltaTime * speed;
      }
    }

    Vector2 direction = new Vector2(0, 0);

    if (Input.touchCount > 0)
    {
      Touch touch = Input.GetTouch(0);
      Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

      switch (touch.phase)
      {
        case TouchPhase.Began:
          // 画面に指が触れた時
          tf.x = touchPos.x;
          break;
        case TouchPhase.Moved:
          // 指を動かしている時
          tf.x = this.transform.position.x + touchPos.x * speed;
          break;
      }

      // プレイヤーの座標の取得と移動量
      Vector2 pos = this.transform.position;
      pos.x += touchPos.x;

      if (pos.x > 1.8)
      {
        pos.x = 1.8f;
      }
      else if (pos.x < -1.8)
      {
        pos.x = -1.8f;
      }
      // プレイヤーの新規位置とする
      transform.position = pos;
    }
  }
}
