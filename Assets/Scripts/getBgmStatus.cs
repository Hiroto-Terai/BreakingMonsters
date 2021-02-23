using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class getBgmStatus : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {
    if (SceneManager.GetActiveScene().name == "Menu")
    {
      getBgmStatusInMenuScene();
    }
    else if (SceneManager.GetActiveScene().name == "EasyPlayMenu")
    {
      getBgmStatusInPlayScene();
    }
  }

  // Update is called once per frame
  void Update()
  {

  }

  public bool getBgmStatusInMenuScene()
  {
    return OptionManager.isBgmPlaying;
  }

  public bool getBgmStatusInPlayScene()
  {
    return OptionManager.isBgmPlaying;
  }
}
