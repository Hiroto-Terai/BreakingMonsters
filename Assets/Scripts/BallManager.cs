using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    // ボールの移動の速さを指定する変数
    public float speed = 5f;
    // 速さの最小値を指定する変数を追加
    // 速さの最大値を指定する変数を追加

    Rigidbody2D myRigidbody2d;
    Transform myTransform;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2d = GetComponent<Rigidbody2D>();
        myRigidbody2d.velocity = new Vector3(speed, speed, 0);
        // 現在地を取得
        myTransform = transform;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
