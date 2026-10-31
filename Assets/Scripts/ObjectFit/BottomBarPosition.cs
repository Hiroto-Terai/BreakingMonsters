using UnityEngine;

// 下の壁（ここにボールが当たると、その先の仲間がやられる）。当たり判定の上端がフィールドの下端にそろうよう置き、
// ホームバーの下まで背景が見えないよう下へ長く伸ばす
public class BottomBarPosition : MonoBehaviour
{
  const float WallHeight = 6f;

  void Start()
  {
    WallStretch.Stretch(transform, WallHeight, Playfield.BottomWallInnerY, -1f);
  }
}
