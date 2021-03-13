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
    SceneManager.activeSceneChanged += OnActiveSceneChanged;
  }
  void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
  {
    this.bannerView.Destroy();
    Debug.Log("delete bottom ads");
  }

  private void RequestBanner()
  {
    string adUnitId = "ca-app-pub-1161341815062676/5784243718";

    // Create a 320x50 banner at the top of the screen.
    this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);

    // Create an empty ad request.
    AdRequest request = new AdRequest.Builder().Build();

    // Load the banner with the request.
    this.bannerView.LoadAd(request);
  }
}
