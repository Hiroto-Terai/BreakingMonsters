using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敵スライムと味方スライムの両方に付いていて、ボールが当たると消える
public class EnemyManager : MonoBehaviour
{
  public AudioClip BallColSound;
  bool isHit;

  private void OnCollisionEnter2D(Collision2D collision)
  {
    // 同じフレームで2回当たっても1回分だけ数える
    if (isHit)
    {
      return;
    }
    isHit = true;
    if (OptionManager.isSePlaying == true)
    {
      AudioSource.PlayClipAtPoint(BallColSound, transform.position);
    }
    NotifySession();
    Destroy(this.gameObject);
  }

  void NotifySession()
  {
    if (GameSession.Instance == null)
    {
      return;
    }
    if (transform.parent.GetComponent<GameClear>() != null)
    {
      GameSession.Instance.OnMonsterDefeated(transform.position);
    }
    else if (transform.parent.GetComponent<GameOver>() != null)
    {
      GameSession.Instance.OnSlimeLost(transform.position);
    }
  }
}
