using GoogleMobileAds.Api;

public class GoogleBannerAdsBottom : GoogleBannerAds
{
  protected override string AdUnitId => AdUnits.BannerBottom;
  protected override AdPosition Position => AdPosition.Bottom;
}
