using GoogleMobileAds.Api;
using UnityEngine;

// バナー広告（320x50）が画面で占める高さ。バナーのある画面で、HUDやボタンと重ならないよう場所を空けるのに使う
public static class BannerLayout
{
  // 50dp のバナーに、少しすき間を足した高さ
  const float HeightDp = 56f;
  const float DefaultDpi = 160f;

  public static float HeightPixels => HeightDp * DeviceScale();

  // dp をピクセルに直す倍率。実機では AdMob がバナーの大きさを決めるのと同じ値（Android の density、iOS の scale）を使う。
  // Screen.dpi は実際の画素密度なので、タブレットなどではバナーより小さく見積もってしまう
  static float DeviceScale()
  {
    float scale = MobileAds.Utils.GetDeviceScale();
    if (scale > 0f)
    {
      return scale;
    }
    return (Screen.dpi > 0f ? Screen.dpi : DefaultDpi) / DefaultDpi;
  }
}
