using GoogleMobileAds.Api;
using UnityEngine;

// アプリ起動時に1度だけ広告SDKを初期化し、リワード広告を読み込んでおく
static class AdsBootstrap
{
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void Initialize()
  {
    MobileAds.Initialize(_ => { });
    // 初期化の完了を待たずに読み込みを頼んでも、SDK が完了後に読み込む
    RewardedAds.Preload();
  }
}
