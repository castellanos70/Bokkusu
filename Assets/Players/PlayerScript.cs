using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    private bool moving = false;
    private float speedX = 0;
    private float speedZ = 0;
	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (moving)
        {
            transform.Translate(new Vector3(speedX*Time.deltaTime, 0, speedZ*Time.deltaTime));
        }
        else
        {
            if (Input.GetKey("up"))
            {
                speedZ = 10;
                speedX = 0;
                moving = true;
            }
            else if (Input.GetKey("down"))
            {
                speedZ = -10;
                speedX = 0;
                moving = true;
            }
            else if (Input.GetKey("right"))
            {
                speedZ = 0;
                speedX = 10;
                moving = true;
            }
            else if (Input.GetKey("left"))
            {
                speedZ = 0;
                speedX = -10;
                moving = true;
            }
        }

    }
}
