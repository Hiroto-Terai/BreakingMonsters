using UnityEngine;

public enum PaddleSide { Bottom, Top }

// バー（自分のバー・敵のバーの両方）に付ける。動いている速さを測り、ボールを打つときの「振り」に使う
public class Paddle : MonoBehaviour
{
  // 速さが急に跳ねないよう、少しずつならす
  const float VelocitySmoothing = 0.35f;

  public PaddleSide Side = PaddleSide.Bottom;
  // 対戦モードのキャラクター（ローグライトでは None）
  public CharacterId Character = CharacterId.None;
  // 当たったボールをどれだけ相手の仲間スライムへ向けるか（0:ふつうに跳ね返す 〜 1:まっすぐ狙う）
  public float AimStrength;

  public float VelocityX { get; private set; }
  float lastX;

  void Start()
  {
    lastX = transform.position.x;
  }

  void LateUpdate()
  {
    if (Time.deltaTime <= 0f)
    {
      return;
    }
    float rawVelocity = (transform.position.x - lastX) / Time.deltaTime;
    VelocityX = Mathf.Lerp(VelocityX, rawVelocity, VelocitySmoothing);
    lastX = transform.position.x;
  }

  // 相手側（打ち返す先）が上か下か
  public float ForwardY => Side == PaddleSide.Bottom ? 1f : -1f;
}
