using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵のバー：ボールが進む向きに合わせて左右に動き、敵スライムを守る
public class EnemyBarManager : MonoBehaviour
{
  // 敵のバーのスピード
  public float speed = 3f;
  // 左右に動ける範囲
  public float limitX = 1.8f;
  Rigidbody2D ballrb;

  // Start is called before the first frame update
  void Start()
  {
    ballrb = GameObject.Find("Ball").GetComponent<Rigidbody2D>();
  }

  // Update is called once per frame
  void Update()
  {
    // ボールが右向きに動いている時は右へ、左向きの時は左へ
    if (ballrb.linearVelocity.x > 0 && this.transform.position.x < limitX)
    {
      this.transform.position += Vector3.right * Time.deltaTime * speed;
    }
    else if (ballrb.linearVelocity.x < 0 && this.transform.position.x > -limitX)
    {
      this.transform.position += Vector3.left * Time.deltaTime * speed;
    }
  }
}
