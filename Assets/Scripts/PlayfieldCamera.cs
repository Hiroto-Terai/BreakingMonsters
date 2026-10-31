using UnityEngine;

// プレイ画面のカメラ。他の物が位置を決める前に、フィールドが画面に収まるよう合わせる
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Camera))]
public class PlayfieldCamera : MonoBehaviour
{
  Vector2Int fittedScreenSize;

  void Awake()
  {
    Fit();
  }

  void OnDestroy()
  {
    Playfield.Clear();
  }

  // 端末を回したり、エディタで画面サイズを変えたときにも合わせ直す
  void Update()
  {
    if (fittedScreenSize.x != Screen.width || fittedScreenSize.y != Screen.height)
    {
      Fit();
    }
  }

  void Fit()
  {
    fittedScreenSize = new Vector2Int(Screen.width, Screen.height);
    bool hasTopBanner = FindFirstObjectByType<GoogleBannerAdsTop>() != null;
    bool isTwoPlayer = FindFirstObjectByType<VersusController>() != null;
    Playfield.FitCamera(GetComponent<Camera>(), hasTopBanner, isTwoPlayer);
  }
}
