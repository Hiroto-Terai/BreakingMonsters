using System.Collections.Generic;
using UnityEngine;

// ドット絵を文字の並びから作る。1文字 = 1ドットで、文字ごとに色を割り当てる
public static class PixelArt
{
  // ゲーム内のドット1つの大きさ（ワールド座標）。既存のスライムとほぼ同じ粒の大きさにそろえる
  public const float WorldPixelSize = 0.05f;
  const float WorldPixelsPerUnit = 1f / WorldPixelSize;

  public static readonly Color32 Clear = new Color32(0, 0, 0, 0);
  public static readonly Color32 Navy = new Color32(5, 8, 46, 255);
  public static readonly Color32 DarkTeal = new Color32(22, 52, 66, 255);
  public static readonly Color32 Teal = new Color32(46, 90, 110, 255);
  public static readonly Color32 LightBlue = new Color32(143, 217, 255, 255);
  public static readonly Color32 Pale = new Color32(214, 226, 255, 255);
  public static readonly Color32 White = new Color32(255, 255, 255, 255);
  public static readonly Color32 Yellow = new Color32(255, 224, 102, 255);
  public static readonly Color32 Orange = new Color32(255, 150, 60, 255);
  public static readonly Color32 Red = new Color32(255, 92, 92, 255);
  public static readonly Color32 DarkRed = new Color32(120, 24, 48, 255);
  public static readonly Color32 Purple = new Color32(70, 32, 110, 255);
  public static readonly Color32 Green = new Color32(110, 220, 120, 255);
  public static readonly Color32 Gray = new Color32(90, 96, 120, 255);
  public static readonly Color32 Black = new Color32(0, 0, 0, 255);

  static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

  // 共通の色の割り当て。絵ごとに同じ文字は同じ色になる
  static Color32 ColorOf(char c)
  {
    switch (c)
    {
      case 'n': return Navy;
      case 'd': return DarkTeal;
      case 't': return Teal;
      case 'b': return LightBlue;
      case 'p': return Pale;
      case 'w': return White;
      case 'y': return Yellow;
      case 'o': return Orange;
      case 'r': return Red;
      case 'R': return DarkRed;
      case 'u': return Purple;
      case 'g': return Green;
      case 'a': return Gray;
      case 'k': return Black;
      // ボス用の陰影・王冠の色
      case 'K': return new Color32(10, 20, 32, 255);
      case '1': return new Color32(22, 50, 64, 255);
      case '2': return new Color32(40, 86, 104, 255);
      case '3': return new Color32(70, 128, 146, 255);
      case 'P': return new Color32(255, 140, 160, 255);
      case 'q': return new Color32(255, 190, 200, 255);
      case 'Y': return new Color32(255, 236, 140, 255);
      case 'G': return new Color32(250, 190, 60, 255);
      case 'O': return new Color32(170, 100, 30, 255);
      default: return Clear;
    }
  }

  public static Texture2D MakeTexture(string[] rows)
  {
    int height = rows.Length;
    int width = rows[0].Length;
    var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    texture.filterMode = FilterMode.Point;
    texture.wrapMode = TextureWrapMode.Clamp;
    var pixels = new Color32[width * height];
    for (int y = 0; y < height; y++)
    {
      // 文字列は上から下、テクスチャは下から上に並ぶので反転する
      string row = rows[height - 1 - y];
      for (int x = 0; x < width; x++)
      {
        pixels[y * width + x] = ColorOf(row[x]);
      }
    }
    texture.SetPixels32(pixels);
    texture.Apply();
    return texture;
  }

  // ゲーム画面に置くためのスプライト（ドットの大きさをワールドでそろえる）
  public static Sprite WorldSprite(string key)
  {
    return WorldSprite(key, new Vector2(0.5f, 0.5f));
  }

  // pivot は絵のどこを位置の基準にするか（0.5, 0 なら下端の中央）
  public static Sprite WorldSprite(string key, Vector2 pivot)
  {
    return Get("world_" + key + pivot, () =>
    {
      var texture = MakeTexture(Shapes.Get(key));
      return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, WorldPixelsPerUnit);
    });
  }

  // UI用のスプライト
  public static Sprite UiSprite(string key)
  {
    return Get("ui_" + key, () =>
    {
      var texture = MakeTexture(Shapes.Get(key));
      return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
    });
  }

  // 枠付きパネル。四隅を崩さず伸ばせるよう 9 スライスにする
  public static Sprite FrameSprite(Color32 fill, Color32 border)
  {
    string key = "frame_" + fill + border;
    return Get(key, () =>
    {
      const int size = 12;
      const int borderWidth = 2;
      var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
      texture.filterMode = FilterMode.Point;
      var pixels = new Color32[size * size];
      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          bool isCorner = (x == 0 || x == size - 1) && (y == 0 || y == size - 1);
          bool isBorder = x < borderWidth || y < borderWidth || x >= size - borderWidth || y >= size - borderWidth;
          // 下側に1ドットの影を付けて、ボタンらしい厚みを出す
          bool isShadow = y == borderWidth && !isBorder;
          Color32 color = isBorder ? border : fill;
          if (isShadow)
          {
            color = new Color32((byte)(fill.r * 0.7f), (byte)(fill.g * 0.7f), (byte)(fill.b * 0.7f), fill.a);
          }
          pixels[y * size + x] = isCorner ? Clear : color;
        }
      }
      texture.SetPixels32(pixels);
      texture.Apply();
      var border4 = new Vector4(4, 4, 4, 4);
      return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect, border4);
    });
  }

  static Sprite Get(string key, System.Func<Sprite> create)
  {
    if (!cache.TryGetValue(key, out var sprite) || sprite == null)
    {
      sprite = create();
      cache[key] = sprite;
    }
    return sprite;
  }

  // 位置をドットの格子にそろえる（なめらかに動きすぎないようにする）
  public static Vector3 Snap(Vector3 position)
  {
    return new Vector3(
      Mathf.Round(position.x / WorldPixelSize) * WorldPixelSize,
      Mathf.Round(position.y / WorldPixelSize) * WorldPixelSize,
      position.z);
  }
}
