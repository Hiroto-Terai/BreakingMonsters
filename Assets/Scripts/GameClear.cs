using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClear : MonoBehaviour
{
  public Text gameClearText;
  Transform myTransform;
  // BGMを持っているオブジェクト
  public GameObject Music;
  // ゲームクリアサウンド
  public AudioClip GameClearSound;
  public GameObject loadSceneCanvas;

  // Start is called before the first frame update
  void Start()
  {
    myTransform = transform;
  }

  // Update is called once per frame
  void Update()
  {
    // 敵スライムが全部いなくなったらクリア（ゲームオーバーと同時なら先に判定した方だけ）
    if (myTransform.childCount == 0 && GameSession.Instance.TryEnd(true))
    {
      gameClearText.text = "Game Clear!!";
      // BGMの停止
      Music.SetActive(false);
      // ゲームクリアのSEを鳴らす
      if (OptionManager.isSePlaying == true)
      {
        AudioSource.PlayClipAtPoint(GameClearSound, transform.position);
      }
      GameObject.Find("Ball").GetComponent<Rigidbody2D>().linearVelocity = new Vector3(0, 0, 0);
      loadSceneCanvas.SetActive(true);
    }
  }
}
