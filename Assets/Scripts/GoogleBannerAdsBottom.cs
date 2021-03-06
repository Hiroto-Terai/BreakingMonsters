using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class GoogleBannerAdsBottom : MonoBehaviour
{
  private BannerView bannerView;

  // Start is called before the first frame update
  void Start()
  {
    // Google Mobile Ads SDKの初期化
    MobileAds.Initialize(initStatus => { });

    this.RequestBanner();
  }

  private void Update()
  {
  }

  private void RequestBanner()
  {
    string adUnitId = "ca-app-pub-3940256099942544/2934735716";

    // Create a 320x50 banner at the top of the screen.
    this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

    // Create an empty ad request.
    AdRequest request = new AdRequest.Builder().Build();

    // Load the banner with the request.
    this.bannerView.LoadAd(request);
  }

  public void DeleteBanner()
  {
    this.bannerView.Destroy();
  }
}
