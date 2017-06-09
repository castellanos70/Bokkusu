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

    private CameraScript.Element[,] grid;
    private CameraScript.Element myPlayer;
    private CameraScript cameraScript;

    private KeyCode[] keycode = new KeyCode[4];


    public void setMaxLevelMoves(int moveCount)
    {
        movesRemaining = moveCount;
    }

    public void setBoard(CameraScript cameraScript, CameraScript.Element[,] myGrid)
    {
        this.cameraScript = cameraScript;
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
			myPlayer = CameraScript.Element.PLAYER1;
        }
        else
        {
            keycode[0] = KeyCode.UpArrow;
            keycode[1] = KeyCode.RightArrow;
            keycode[2] = KeyCode.DownArrow;
            keycode[3] = KeyCode.LeftArrow;
			myPlayer = CameraScript.Element.PLAYER2;
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
				grid[x0, z0] = CameraScript.Element.FLOOR;

                if (grid[(int)x, (int)z] == CameraScript.Element.GOAL)
                {
                    cameraScript.setGameState(CameraScript.GameState.WON);
                }

                grid[(int)x, (int)z] = myPlayer;
            }
            transform.position = new Vector3(x, 1, z);
        }
        else
        {
            if (movesRemaining > 0)
            {
                int x1 = x0;
                int z1 = z0;
                if (Input.GetKey(keycode[0]))
                {
                    speedZ = 10;
                    speedX = 0;
                    z1 += 1;
                    moving = true;
                }
                else if (Input.GetKey(keycode[2]))
                {
                    speedZ = -10;
                    speedX = 0;
                    z1 -= 1;
                    moving = true;
                }
                else if (Input.GetKey(keycode[1]))
                {
                    speedZ = 0;
                    speedX = 10;
                    x1 += 1;
                    moving = true;
                }
                else if (Input.GetKey(keycode[3]))
                {
                    speedZ = 0;
                    speedX = -10;
                    x1 -= 1;
                    moving = true;
                }


                if (moving)
                {
                    
                    bool hit = checkMove(x1, z1);
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

        if (speedX > 0) x1 = (int)(x + 0.51f + speedX * Time.deltaTime);
        else if (speedX < 0) x1 = (int)(x - 0.51f);
        else if (speedZ > 0) z1 = (int)(z + 0.51f + speedZ * Time.deltaTime);
        else if (speedZ < 0) z1 = (int)(z - 0.51f);

        //Debug.Log(Time.time + ": [" + x0 + "," + z0 + "]===> "+ " grid[" +x1+","+z1+"]="+grid[x1,z1]);

		if (grid[x1, z1] == CameraScript.Element.FLOOR) return false;
		if (grid[x1, z1] == CameraScript.Element.GOAL) return false;
        if (grid[x1, z1] == myPlayer) return false;
        return true;
    }



    private bool checkMove(int x1, int z1)
    {
        if (grid[x1, z1] == CameraScript.Element.FLOOR) return false;
        if (grid[x1, z1] == CameraScript.Element.GOAL) return false;
        if (grid[x1, z1] == myPlayer) return false;
        return true;
    }
}
