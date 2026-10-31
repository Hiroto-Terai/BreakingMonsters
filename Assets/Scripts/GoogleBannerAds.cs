using GoogleMobileAds.Api;
using UnityEngine;

// 画面の上か下に出すバナー広告。シーンを出るときに一緒に消す
public abstract class GoogleBannerAds : MonoBehaviour
{
  BannerView bannerView;

  protected abstract string AdUnitId { get; }
  protected abstract AdPosition Position { get; }

  void Start()
  {
    bannerView = new BannerView(AdUnitId, AdSize.Banner, Position);
    bannerView.LoadAd(new AdRequest());
  }

  void OnDestroy()
  {
    bannerView?.Destroy();
  }
}
