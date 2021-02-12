using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
  // ボールの移動の速さを指定する変数
  public float speed = 5f;
  // 速さの最小値を指定する変数を追加
  public float minSpeed = 5f;
  // 速さの最大値を指定する変数を追加
  public float maxSpeed = 10f;
  Rigidbody2D myRigidbody2d;
  Transform myTransform;

  // Start is called before the first frame update
  void Start()
  {
    myRigidbody2d = GetComponent<Rigidbody2D>();
    myRigidbody2d.velocity = new Vector3(speed, speed, 0);
    // 現在地を取得
    myTransform = transform;
  }

  // Update is called once per frame
  void Update()
  {
    // 速度チェック
    Vector2 velocity = myRigidbody2d.velocity;
    // 速さを計算
    // Clampで速さを制限
    // Mathf.Clamp(値, 最小値, 最大値)のように用いる
    float clampedSpeed = Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed);
    // 速度を変更
    // 大きさ1のベクトル(normalized) * 速さ
    myRigidbody2d.velocity = velocity.normalized * clampedSpeed;
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    // プレイヤーにあたった時に、跳ね返る方向を変える
    // タグで衝突相手を識別
    if (collision.gameObject.CompareTag("Player"))
    {
      // 現在のプレイヤーの位置を取得
      Vector2 playerPos = collision.transform.position;
      // ボールの位置を取得
      Vector2 ballPos = myTransform.position;
      // プレイヤーから見たボールの方向を計算
      Vector2 direction = (ballPos - playerPos).normalized;
      // 現在のボールの速さを取得
      float speed = myRigidbody2d.velocity.magnitude;
      // 速度を変更
      myRigidbody2d.velocity = direction * speed;
    }
  }
}
