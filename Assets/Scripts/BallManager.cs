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
  public AudioClip PlayerColSound;
  public AudioClip EnemyColSound;
  // ボールの色を取得(透明化のため)
  SpriteRenderer spriteRenderer;

  // ハードステージでは一定時間ごとにボールが消えて、また現れる
  public bool IsFading { get; set; }
  const float VisibleSeconds = 5f;
  const float FadeOutSeconds = 1f;
  const float HiddenSeconds = 0.5f;
  const float FadeInSeconds = 0.3f;
  // 透明度をなめらかにせず段階的に変えて、ドットゲームらしい見た目にする
  const int FadeSteps = 4;
  float fadeCycleTime = 0f;

  // 水平に近すぎる角度(ラリーが長引く)や垂直に近すぎる角度(同じ所を往復する)を避ける
  const float MinAngleFromHorizontalDeg = 30f;
  const float MaxAngleFromHorizontalDeg = 80f;
  // プレイヤーで打ち返すたびに少しずつ速くする
  const float SpeedUpPerPaddleHit = 1.04f;

  // 残像（ドットの影を少し残す）
  const float AfterimageInterval = 0.04f;
  float afterimageTimer;

  void Awake()
  {
    myRigidbody2d = GetComponent<Rigidbody2D>();
    this.spriteRenderer = GetComponent<SpriteRenderer>();
  }

  public bool IsMoving => myRigidbody2d.linearVelocity != Vector2.zero;

  // 指定した向きに、最初の速さで打ち出す
  public void Launch(Vector2 direction)
  {
    // 元の作りでは (speed, speed) で打ち出していたので、その速さにそろえる
    float launchSpeed = speed * Mathf.Sqrt(2f);
    myRigidbody2d.linearVelocity = ClampDirection(direction) * launchSpeed;
  }

  public void Stop()
  {
    myRigidbody2d.linearVelocity = Vector2.zero;
  }

  // Update is called once per frame
  void Update()
  {
    UpdateFade();

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

    afterimageTimer -= Time.deltaTime;
    if (afterimageTimer <= 0f && spriteRenderer.color.a > 0f)
    {
      afterimageTimer = AfterimageInterval;
      HitEffects.Afterimage(spriteRenderer, transform.position);
    }
  }

  void UpdateFade()
  {
    if (!IsFading)
    {
      fadeCycleTime = 0f;
      spriteRenderer.color = Color.white;
      return;
    }
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
    alpha = Mathf.Round(alpha * FadeSteps) / FadeSteps;
    spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
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
      // プレイヤーから見たボールの方向を計算
      Vector2 direction = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;
      // 現在のボールの速さを取得して少し速くする
      float newSpeed = Mathf.Min(myRigidbody2d.linearVelocity.magnitude * SpeedUpPerPaddleHit, maxSpeed);
      // 速度を変更
      myRigidbody2d.linearVelocity = direction * newSpeed;
      if (StageRunController.Instance != null)
      {
        StageRunController.Instance.OnPaddleHit();
      }
    }

    // 敵のバーに当たった時
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
