using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GoogleBannerAdsTop : MonoBehaviour
{
  private BannerView bannerView;

  // Start is called before the first frame update
  void Start()
  {
    // Google Mobile Ads SDKの初期化
    MobileAds.Initialize(initStatus => { });

    this.RequestBanner();
  }
  private void RequestBanner()
  {
    string adUnitId = "ca-app-pub-3940256099942544/2934735716";

    // Create a 320x50 banner at the top of the screen.
    this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);

    // Create an empty ad request.
    AdRequest request = new AdRequest.Builder().Build();

    // Load the banner with the request.
    this.bannerView.LoadAd(request);
  }
}
