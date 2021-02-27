using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
  public AudioClip PlayerColSound;
  public AudioClip EnemyColSound;
  bool isCalledOnce;

  // Start is called before the first frame update
  void Start()
  {
    myRigidbody2d = GetComponent<Rigidbody2D>();
    // 現在地を取得
    myTransform = transform;
    isCalledOnce = false;
  }

  // Update is called once per frame
  void Update()
  {
    if (SceneManager.GetActiveScene().name == "EasyModePlay" || SceneManager.GetActiveScene().name == "NormalModePlay" || SceneManager.GetActiveScene().name == "HardModePlay")
    {
      // カウントダウン中はボールを放たない
      if (GameObject.Find("CountText").GetComponent<Text>().text == "")
      {
        if (!isCalledOnce)
        {
          isCalledOnce = true;
          myRigidbody2d.velocity = new Vector3(speed, speed, 0);
        }
      }
    }

    // 速度チェック
    Vector2 velocity = myRigidbody2d.velocity;
    // 速さを計算
    // Clampで速さを制限
    // Mathf.Clamp(値, 最小値, 最大値)のように用いる
    // magnitudeでベクトルの大きさを計測
    float clampedSpeed = Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed);
    // 速度を変更
    // 大きさ1のベクトル(normalized) * 速さ
    myRigidbody2d.velocity = velocity.normalized * clampedSpeed;

    // 移動する方向のベクトルを正規化(大きさ1のベクトルに変換)
    Vector2 velocityNormalized = myRigidbody2d.velocity.normalized;

    //移動する方向を一定の範囲に制限する
    // 垂直方向は垂直から10度内の範囲には入らないようにする
    float limitVerticalDeg = 10f;
    // 水平方向は垂直から45度内の範囲には入らないようにする
    float limitHorizontalDeg = 45f;

    // x方向のベクトルが正の場合
    if (velocityNormalized.x >= 0f)
    {
      velocityNormalized.x = Mathf.Clamp(velocityNormalized.x, Mathf.Cos(Mathf.Deg2Rad * (90 - limitVerticalDeg)), Mathf.Cos(Mathf.Deg2Rad * (0 + limitHorizontalDeg)));
    }
    // x方向のベクトルが負の場合
    else
    {
      velocityNormalized.x = Mathf.Clamp(velocityNormalized.x, Mathf.Cos(Mathf.Deg2Rad * (180 - limitHorizontalDeg)), Mathf.Cos(Mathf.Deg2Rad * (90 + limitVerticalDeg)));
    }

    // y方向のベクトルが正の場合
    if (velocityNormalized.y >= 0f)
    {
      velocityNormalized.y = Mathf.Clamp(velocityNormalized.y, Mathf.Sin(Mathf.Deg2Rad * (0 + limitHorizontalDeg)), Mathf.Sin(Mathf.Deg2Rad * (90 + limitVerticalDeg)));
    }
    // y方向のベクトルが負の場合
    else
    {
      velocityNormalized.y = Mathf.Clamp(velocityNormalized.y, Mathf.Sin(Mathf.Deg2Rad * (270 - limitVerticalDeg)), Mathf.Sin(Mathf.Deg2Rad * (180 + limitHorizontalDeg)));
    }
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    // プレイヤーにあたった時に、跳ね返る方向を変える
    // タグで衝突相手を識別
    if (collision.gameObject.CompareTag("Player"))
    {
      if (OptionManager.isSePlaying == true)
      {
        AudioSource.PlayClipAtPoint(PlayerColSound, transform.position, 1.0f);
      }
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

    // 敵プレイヤーに当たった時
    // サウンドを鳴らす処理のみ
    if (collision.gameObject.CompareTag("Enemy"))
    {
      if (OptionManager.isSePlaying == true)
      {
        AudioSource.PlayClipAtPoint(EnemyColSound, transform.position, 1.0f);
      }
    }
  }
}
