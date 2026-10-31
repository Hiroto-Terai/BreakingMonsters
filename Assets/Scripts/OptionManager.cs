using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
{
  // オプションキャンバスのゲームオブジェクト
  public GameObject OptionCanvas;
  private string TwitterURL;
  private string InstagramURL;
  private string PrivacyPolicyURL;

  // BGMが再生されているか
  public static bool isBgmPlaying = true;

  // SEが再生されているか
  public static bool isSePlaying = true;

  const string BgmPrefsKey = "option_bgm";
  const string SePrefsKey = "option_se";
  const float BgmVolume = 0.15f;
  const float FadeSeconds = 0.3f;
  const int InterstitialStartStage = 3;

  // アプリ起動時（エディタでは再生のたび）に保存済みの設定を読み込む
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void LoadSettings()
  {
    isBgmPlaying = PlayerPrefs.GetInt(BgmPrefsKey, 1) == 1;
    isSePlaying = PlayerPrefs.GetInt(SePrefsKey, 1) == 1;
    wasTimeStoppedBeforeOption = false;
  }

  static void SaveSettings()
  {
    PlayerPrefs.SetInt(BgmPrefsKey, isBgmPlaying ? 1 : 0);
    PlayerPrefs.SetInt(SePrefsKey, isSePlaying ? 1 : 0);
    PlayerPrefs.Save();
  }

  public GameObject Music;

  public GameObject GoogleInterStitialAds;

  // Start is called before the first frame update
  void Start()
  {
    TwitterURL = "https://twitter.com/hirot_gamedev";
    PrivacyPolicyURL = "https://hiroto-terai.github.io/BreakingMonsters/";
    Music = GameObject.Find("Music");
    // 結果画面のボタンが出る時点ではBGMが止められていて見つからない
    if (Music == null)
    {
      return;
    }
    Music.GetComponent<AudioSource>().volume = isBgmPlaying ? BgmVolume : 0f;
  }
  // 宝箱やレベルアップの選択中（時間が止まっている）に開いたかどうか。閉じたときにその状態へ戻す。
  // 開くのは一時停止ボタン、閉じるのはメニューの中のボタンで、それぞれ別の OptionManager なので共有する
  static bool wasTimeStoppedBeforeOption;

  // Android の戻るボタンで一時停止メニューを開け閉めする。
  // OptionManager はメニューの中のボタンごとにも付いているので、メニューの外にある一時停止ボタンのものだけが受け持つ
  void Update()
  {
    if (!Input.GetKeyDown(KeyCode.Escape) || OptionCanvas == null || transform.IsChildOf(OptionCanvas.transform))
    {
      return;
    }
    if (OptionCanvas.activeSelf)
    {
      closeOption();
    }
    else
    {
      openOption();
    }
  }

  public void openOption()
  {
    playButtonSound();
    wasTimeStoppedBeforeOption = Time.timeScale == 0f;
    Time.timeScale = 0f;
    OptionCanvas.SetActive(true);

    if (isBgmPlaying != true)
    {
      GameObject.Find("BGM").transform.GetChild(1).gameObject.SetActive(true);
    }
    if (isSePlaying != true)
    {
      GameObject.Find("SE").transform.GetChild(1).gameObject.SetActive(true);
    }
  }

  public void closeOption()
  {
    // 選択中に開いたなら止めたまま戻す（そのまま動かすと、強化を選んでいる間にボールが進んでしまう）。
    // ヒットストップなどの途中だったときは、戻す処理が一時停止中に済んでいることがあるので、ふつうの速さに戻す
    Time.timeScale = wasTimeStoppedBeforeOption ? 0f : 1f;
    playButtonSound();
    OptionCanvas.SetActive(false);
  }

  // Twitterを開く
  public void openTwitter()
  {
    playButtonSound();
    Application.OpenURL(TwitterURL);
  }

  public void openPrivacyPolicy()
  {
    playButtonSound();
    Application.OpenURL(PrivacyPolicyURL);
  }

  public void goToMenuScene()
  {
    GoToScene("Menu");
  }

  public void Retry()
  {
    GoToScene(SceneManager.GetActiveScene().name);
  }

  // 3回に1回は広告を挟んでからシーンを切り替える。広告を出したら true を返す
  public bool GoToScene(string sceneName)
  {
    // 連打で二重に遷移しないようにする
    if (FadeManager.Instance.IsFading)
    {
      return false;
    }
    playButtonSound();

    AdsCount.adsCount += 1;
    // 遊び始めの数ステージは広告を挟まず、面白さに入りこんでもらう
    bool isAdTurn = AdsCount.adsCount % 3 == 0 && Progress.HighestCleared >= InterstitialStartStage;
    bool isAdShown = isAdTurn && !RewardedAds.HasJustWatched && GoogleInterStitialAds.GetComponent<GoogleInterStitialAds>().ShowAds();
    RewardedAds.HasJustWatched = false;
    // 広告の閉じるボタンが一時停止画面の暗幕に隠れて押せなくならないよう、一時停止画面は閉じておく
    if (isAdShown && OptionCanvas != null)
    {
      OptionCanvas.SetActive(false);
    }
    // 広告表示中は時間を止め、広告を閉じたときに再開する（広告が出せなければ止めない）
    Time.timeScale = isAdShown ? 0f : 1f;
    FadeManager.Instance.LoadScene(sceneName, FadeSeconds);
    return isAdShown;
  }

  public void BgmController()
  {
    if (isBgmPlaying == true)
    {
      isBgmPlaying = false;
      playButtonSound();
      Music.GetComponent<AudioSource>().volume = 0f;
      SaveSettings();
      this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }
    else
    {
      isBgmPlaying = true;
      playButtonSound();
      Music.GetComponent<AudioSource>().volume = BgmVolume;
      SaveSettings();
      this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
  }

  public void SeController()
  {
    if (isSePlaying == true)
    {
      isSePlaying = false;
      playButtonSound();
      SaveSettings();
      this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }
    else
    {
      isSePlaying = true;
      playButtonSound();
      SaveSettings();
      this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
  }

  public void playButtonSound()
  {
    SoundEffects.Play(Sfx.Button);
  }
}
