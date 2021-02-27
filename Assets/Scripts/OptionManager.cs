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

  public GameObject Music;
  public AudioClip ButtonSound;
  private string sceneNameForRetry;

  // Start is called before the first frame update
  void Start()
  {
    TwitterURL = "https://twitter.com/hirot_gamedev";
    PrivacyPolicyURL = "https://hiroto-terai.github.io/BreakingMonsters/";
    Music = GameObject.Find("Music");

    if (OptionManager.isBgmPlaying == true)
    {
      Music.GetComponent<AudioSource>().volume = 0.15f;
    }
    else
    {
      Music.GetComponent<AudioSource>().volume = 0f;
    }
    Debug.Log(isSePlaying);
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void openOption()
  {
    playButtonSound();
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
    Time.timeScale = 1f;
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
    Time.timeScale = 1f;
    playButtonSound();
    FadeManager.Instance.LoadScene("Menu", 0.3f);
  }
  public void Retry()
  {
    Time.timeScale = 1f;
    playButtonSound();
    sceneNameForRetry = SceneManager.GetActiveScene().name;
    FadeManager.Instance.LoadScene(sceneNameForRetry, 0.3f);
  }

  public void BgmController()
  {
    if (isBgmPlaying == true)
    {
      isBgmPlaying = false;
      playButtonSound();
      Music.GetComponent<AudioSource>().volume = 0f;
      this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }
    else
    {
      isBgmPlaying = true;
      playButtonSound();
      Music.GetComponent<AudioSource>().volume = 0.15f;
      this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
  }

  public void SeController()
  {
    if (isSePlaying == true)
    {
      isSePlaying = false;
      playButtonSound();
      this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }
    else
    {
      isSePlaying = true;
      playButtonSound();
      this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
  }

  public void playButtonSound()
  {
    if (isSePlaying == true)
    {
      AudioSource.PlayClipAtPoint(ButtonSound, GameObject.Find("Main Camera").transform.position);
    }
  }
}
