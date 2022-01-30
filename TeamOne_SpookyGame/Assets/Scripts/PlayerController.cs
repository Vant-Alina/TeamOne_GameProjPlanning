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

    //Keeps track of what box is grabbed, where it should go, and when it should be let go
    GameObject grabbedBox;
    Vector3 grabbedBoxOffset;
    [SerializeField] float maxGrabDistance;

    //Player rigidbody
    Rigidbody rb;

    //The direction the player is moving
    Vector2 moveDir;

    void Start()
    {
        //Create a reference to the player rigidbody
        rb = GetComponent<Rigidbody>();
        grabbedBox = null;
    }

    // Update is called once per frame
    void Update()
    {
        //Gets the direction the player should move
        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveDir.Normalize();

        //Checks to see if the player is holding the grab button or not
        if (Input.GetButtonDown("Grab"))
        {
            AttemptGrab();
        } else if (Input.GetButtonUp("Grab"))
        {
            LetGo();
        }

    }

    void FixedUpdate()
    {

        //If the player isn't moving, add artificial friction
        if (moveDir.magnitude == 0)
        {
            rb.velocity = new Vector3(rb.velocity.x * frictionAmnt, rb.velocity.y, rb.velocity.z * frictionAmnt);

        //If the player is moving, add velocity in the direction held
        } else
        {
            Vector3 newVelocity = new Vector3(moveDir.x * speed, 0, moveDir.y * speed);
            rb.velocity = newVelocity;
        }

        //If a box is currently being held, update the box's position
        if (grabbedBox != null)
        {
            //Calculates the position the box should move towards
            Vector3 grabbedBoxPos = grabbedBox.GetComponent<Rigidbody>().position;
            Vector3 desiredPos = transform.position + grabbedBoxOffset;
            
            //Sets the desired y position to the box y position so that it doesn't move vertically
            desiredPos.y = grabbedBoxPos.y;

            //Calculate the direction the box should move
            Vector3 moveDir = (desiredPos - grabbedBoxPos).normalized;

            //Add the player's movement speed to the box if it hasn't reached the desired position
            if (Vector3.Distance(desiredPos, grabbedBox.transform.position) > 0.1)
            {
                grabbedBox.GetComponent<Rigidbody>().velocity = moveDir * speed * Vector3.Distance(desiredPos, grabbedBox.transform.position) * 3;
            }
            
            //If the box is too far away, let go of it
            if (Vector3.Distance(transform.position, grabbedBox.transform.position) > maxGrabDistance)
            {
                LetGo();
            }
        }
    }

    //Attempt to grab a box, if any are within reach
    void AttemptGrab()
    {
       //Keeps track of the closest box to the player, if any
        GameObject closestBox = null;

        //Gets all colliders within the player's reach
        Collider[] collisionsHit = Physics.OverlapSphere(transform.position, transform.gameObject.GetComponent<CapsuleCollider>().radius * 1.1f);

        //Checks each collider
        foreach (Collider collider in collisionsHit)
        {
            float closestBoxDistance = 999999f;
            GameObject gameObjectHit = collider.transform.gameObject;

            //If the collider is a box and is closer than the other boxes checked
            if (gameObjectHit.tag == "Box" && Vector3.Distance(transform.position, gameObjectHit.transform.position) < closestBoxDistance)
            {
                //Set the closest box to this box, and update the closest box distance
                closestBox = gameObjectHit;
                closestBoxDistance = Vector3.Distance(transform.position, gameObjectHit.transform.position);
            }
        }

        //If a box was found, update the grabbed box and it's offset accordingly
        if (closestBox != null)
        {
            grabbedBox = closestBox;
            grabbedBoxOffset = grabbedBox.transform.position - transform.position;
        }
    }

    //Lets go of the grabbed box
    void LetGo()
    {
        if (grabbedBox != null)
        {
            grabbedBox = null;
        }
    }


}
