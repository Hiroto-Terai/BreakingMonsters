using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClear : MonoBehaviour
{
  public Text gameClearText;
  Transform myTransform;
  bool isGameClear = false;
  // BGMを持っているオブジェクト
  public GameObject Music;
  // ゲームクリアサウンド
  // update関数内で一回だけ呼び出すためのフラグを用意
  public AudioClip GameClearSound;
  bool isCalledOnce;

  // Start is called before the first frame update
  void Start()
  {
    myTransform = transform;
    // update関数内で一回だけ呼び出すためのフラグを初期化
    isCalledOnce = false;
  }

  // Update is called once per frame
  void Update()
  {
    if (myTransform.childCount == 0)
    {
      gameClearText.text = "Game Clear!!";
      // BGMの停止
      Music.SetActive(false);
      // ゲームクリアのSEを鳴らす
      if (!isCalledOnce)
      {
        isCalledOnce = true;
        if (OptionManager.isSePlaying == true)
        {
          AudioSource.PlayClipAtPoint(GameClearSound, transform.position);
        }
      }
      Time.timeScale = 0f;
      isGameClear = true;
    }

    if (isGameClear == true)
    {
      if (Input.touchCount > 0)
      {
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
          Time.timeScale = 1;
          FadeManager.Instance.LoadScene("EasyModePlay", 0.3f);
        }
      }
    }
  }
}
