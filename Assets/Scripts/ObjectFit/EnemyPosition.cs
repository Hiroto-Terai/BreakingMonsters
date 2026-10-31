using UnityEngine;

// 敵のバーの位置
public class EnemyPosition : MonoBehaviour
{
  void Start()
  {
    transform.position = new Vector3(0f, Playfield.EnemyBarY, 0f);
  }
}
