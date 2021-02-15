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

  // Start is called before the first frame update
  void Start()
  {
    myTransform = transform;
  }

  // Update is called once per frame
  void Update()
  {
    if (myTransform.childCount == 0)
    {
      gameClearText.text = "Game Clear!!";
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
          SceneManager.LoadScene("Play");
        }
      }
    }
  }
}
