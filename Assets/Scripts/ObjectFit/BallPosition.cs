using UnityEngine;

// ボールの最初の位置
public class BallPosition : MonoBehaviour
{
  void Start()
  {
    transform.position = new Vector3(0f, Playfield.BallStartY, 0f);
  }
}
