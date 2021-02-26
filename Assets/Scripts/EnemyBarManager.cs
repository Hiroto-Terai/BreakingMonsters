using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBarManager : MonoBehaviour
{
  // 敵のバーのスピード
  public float speed = 3f;
  GameObject ball;
  Rigidbody2D ballrb;

  // Start is called before the first frame update
  void Start()
  {
    ball = GameObject.Find("Ball");
    ballrb = ball.GetComponent<Rigidbody2D>();
  }

  // Update is called once per frame
  void Update()
  {
    {
      if (SceneManager.GetActiveScene().name != "HardModePlay")
      {
        // ボールが右向きに動いている時
        if (ballrb.velocity.x > 0)
        {
          // 敵のバーも右向きに動く
          if (this.transform.position.x < 1.8)
          {
            this.transform.position += Vector3.right * Time.deltaTime * speed;
          }
        }
        // ボールが左向きに動いている時
        else if (ballrb.velocity.x < 0)
        {
          if (this.transform.position.x > -1.8)
          {
            this.transform.position += Vector3.left * Time.deltaTime * speed;
          }
        }
      }
      else if (SceneManager.GetActiveScene().name == "HardModePlay")
      {
        // ボールが右向きに動いている時
        if (ballrb.velocity.x > 0)
        {
          // 敵のバーも右向きに動く
          if (this.transform.position.x < 1.2)
          {
            this.transform.position += Vector3.right * Time.deltaTime * speed;
          }
        }
        // ボールが左向きに動いている時
        else if (ballrb.velocity.x < 0)
        {
          if (this.transform.position.x > -1.2)
          {
            this.transform.position += Vector3.left * Time.deltaTime * speed;
          }
        }
      }
    }
  }
}
