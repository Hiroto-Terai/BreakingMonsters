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
        //myRigidBody2d.velocity = new Vector3(Input.GetAxis("Horizontal") * speed, 0f, 0f);
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.position += new Vector3(2f * Time.deltaTime * speed, 0f, 0f);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.position += new Vector3(-2f * Time.deltaTime * speed, 0f, 0f);
        }

        // プレイヤーの移動(スマホでのタッチ操作)
        if(Input.touchCount == 1)
        {
            // タッチ情報の取得
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                transform.Translate(touch.deltaPosition.x, touch.deltaPosition.y, 0.0f);
                //transform.position += new Vector3(touch.position.x, 0, 0);
            }     
        }
    }
        
}
