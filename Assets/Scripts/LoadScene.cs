using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
  GameObject mainCamera;
  GameObject Button;
  Camera main;
  public AudioClip StartSound;

  // Start is called before the first frame update
  void Start()
  {
    mainCamera = GameObject.Find("Main Camera");
    Button = GameObject.Find("Start");
  }

  // Update is called once per frame
  void Update()
  {
    main = mainCamera.GetComponent<Camera>();
    Vector3 touchPos = main.ScreenToViewportPoint(Input.mousePosition);
    Collider2D col = Physics2D.OverlapPoint(touchPos);
    // タップ確認
    if (Input.GetMouseButtonDown(0))
    {
      if (col = Button.GetComponent<Collider2D>())
      {
        AudioSource.PlayClipAtPoint(StartSound, transform.position);
        FadeManager.Instance.LoadScene("Menu", 0.3f);
      }
    }
  }
}
