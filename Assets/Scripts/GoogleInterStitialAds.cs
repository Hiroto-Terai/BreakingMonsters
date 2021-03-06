using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GoogleInterStitialAds : MonoBehaviour
{
  private void Start()
  {
    RequestInterstitial();
  }

  private InterstitialAd interstitial;

  private void RequestInterstitial()
  {
    string adUnitId = "ca-app-pub-3940256099942544/4411468910";

    // Initialize an InterstitialAd.
    this.interstitial = new InterstitialAd(adUnitId);

    // Create an empty ad request.
    AdRequest request = new AdRequest.Builder().Build();
    // Load the interstitial with the request.
    this.interstitial.LoadAd(request);
    if (this.interstitial.IsLoaded())
    {
      this.interstitial.Show();
    }
    interstitial.OnAdClosed += (handler, EventArgs) =>
        {
          // 後処理
          interstitial.Destroy();
          Debug.Log("delete ads!");
        };
  }
}
