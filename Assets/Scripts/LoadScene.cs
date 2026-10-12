using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
  public AudioClip StartSound;

  // Update is called once per frame
  void Update()
  {
    // 画面のどこをタップしてもスタート（連打で二重に遷移しないようにする）
    if (Input.GetMouseButtonDown(0) && !FadeManager.Instance.IsFading)
    {
      if (OptionManager.isSePlaying)
      {
        AudioSource.PlayClipAtPoint(StartSound, transform.position);
      }
      FadeManager.Instance.LoadScene("Menu", 0.3f);
    }
  }
}
