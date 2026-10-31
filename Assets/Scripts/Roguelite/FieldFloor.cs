using UnityEngine;

// 床の模様。背景の板の上に半透明で敷きつめる（板の色が透けるので、フィーバー中の赤い点滅もそのまま見える）
public static class FieldFloor
{
  const string ShapeKey = "floor_tile";
  // 壁やモンスターより奥に描く
  const int SortingOrder = -50;

  // プレイ画面のフィールドの内側に敷く
  public static void CreateOnField()
  {
    var center = new Vector3(0f, (Playfield.TopWallInnerY + Playfield.BottomWallInnerY) / 2f, 0f);
    var size = new Vector2(Playfield.InnerHalfWidth * 2f, Playfield.TopWallInnerY - Playfield.BottomWallInnerY);
    Create(center, size);
  }

  // カメラに映る画面全体に敷く（タイトルやマップの背景）
  public static void CreateCovering(Camera camera)
  {
    float height = camera.orthographicSize * 2f;
    Create(camera.transform.position, new Vector2(height * camera.aspect, height));
  }

  public static void Create(Vector3 center, Vector2 size)
  {
    var floor = new GameObject("FieldFloor").AddComponent<SpriteRenderer>();
    floor.sprite = PixelArt.TiledWorldSprite(ShapeKey);
    floor.drawMode = SpriteDrawMode.Tiled;
    floor.size = size;
    floor.sortingOrder = SortingOrder;
    floor.transform.position = new Vector3(center.x, center.y, 0f);
  }
}
