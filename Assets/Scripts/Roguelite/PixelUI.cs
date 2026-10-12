using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// ドット風のUI部品（文字・パネル・ボタン・アイコン）をコードで組み立てる
public static class PixelUI
{
  // ドット文字がにじまないよう、フォントの元の大きさ(12)の倍数を使う
  public const int SmallFont = 36;
  public const int MediumFont = 48;
  public const int LargeFont = 72;
  public const int HugeFont = 96;
  // UIのドット1つを何ユニットで描くか
  const float UiPixelScale = 6f;
  const float CanvasReferencePixelsPerUnit = 100f;

  static Font font;

  public static Font Font
  {
    get
    {
      if (font == null)
      {
        // 画面の既存の文字からドットフォント（PixelMplus）を借りる
        foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
          if (text.font != null && text.font.name.StartsWith("PixelMplus"))
          {
            font = text.font;
            break;
          }
        }
        // 文字テクスチャは必要になってから作り直されるので、そのたびにドットのまま拡大されるようにする
        Font.textureRebuilt += SetPointFilter;
        SetPointFilter(font);
      }
      return font;
    }
  }

  static void SetPointFilter(Font target)
  {
    if (target == font && target.material != null && target.material.mainTexture != null)
    {
      target.material.mainTexture.filterMode = FilterMode.Point;
    }
  }

  // 既存の画面より手前に出す専用のキャンバス。既存UIと同じ基準解像度で拡大縮小する
  public static Canvas CreateOverlayCanvas(string name, int sortingOrder)
  {
    var gameObject = new GameObject(name, typeof(RectTransform));
    var canvas = gameObject.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = sortingOrder;
    var scaler = gameObject.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1334f, 750f);
    scaler.matchWidthOrHeight = 0f;
    gameObject.AddComponent<GraphicRaycaster>();
    return canvas;
  }

  public static RectTransform CreateRect(string name, Transform parent)
  {
    var gameObject = new GameObject(name, typeof(RectTransform));
    gameObject.transform.SetParent(parent, false);
    return gameObject.GetComponent<RectTransform>();
  }

  // 親の中での位置と大きさを、アンカー(0〜1)とサイズで指定する
  public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
  {
    rect.anchorMin = anchor;
    rect.anchorMax = anchor;
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = position;
    rect.sizeDelta = size;
    return rect;
  }

  public static RectTransform Stretch(RectTransform rect)
  {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
    return rect;
  }

  public static Text CreateText(Transform parent, string message, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
  {
    var rect = CreateRect("Text", parent);
    var text = rect.gameObject.AddComponent<Text>();
    text.font = Font;
    text.fontSize = fontSize;
    text.color = color;
    text.alignment = alignment;
    text.horizontalOverflow = HorizontalWrapMode.Wrap;
    text.verticalOverflow = VerticalWrapMode.Overflow;
    text.supportRichText = true;
    text.raycastTarget = false;
    text.text = message;
    return text;
  }

  public static Image CreatePanel(Transform parent, Color32 fill, Color32 border)
  {
    var rect = CreateRect("Panel", parent);
    var image = rect.gameObject.AddComponent<Image>();
    image.sprite = PixelArt.FrameSprite(fill, border);
    image.type = Image.Type.Sliced;
    // キャンバスは100ユニット=1ピクセル基準なので、枠のドット1つが UiPixelScale ユニットになるよう倍率を合わせる
    image.pixelsPerUnitMultiplier = CanvasReferencePixelsPerUnit / UiPixelScale;
    return image;
  }

  // 画面全体を覆う半透明の暗幕（後ろのボタンを押せないようにする）
  public static Image CreateDimmer(Transform parent, float alpha)
  {
    var rect = Stretch(CreateRect("Dimmer", parent));
    var image = rect.gameObject.AddComponent<Image>();
    image.color = new Color(0f, 0f, 0f, alpha);
    return image;
  }

  public static Image CreateIcon(Transform parent, string shapeKey, float pixelScale)
  {
    var sprite = PixelArt.UiSprite(shapeKey);
    var rect = CreateRect("Icon_" + shapeKey, parent);
    var image = rect.gameObject.AddComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    rect.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height) * pixelScale;
    return image;
  }

  public static Button CreateButton(Transform parent, string label, int fontSize, Color32 fill, UnityAction onClick)
  {
    var panel = CreatePanel(parent, fill, PixelArt.Pale);
    panel.name = "Button_" + label;
    var button = panel.gameObject.AddComponent<Button>();
    var colors = button.colors;
    colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
    colors.disabledColor = new Color(0.45f, 0.45f, 0.45f);
    button.colors = colors;
    button.onClick.AddListener(onClick);
    var text = CreateText(panel.transform, label, fontSize, PixelArt.White);
    Stretch(text.rectTransform);
    return button;
  }
}
