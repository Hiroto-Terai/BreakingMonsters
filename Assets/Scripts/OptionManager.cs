using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
  // オプションキャンバスのゲームオブジェクト
  public GameObject OptionCanvas;

  // Start is called before the first frame update
  void Start()
  {
    OptionCanvas.SetActive(false);
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void openOption()
  {
    OptionCanvas.SetActive(true);
  }

  public void closeOption()
  {
    OptionCanvas.SetActive(false);
  }
}
