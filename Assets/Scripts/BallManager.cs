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
  // ボールの色を取得(透明化のため)
  SpriteRenderer spriteRenderer;
  TrailRenderer trail;
  CountDown countDown;
  bool isHardMode;

  // ハードモードの透明化のタイミング
  const float VisibleSeconds = 5f;
  const float FadeOutSeconds = 1f;
  const float HiddenSeconds = 0.5f;
  const float FadeInSeconds = 0.3f;
  float fadeCycleTime = 0f;

  // 水平に近すぎる角度(ラリーが長引く)や垂直に近すぎる角度(同じ所を往復する)を避ける
  const float MinAngleFromHorizontalDeg = 30f;
  const float MaxAngleFromHorizontalDeg = 80f;
  // プレイヤーで打ち返すたびに少しずつ速くする
  const float SpeedUpPerPaddleHit = 1.04f;

  const float TrailSeconds = 0.15f;
  const float TrailWidth = 0.12f;

  // Start is called before the first frame update
  void Start()
  {
    myRigidbody2d = GetComponent<Rigidbody2D>();
    // 現在地を取得
    myTransform = transform;
    isCalledOnce = false;
    this.spriteRenderer = GetComponent<SpriteRenderer>();
    string sceneName = SceneManager.GetActiveScene().name;
    isHardMode = sceneName == "HardModePlay";
    countDown = FindFirstObjectByType<CountDown>();
    if (countDown != null)
    {
      AddTrail();
    }
  }

  void AddTrail()
  {
    trail = gameObject.AddComponent<TrailRenderer>();
    trail.time = TrailSeconds;
    trail.startWidth = TrailWidth;
    trail.endWidth = 0f;
    trail.material = new Material(Shader.Find("Sprites/Default"));
    trail.sortingOrder = spriteRenderer.sortingOrder - 1;
    SetTrailAlpha(1f);
  }

  void SetTrailAlpha(float alpha)
  {
    trail.startColor = new Color(0.55f, 0.85f, 1f, 0.5f * alpha);
    trail.endColor = new Color(0.55f, 0.85f, 1f, 0f);
  }

  // Update is called once per frame
  void Update()
  {
    if (isHardMode)
    {
      UpdateHardModeFade();
    }

    // カウントダウン中はボールを放たない
    if (countDown != null && countDown.IsFinished && !isCalledOnce)
    {
      isCalledOnce = true;
      myRigidbody2d.linearVelocity = new Vector3(speed, speed, 0);
    }

    // 速度チェック
    Vector2 velocity = myRigidbody2d.linearVelocity;
    if (velocity == Vector2.zero)
    {
      return;
    }
    // Clampで速さを制限
    float clampedSpeed = Mathf.Clamp(velocity.magnitude, minSpeed, maxSpeed);
    // 移動する方向を一定の角度の範囲に制限する
    myRigidbody2d.linearVelocity = ClampDirection(velocity) * clampedSpeed;
  }

  // ハードモードでは一定時間ごとにボールが消えて、また現れる
  void UpdateHardModeFade()
  {
    fadeCycleTime += Time.deltaTime;
    float t = fadeCycleTime - VisibleSeconds;
    float alpha;
    if (t < 0f)
    {
      alpha = 1f;
    }
    else if (t < FadeOutSeconds)
    {
      alpha = 1f - t / FadeOutSeconds;
    }
    else if (t < FadeOutSeconds + HiddenSeconds)
    {
      alpha = 0f;
    }
    else if (t < FadeOutSeconds + HiddenSeconds + FadeInSeconds)
    {
      alpha = (t - FadeOutSeconds - HiddenSeconds) / FadeInSeconds;
    }
    else
    {
      alpha = 1f;
      fadeCycleTime = 0f;
    }
    spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
    if (trail != null)
    {
      SetTrailAlpha(alpha);
    }
  }

  Vector2 ClampDirection(Vector2 velocity)
  {
    float angle = Mathf.Atan2(Mathf.Abs(velocity.y), Mathf.Abs(velocity.x)) * Mathf.Rad2Deg;
    angle = Mathf.Clamp(angle, MinAngleFromHorizontalDeg, MaxAngleFromHorizontalDeg) * Mathf.Deg2Rad;
    float signX = velocity.x >= 0f ? 1f : -1f;
    float signY = velocity.y >= 0f ? 1f : -1f;
    return new Vector2(Mathf.Cos(angle) * signX, Mathf.Sin(angle) * signY);
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
      // 現在のボールの速さを取得して少し速くする
      float speed = Mathf.Min(myRigidbody2d.linearVelocity.magnitude * SpeedUpPerPaddleHit, maxSpeed);
      // 速度を変更
      myRigidbody2d.linearVelocity = direction * speed;
      if (GameSession.Instance != null)
      {
        GameSession.Instance.OnPaddleHit();
      }
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
