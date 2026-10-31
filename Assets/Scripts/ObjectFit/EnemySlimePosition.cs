using UnityEngine;

// モンスター（対戦では相手の仲間スライム）を並べる基準の高さ
public class EnemySlimePosition : MonoBehaviour
{
  void Start()
  {
    transform.position = new Vector3(0f, Playfield.MonstersRootY, 0f);
  }
}
