using UnityEngine;
using UnityEngine.UI;

// ポップアップ文字をカクカクと上に浮かせながら消す（ドットゲーム風）
public class FloatingText : MonoBehaviour
{
  const float LifeSeconds = 0.8f;
  // UIのドット1つ分ずつ動かす
  const float StepUnits = 6f;
  const int FadeSteps = 4;

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
    float lift = Mathf.Round(Lift * progress / StepUnits) * StepUnits;
    rectTransform.anchoredPosition = startPosition + new Vector2(0f, lift);
    Color color = text.color;
    color.a = 1f - Mathf.Floor(progress * FadeSteps) / FadeSteps;
    text.color = color;
    if (progress >= 1f)
    {
      Destroy(gameObject);
    }
  }
}
