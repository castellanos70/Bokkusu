using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GameObject boardBlock;
    public Material[] wallMat = new Material[10];


    private bool moving = false;
    private float speedX = 0;
    private float speedZ = 0;
    private const int boardSize = 25;

    private enum Element { FLOOR, WALL, PLAYER1, PLAYER2, PORTAL1, PORTAL2, PORTAL3 };
    private Element[,] grid = new Element[boardSize, boardSize];


    
    void Start ()
    {
	   //TODO: (Ben) populate grid from ASCII file	
       for (int x=0; x< boardSize; x++)
       {
            for (int z = 0; z < boardSize; z++)
            {
                grid[x, z] = Element.FLOOR;
                if (x == 0 || z == 0) grid[x, z] = Element.WALL;
                else if (x == boardSize-1 || z == boardSize-1) grid[x, z] = Element.WALL;
                else if (x==12 && z==12) grid[x, z] = Element.PLAYER1;
                else if (Random.value < 0.04f) grid[x, z] = Element.WALL;
            }
        }



        // Spawn board blocks
        for (int x = 0; x < boardSize; x++)
        {
            for (int z = 0; z < boardSize; z++)
            {

                GameObject block = Instantiate(boardBlock, new Vector3(x, 0, z), Quaternion.identity);
                if (grid[x, z] == Element.WALL)
                {

                    Renderer renderer = block.GetComponent<Renderer>();
                    renderer.material = wallMat[Random.Range(0, wallMat.Length)];
                    block.transform.Translate(Vector3.up);
                }
                else if (grid[x, z] == Element.PLAYER1)
                {

                    transform.Translate(new Vector3(x,1,z));
                    grid[x, z] = Element.FLOOR;
                }

            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (moving)
        {
            int x0 = (int)transform.position.x;
            int z0 = (int)transform.position.z;
            float x = transform.position.x + speedX * Time.deltaTime;
            float z = transform.position.z + speedZ * Time.deltaTime;

            int x1 = (int)(x - 0.5f);
            int z1 = (int)(z - 0.5f);
            int x2 = (int)(x + 0.5f);
            int z2 = (int)(z + 0.5f);

            bool hit = false;
            if (speedX > 0)
            {
                if (grid[x2, z0] != Element.FLOOR) hit = true;
            }
            else if (speedX < 0)
            {
                if (grid[x1, z0] != Element.FLOOR) hit = true;
            }
            else if (speedZ > 0)
            {
                if (grid[x0, z2] != Element.FLOOR) hit = true;
            }
            else if (speedZ < 0)
            {
                if (grid[x0, z1] != Element.FLOOR) hit = true;
            }

            if (hit)
            {
                x = x0; z = z0;
                moving = false;
            }


            transform.position = new Vector3(x, 1, z);


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
