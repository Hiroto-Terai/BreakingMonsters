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
  public string reloadScene;

  public GameObject loadSceneCanvas;

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
      gameOverText.text = "Game Over...";
      // BGMの停止
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
      GameObject.Find("Ball").GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
      isGameOver = true;
      if (isGameOver == true)
      {
        loadSceneCanvas.SetActive(true);
      }
    }
  }
}
