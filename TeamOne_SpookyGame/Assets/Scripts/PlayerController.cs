using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update

    //Controls the player's max speed
    [SerializeField] float speed;

    //Controls how much friction there is when the player is slowing down
    [SerializeField] float frictionAmnt;

    //Player rigidbody
    Rigidbody rb;

    Vector2 moveDir;

    void Start()
    {
        //Create a reference to the player rigidbody
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveDir.Normalize();

    }

    void FixedUpdate()
    {

        if (moveDir.magnitude == 0)
        {
            rb.velocity = new Vector3(rb.velocity.x * frictionAmnt, rb.velocity.y, rb.velocity.z * frictionAmnt);
        } else
        {
            Vector3 newVelocity = new Vector3(moveDir.x * speed, 0, moveDir.y * speed);
            rb.velocity = newVelocity;
        }

        
        
        /*if (rb.velocity.magnitude >= maxSpeed)
        {
            rb.velocity.Normalize();
            rb.velocity *= maxSpeed;
            print("a");
        }*/


        //If the player isn't moving, or is trying to move opposite to their current velocity
        /* if (moveDir == 0 || Mathf.Sign(moveDir) != Mathf.Sign(rb.velocity.x))
         {

             //Add friction to slow down the player
             rb.velocity = new Vector3(rb.velocity.x * frictionAmnt, rb.velocity.y, rb.velocity.z);
         }*/

    }


    //Checks to see if the player collided with something
    private void OnCollisionEnter(Collision collision)
    {
        
    }
}
