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
  public static bool isSePlaying;

  public GameObject Music;

  // Start is called before the first frame update
  void Start()
  {
    TwitterURL = "https://twitter.com/UVER_Hroxas";
    InstagramURL = "https://www.instagram.com/?hl=ja";
    PrivacyPolicyURL = "https://hiroto-terai.github.io/BreakingMonsters/";
    Music = GameObject.Find("Music");

    if (OptionManager.isBgmPlaying == true)
    {
      Music.GetComponent<AudioSource>().volume = 0.15f;
      this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }
    else
    {
      Music.GetComponent<AudioSource>().volume = 0f;
      this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void openOption()
  {
    OptionCanvas.SetActive(true);
    Time.timeScale = 0f;
  }

  public void closeOption()
  {
    OptionCanvas.SetActive(false);
    Time.timeScale = 1f;
  }

  // Twitterを開く
  public void openTwitter()
  {
    Application.OpenURL(TwitterURL);
  }

  //インスタを開く
  public void openInstagram()
  {
    Application.OpenURL(InstagramURL);
  }

  public void openPrivacyPolicy()
  {
    Application.OpenURL(PrivacyPolicyURL);
  }

  public void goToMenuScene()
  {
    SceneManager.LoadScene("Menu", LoadSceneMode.Single);
  }

  public void BgmController()
  {
    if (isBgmPlaying == true)
    {
      isBgmPlaying = false;
      Music.GetComponent<AudioSource>().volume = 0f;
      this.gameObject.transform.GetChild(1).gameObject.SetActive(true);
    }
    else
    {
      isBgmPlaying = true;
      Music.GetComponent<AudioSource>().volume = 0.15f;
      this.gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }

  }
}
