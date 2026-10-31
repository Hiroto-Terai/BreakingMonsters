using UnityEngine;

// フィールドのふち。壁の内側の端に沿って細い段差を描き、ボールが跳ね返る面をくっきり見せる
public static class FieldFrame
{
  // ふちの太さ（3ドット）。カメラに映る壁の幅より細くして、フィールドの中にははみ出さない
  const float Thickness = 3 * PixelArt.WorldPixelSize;
  // 壁（-10）より手前、モンスター（0）より奥に描く。いちばん上の段のモンスターは頭が壁にかかるので、ふちで隠さないようにする
  const int SortingOrder = -5;
  const string VerticalShape = "frame_vertical";
  const string HorizontalShape = "frame_horizontal";

  public static void Create()
  {
    float top = Playfield.TopWallInnerY;
    float bottom = Playfield.BottomWallInnerY;
    float half = Playfield.InnerHalfWidth;
    var sideSize = new Vector2(Thickness, top - bottom);
    var edgeSize = new Vector2(half * 2f + Thickness * 2f, Thickness);
    float middleY = (top + bottom) / 2f;
    // 絵は縦なら右側、横なら下側がフィールドに面しているので、反対側のふちは反転させる
    CreateStrip(VerticalShape, new Vector2(-half - Thickness / 2f, middleY), sideSize, false, false);
    CreateStrip(VerticalShape, new Vector2(half + Thickness / 2f, middleY), sideSize, true, false);
    CreateStrip(HorizontalShape, new Vector2(0f, top + Thickness / 2f), edgeSize, false, false);
    CreateStrip(HorizontalShape, new Vector2(0f, bottom - Thickness / 2f), edgeSize, false, true);
  }

  static void CreateStrip(string shapeKey, Vector2 center, Vector2 size, bool isFlippedX, bool isFlippedY)
  {
    var strip = new GameObject("FieldFrame").AddComponent<SpriteRenderer>();
    strip.sprite = PixelArt.TiledWorldSprite(shapeKey);
    strip.drawMode = SpriteDrawMode.Tiled;
    strip.size = size;
    strip.flipX = isFlippedX;
    strip.flipY = isFlippedY;
    strip.sortingOrder = SortingOrder;
    strip.transform.position = new Vector3(center.x, center.y, 0f);
  }
}
