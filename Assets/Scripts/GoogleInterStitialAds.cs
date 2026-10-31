using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// 画面切り替えのときに挟む全画面広告。シーンに入ったら読み込んでおき、出せるときだけ出す
public class GoogleInterStitialAds : MonoBehaviour
{
  InterstitialAd interstitial;

  void Start()
  {
    // 広告SDKのコールバックは Unity のメインスレッド以外から届くことがあるので、ExecuteInUpdate で戻してから扱う
    InterstitialAd.Load(AdUnits.Interstitial, new AdRequest(), (ad, error) => MobileAdsEventExecutor.ExecuteInUpdate(() => OnLoaded(ad, error)));
  }

  void OnDestroy()
  {
    interstitial?.Destroy();
  }

  void OnLoaded(InterstitialAd ad, LoadAdError error)
  {
    if (error != null)
    {
      Debug.LogWarning("インタースティシャル広告を読み込めなかった: " + error.GetMessage());
      return;
    }
    // 読み込み中にシーンを出ていたら使わない
    if (this == null)
    {
      ad.Destroy();
      return;
    }
    interstitial = ad;
    interstitial.OnAdFullScreenContentClosed += () => MobileAdsEventExecutor.ExecuteInUpdate(OnClosed);
    interstitial.OnAdFullScreenContentFailed += _ => MobileAdsEventExecutor.ExecuteInUpdate(OnClosed);
  }

  // 広告を表示できたら true を返す（未ロードなら表示しない）
  public bool ShowAds()
  {
    if (interstitial == null || !interstitial.CanShowAd())
    {
      return false;
    }
    interstitial.Show();
    return true;
  }

  // 広告を出している間は時間を止めているので、閉じたら再開する
  void OnClosed()
  {
    Time.timeScale = 1f;
  }
}
