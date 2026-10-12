using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class GoogleInterStitialAds : MonoBehaviour
{
  private void Start()
  {
    Debug.Log("OnEnable now");
    RequestInterstitial();
  }
  private InterstitialAd interstitial;

  private void RequestInterstitial()
  {
#if UNITY_IPHONE
    string adUnitId = "ca-app-pub-1161341815062676/3158080372";
#else
    string adUnitId = "unexpected_platform";
#endif

    // Initialize an InterstitialAd.
    this.interstitial = new InterstitialAd(adUnitId);
    interstitial.OnAdClosed += HandleOnAdClosed;

    // Create an empty ad request.
    AdRequest request = new AdRequest.Builder().Build();
    Debug.Log("Build now");
    // Load the interstitial with the request.
    this.interstitial.LoadAd(request);
    Debug.Log("LoadAd now");
  }
  // 広告を表示できたら true を返す（未ロードなら表示しない）
  public bool ShowAds()
  {
    if (!this.interstitial.IsLoaded())
    {
      return false;
    }
    Debug.Log("Show now");
    this.interstitial.Show();
    return true;
  }
  public void HandleOnAdClosed(object sender, EventArgs args)
  {
    Debug.Log("Destroy now");
    interstitial.Destroy();
    Time.timeScale = 1;
  }
}
