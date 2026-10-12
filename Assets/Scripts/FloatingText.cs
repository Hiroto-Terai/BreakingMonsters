using UnityEngine;
using UnityEngine.UI;

// スコアのポップアップを上に浮かせながら消す
public class FloatingText : MonoBehaviour
{
  const float LifeSeconds = 0.8f;

  public float Lift;
  Text text;
  RectTransform rectTransform;
  Vector2 startPosition;
  float time;

  void Start()
  {
    text = GetComponent<Text>();
    rectTransform = GetComponent<RectTransform>();
    startPosition = rectTransform.anchoredPosition;
  }

  void Update()
  {
    time += Time.deltaTime;
    float progress = time / LifeSeconds;
    rectTransform.anchoredPosition = startPosition + new Vector2(0f, Lift * progress);
    Color color = text.color;
    color.a = 1f - progress;
    text.color = color;
    if (progress >= 1f)
    {
      Destroy(gameObject);
    }
  }
}
