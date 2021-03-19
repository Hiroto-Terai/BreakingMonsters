using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionButtonPosition : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {
    if (Screen.currentResolution.height <= 1334)
    {
      GetComponent<RectTransform>().anchoredPosition = new Vector2(50, -290);
    }
    else
    {
      GetComponent<RectTransform>().anchoredPosition = new Vector2(50, -400);
    }
  }
}
