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
}
