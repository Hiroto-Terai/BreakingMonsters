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
  // BGMを持っているオブジェクト
  public GameObject Music;
  // ゲームオーバーサウンド
  public AudioClip GameOverSound;
  public string reloadScene;

  public GameObject loadSceneCanvas;

  // Start is called before the first frame update
  void Start()
  {
    myTransform = transform;
  }

  // Update is called once per frame
  void Update()
  {
    // 味方スライムが全部いなくなったらゲームオーバー（クリアと同時なら先に判定した方だけ）
    if (myTransform.childCount == 0 && GameSession.Instance.TryEnd(false))
    {
      gameOverText.text = "Game Over...";
      // BGMの停止
      Music.SetActive(false);
      // ゲームオーバーのSEを鳴らす
      if (OptionManager.isSePlaying == true)
      {
        AudioSource.PlayClipAtPoint(GameOverSound, transform.position);
      }
      GameObject.Find("Ball").GetComponent<Rigidbody2D>().linearVelocity = new Vector3(0, 0, 0);
      loadSceneCanvas.SetActive(true);
    }
  }
}
