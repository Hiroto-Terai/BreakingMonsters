// AdMob の広告ユニットID
// Android は AdMob にアプリをまだ登録していないので、Google のテスト用IDを使う（本番用を作ったら差し替える）
// iOS のリワード広告も本番用をまだ作っていないので、テスト用IDを使う
public static class AdUnits
{
#if UNITY_ANDROID
  public const string BannerTop = "ca-app-pub-3940256099942544/6300978111";
  public const string BannerBottom = "ca-app-pub-3940256099942544/6300978111";
  public const string Interstitial = "ca-app-pub-3940256099942544/1033173712";
  public const string Rewarded = "ca-app-pub-3940256099942544/5224354917";
#else
  public const string BannerTop = "ca-app-pub-1161341815062676/5784243718";
  public const string BannerBottom = "ca-app-pub-1161341815062676/5784243718";
  public const string Interstitial = "ca-app-pub-1161341815062676/3158080372";
  public const string Rewarded = "ca-app-pub-3940256099942544/1712485313";
#endif
}
