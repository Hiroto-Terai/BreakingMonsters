using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
  public Text gameOverText;
  // スライムを持つ親オブジェクトを取得
  Transform myTransform;
  // ゲームオーバーの判定
  bool isGameOver = false;
  // BGMを持っているオブジェクト
  public GameObject Music;
  // ゲームオーバーサウンド
  // update関数内で一回だけ呼び出すためのフラグを用意
  public AudioClip GameOverSound;
  bool isCalledOnce;

  // Start is called before the first frame update
  void Start()
  {
    myTransform = this.transform;
    // update関数内で一回だけ呼び出すためのフラグを初期化
    isCalledOnce = false;
  }

  // Update is called once per frame
  void Update()
  {
    if (myTransform.childCount == 0)
    {
      gameOverText.text = "Game Over...";
      Music.SetActive(false);
      // ゲームクリアのSEを鳴らす
      if (!isCalledOnce)
      {
        isCalledOnce = true;
        if (OptionManager.isSePlaying == true)
        {
          AudioSource.PlayClipAtPoint(GameOverSound, transform.position);
        }
      }
      Time.timeScale = 0f;
      isGameOver = true;
    }

    if (isGameOver == true)
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
