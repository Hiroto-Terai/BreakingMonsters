using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

// 見ると報酬がもらえる広告（ふっかつ・コイン2倍）。アプリ全体で1本を読み込んでおき、使ったら次を読み込む
public static class RewardedAds
{
  static RewardedAd loadedAd;
  static bool isLoading;

  public static bool IsReady => loadedAd != null && loadedAd.CanShowAd();

  // リワード広告を見た直後の画面切り替えでは、全画面広告を続けて出さない（切り替えのたびに false に戻す）
  public static bool HasJustWatched { get; set; }

  // ドメインリロード無効時もエディタ再生ごとに初期値から始めるため
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetStatics()
  {
    loadedAd = null;
    isLoading = false;
    HasJustWatched = false;
  }

  public static void Preload()
  {
    if (isLoading || loadedAd != null)
    {
      return;
    }
    isLoading = true;
    // 広告SDKのコールバックは Unity のメインスレッド以外から届くことがあるので、ExecuteInUpdate で戻してから扱う
    RewardedAd.Load(AdUnits.Rewarded, new AdRequest(), (ad, error) => MobileAdsEventExecutor.ExecuteInUpdate(() => OnLoaded(ad, error)));
  }

  static void OnLoaded(RewardedAd ad, LoadAdError error)
  {
    isLoading = false;
    if (error != null)
    {
      Debug.LogWarning("リワード広告を読み込めなかった: " + error.GetMessage());
      return;
    }
    loadedAd = ad;
  }

  // 最後まで見てもらえたら、広告を閉じた後に onRewarded を呼ぶ。途中で閉じたら onSkipped を呼ぶ
  public static void Show(Action onRewarded, Action onSkipped)
  {
    var ad = loadedAd;
    loadedAd = null;
    bool isRewarded = false;
    float previousTimeScale = Time.timeScale;
    Time.timeScale = 0f;

    void Finish()
    {
      ad.Destroy();
      Time.timeScale = previousTimeScale;
      Preload();
      if (isRewarded)
      {
        HasJustWatched = true;
        onRewarded();
      }
      else
      {
        onSkipped();
      }
    }

    ad.OnAdFullScreenContentClosed += () => MobileAdsEventExecutor.ExecuteInUpdate(Finish);
    ad.OnAdFullScreenContentFailed += _ => MobileAdsEventExecutor.ExecuteInUpdate(Finish);
    ad.Show(_ => isRewarded = true);
  }
}
