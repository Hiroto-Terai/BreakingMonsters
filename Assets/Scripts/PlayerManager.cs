using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // プレイヤーの移動の速さ
    public float speed = 10f;
    Rigidbody2D myRigidBody2d;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if(transform.position.x < 1.8){
                transform.position += Vector3.rigit * Time.deltaTime * speed;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if(transform.position.x > -1.8){
                transform.position += Vector3.left * Time.deltaTime * speed;
            }
        }

        // プレイヤーの移動(スマホでのタッチ操作)
        if(Input.touchCount == 1)
        {
            // タッチ情報の取得
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                transform.position += new Vector3(touch.position.x, 0, 0);
            }     
        }
    }
}
