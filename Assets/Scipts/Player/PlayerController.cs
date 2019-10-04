using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;

    //Movement Vars
    public float speed = 1;
    void Start(){
       rb = GetComponent<Rigidbody>();
    }

    void Update(){
        handleMovement();
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        // Debug.Log("Horizontal: " + horiz);
        // Debug.Log("Vertical: " + vert);
    }

    private void handleMovement(){
         if (Input.GetKey(KeyCode.A))
            moveOneStep(-Vector3.right);
         if (Input.GetKey(KeyCode.D))
            moveOneStep(Vector3.right);
         if (Input.GetKey(KeyCode.W))
            moveOneStep(Vector3.forward);
         if (Input.GetKey(KeyCode.S))
            moveOneStep(-Vector3.forward);
    }

    public void moveOnAxis(int axis){ //Uses outputs from neural network
        if (axis == 0)
            moveOneStep(-Vector3.right);
        if (axis == 1)
            moveOneStep(Vector3.right);
        if (axis == 2)
            moveOneStep(Vector3.forward);
        if (axis == 3)
            moveOneStep(-Vector3.forward);
    }

    public void move(float horiz, float vert){
        Vector3 dir = new Vector3(horiz, 0, vert).normalized;
        moveOneStep(dir);
    }

    private void moveOneStep(Vector3 dir){
        if (rb == null){rb = GetComponent<Rigidbody>();}
        rb.MovePosition(transform.position + (dir * Time.deltaTime * speed));
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
