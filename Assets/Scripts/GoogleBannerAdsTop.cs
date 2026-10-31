using GoogleMobileAds.Api;

public class GoogleBannerAdsTop : GoogleBannerAds
{
  protected override string AdUnitId => AdUnits.BannerTop;
  protected override AdPosition Position => AdPosition.Top;
}
