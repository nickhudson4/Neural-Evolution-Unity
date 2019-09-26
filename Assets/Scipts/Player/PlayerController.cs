using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;

    //Movement Vars
    public float speed = 1;
    void Start(){
       rb = GetComponent<Rigidbody>(); 
    }

    void Update(){
        handleMovement();
    }

    private void handleMovement(){
         if (Input.GetKey(KeyCode.A))
            moveOneStep(-Vector3.right);
         if (Input.GetKey(KeyCode.D))
            moveOneStep(Vector3.right);
         if (Input.GetKey(KeyCode.W))
            moveOneStep(Vector3.up);
         if (Input.GetKey(KeyCode.S))
            moveOneStep(-Vector3.up);
    }

    private void moveOneStep(Vector3 dir){
        rb.MovePosition(transform.position + (dir * Time.deltaTime));
    } 

    public Vector2 vector2_to_vector3(Vector3 orig, string dropAxis){
        if (dropAxis == "y"){
            return new Vector2(orig.x, orig.z);
        }
        if (dropAxis== "x"){
            return new Vector2(orig.y, orig.z);
        }
        else {
            return new Vector2(orig.x, orig.y);
        }
    }
}
