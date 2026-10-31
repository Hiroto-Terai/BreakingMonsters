using UnityEngine;

// 自分のバーの位置
public class PlayerPosition : MonoBehaviour
{
  void Start()
  {
    transform.position = new Vector3(0f, Playfield.PlayerY, 0f);
  }
}
