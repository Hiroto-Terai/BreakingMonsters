using UnityEngine;

// 味方スライムを並べる高さ
public class PlayerSlimePosition : MonoBehaviour
{
  void Start()
  {
    transform.position = new Vector3(0f, Playfield.PlayerLivesY, 0f);
  }
}
