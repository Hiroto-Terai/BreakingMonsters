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
  // Start is called before the first frame update
  void Start()
  {
    myTransform = this.transform;
  }

  // Update is called once per frame
  void Update()
  {
    if (myTransform.childCount == 0)
    {
      gameOverText.text = "Game Over...";
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
          SceneManager.LoadScene("Play");
        }
      }
    }
  }
}
