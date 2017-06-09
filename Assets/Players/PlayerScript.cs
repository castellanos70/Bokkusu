using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public int playerNumber;
    public TextMesh levelMovesTextMesh;

    private int movesRemaining;
    private bool moving = false;
    private float speedX = 0;
    private float speedZ = 0;
    private int boardWidth, boardHeight;

    private int[,] grid;
    private int myPlayer;

    private KeyCode[] keycode = new KeyCode[4];


    public void setMaxLevelMoves(int moveCount)
    {
        movesRemaining = moveCount;
    }

    public void setBoard(int[,] myGrid)
    {
        grid = myGrid;
        boardWidth = grid.GetLength(0);
        boardHeight = grid.GetLength(1);
    }



    void Start ()
    {
        levelMovesTextMesh.text = movesRemaining.ToString();

        if (playerNumber == 1)
        {
            keycode[0] = KeyCode.W;
            keycode[1] = KeyCode.D;
            keycode[2] = KeyCode.S;
            keycode[3] = KeyCode.A;
			myPlayer = (int)CameraScript.Element.PLAYER1;
        }
        else
        {
            keycode[0] = KeyCode.UpArrow;
            keycode[1] = KeyCode.RightArrow;
            keycode[2] = KeyCode.DownArrow;
            keycode[3] = KeyCode.LeftArrow;
			myPlayer = (int)CameraScript.Element.PLAYER2;
        }
    }
		
	
	// Update is called once per frame
	void Update ()
    {
        int x0 = (int)transform.position.x;
        int z0 = (int)transform.position.z;

        if (moving)
        {
            float x = transform.position.x + speedX * Time.deltaTime;
            float z = transform.position.z + speedZ * Time.deltaTime;
            bool hit = checkMove(x0, z0, x, z);

            if (hit)
            {
                x = x0; z = z0;
                moving = false;
            }
            else
            {
				grid[x0, z0] = (int)CameraScript.Element.FLOOR;
                grid[(int)x, (int)z] = myPlayer;
            }
            transform.position = new Vector3(x, 1, z);

			/*
            x0 = (int)transform.position.x;
            z0 = (int)transform.position.z;
			grid[x0, z0] = (int)CameraScript.Element.PLAYER2;
			*/
        }
        else
        {
            if (movesRemaining > 0)
            {
                if (Input.GetKey(keycode[0]))
                {
                    speedZ = 10;
                    speedX = 0;
                    moving = true;
                }
                else if (Input.GetKey(keycode[2]))
                {
                    speedZ = -10;
                    speedX = 0;
                    moving = true;
                }
                else if (Input.GetKey(keycode[1]))
                {
                    speedZ = 0;
                    speedX = 10;
                    moving = true;
                }
                else if (Input.GetKey(keycode[3]))
                {
                    speedZ = 0;
                    speedX = -10;
                    moving = true;
                }


                if (moving)
                {
                    float x = transform.position.x + speedX * Time.deltaTime;
                    float z = transform.position.z + speedZ * Time.deltaTime;
                    bool hit = checkMove(x0, z0, x, z);
                    //Debug.Log(Time.time + ": hit=" + hit);

                    if (hit)
                    {
                        speedX = 0;
                        speedZ = 0;
                        moving = false;
                    }
                    else
                    {
                        movesRemaining--;
                        levelMovesTextMesh.text = movesRemaining.ToString();
                    }
                }
            }
        }
    }

    private bool checkMove(int x0, int z0, float x, float z)
    {
        int x1 = x0;
        int z1 = z0;

        if (speedX > 0) x1 = (int)(x + 0.55f);
        else if (speedX < 0) x1 = (int)(x - 0.55f);
        else if (speedZ > 0) z1 = (int)(z + 0.55f);
        else if (speedZ < 0) z1 = (int)(z - 0.55f);

        //Debug.Log(Time.time + ": [" + x0 + "," + z0 + "]===> "+ " grid[" +x1+","+z1+"]="+grid[x1,z1]);

		if (grid[x1, z1] == (int)CameraScript.Element.FLOOR) return false;
		if (grid[x1, z1] == (int)CameraScript.Element.GOAL) return false;
        if (grid[x1, z1] == myPlayer) return false;
        return true;
    }
}
