using UnityEngine;

// 上の壁。当たり判定の下端がフィールドの上端にそろうよう置き、縦長の画面でも上が空かないよう上へ長く伸ばす
public class TopBarPosition : MonoBehaviour
{
  const float WallHeight = 12f;

  void Start()
  {
    WallStretch.Stretch(transform, WallHeight, Playfield.TopWallInnerY, 1f);
  }
}
