using UnityEngine;

// 上下の壁を、内側の端（ボールが跳ね返る面）を動かさずに外側へ伸ばす
public static class WallStretch
{
  // outward は外側の向き（上の壁は 1、下の壁は -1）
  public static void Stretch(Transform wall, float height, float innerY, float outward)
  {
    var collider = wall.GetComponent<BoxCollider2D>();
    Vector3 scale = wall.localScale;
    scale.y = height / collider.size.y;
    wall.localScale = scale;
    wall.position = new Vector3(0f, innerY + outward * height / 2f, 0f);
  }
}
