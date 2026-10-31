using UnityEngine;

// プレイ画面のフィールドの寸法。画面の縦横比にかかわらず、物はすべてこのワールド座標に置き、
// カメラのほうをフィールドに合わせる（端末によって間隔や難しさが変わらないようにする）
public static class Playfield
{
  // 左右の壁の内側は x = ±2.5（壁の当たり判定は Scene に置いてある）
  public const float InnerHalfWidth = 2.5f;
  // 壁を少しだけ見せて、フィールドの端だと分かるようにする
  const float VisibleWallThickness = 0.25f;

  public const float TopWallInnerY = 4.86f;
  public const float BottomWallInnerY = -4.86f;
  public const float PlayerY = -2.79f;
  public const float PlayerLivesY = -3.56f;
  // バリアは仲間スライムと自分のバーの間に張る（バーより上だと、打ち返す前に壊れてしまう）
  public const float ShieldY = -3.2f;
  public const float BallStartY = -2.17f;
  public const float EnemyBarY = 2.61f;
  public const float MonstersRootY = 3.38f;

  // STAGE表示などを並べる帯の高さ（画面の高さに対する割合）
  const float HudBandRatio = 0.045f;
  const float MinHudBandPixels = 80f;

  static float FieldWidth => (InnerHalfWidth + VisibleWallThickness) * 2f;
  static float FieldHeight => TopWallInnerY - BottomWallInnerY + VisibleWallThickness * 2f;

  // フィールドの上端のすぐ上にある、HUDを置く帯の中心（スクリーン座標、下が0）
  public static float HudCenterScreenY { get; private set; }
  public static float HudBandScreenHeight { get; private set; }
  public static bool IsFitted { get; private set; }

  // ドメインリロード無効時もエディタ再生ごとに初期値から始めるため
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetStatics()
  {
    IsFitted = false;
  }

  public static void Clear()
  {
    IsFitted = false;
  }

  // セーフエリアから上のバナーとHUDの帯を除いた範囲に、フィールドがぴったり収まるようカメラを合わせる
  public static void FitCamera(Camera camera, bool hasTopBanner, bool isTwoPlayer)
  {
    Rect safe = Screen.safeArea;
    float banner = hasTopBanner ? BannerLayout.HeightPixels : 0f;
    float hud = Mathf.Max(MinHudBandPixels, Screen.height * HudBandRatio);
    float bottom = safe.yMin;
    // AdMob のバナーはノッチの下（セーフエリアの上端）から下向きに出るので、その下にHUDの帯を取る
    float top = safe.yMax - banner - hud;

    float unitsPerPixel = Mathf.Max(FieldWidth / safe.width, FieldHeight / (top - bottom));
    camera.orthographicSize = Screen.height * unitsPerPixel / 2f;
    // 縦長の画面で余る高さは、1人で遊ぶときはフィールドの下に回す（指をそこに置けば、ボールや仲間が指で隠れない）。
    // 2人で遊ぶときは、どちらの指の置き場も同じになるよう上下に分ける
    float spare = top - bottom - FieldHeight / unitsPerPixel;
    float fieldTopScreenY = isTwoPlayer ? top - spare / 2f : top;
    float fieldCenterScreenY = fieldTopScreenY - FieldHeight / unitsPerPixel / 2f;
    float cameraY = (TopWallInnerY + BottomWallInnerY) / 2f - (fieldCenterScreenY - Screen.height / 2f) * unitsPerPixel;
    float fieldCenterScreenX = safe.center.x;
    float cameraX = -(fieldCenterScreenX - Screen.width / 2f) * unitsPerPixel;
    camera.transform.position = new Vector3(cameraX, cameraY, camera.transform.position.z);

    // HUD はフィールドの上端（見えている壁の上）に乗せる
    HudBandScreenHeight = hud;
    HudCenterScreenY = fieldTopScreenY + hud / 2f;
    IsFitted = true;
  }
}
