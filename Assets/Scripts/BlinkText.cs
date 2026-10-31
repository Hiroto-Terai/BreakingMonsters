using UnityEngine;
using UnityEngine.UI;

// 「Tap to Start」などの文字をゆっくり点滅させる
public class BlinkText : MonoBehaviour
{
  const float BlinkSpeed = 3f;
  const float MinAlpha = 0.25f;

  Text text;

  void Start()
  {
    text = GetComponent<Text>();
  }

  void Update()
  {
    Color color = text.color;
    color.a = Mathf.Lerp(MinAlpha, 1f, (Mathf.Sin(Time.unscaledTime * BlinkSpeed) + 1f) / 2f);
    text.color = color;
  }
}
