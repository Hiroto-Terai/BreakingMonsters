using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
  // プレイヤーの移動の速さ
  public float speed = 10f;
  Rigidbody2D myRigidBody2d;

  private float deltaX, deltaY;

  // Start is called before the first frame update
  void Start()
  {
    myRigidBody2d = GetComponent<Rigidbody2D>();
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

    // プレイヤーの移動(スマホでのタッチ操作)
    if (Input.touchCount > 0)
    {
      // タッチ情報の取得
      Touch touch = Input.GetTouch(0);

      Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);

      switch (touch.phase)
      {
        case TouchPhase.Began:
          Debug.Log(touch.phase);
          Debug.Log(touchPos);
          deltaX = touchPos.x - transform.position.x;
          deltaY = touchPos.y - transform.position.y;
          break;

        case TouchPhase.Moved:
          myRigidBody2d.MovePosition(new Vector2(touchPos.x - deltaX, touchPos.y - deltaY));
          Debug.Log(this.transform.position);
          break;

        case TouchPhase.Ended:
          myRigidBody2d.velocity = Vector2.zero;
          break;
      }
    }
  }
}
