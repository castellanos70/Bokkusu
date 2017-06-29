using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
	public GameObject arrow;
	public Material arrowMaterial;
    private Material playerMaterial;
    public AudioSource playerAudio;

    public int playerNumber;
    public int speedMax;
    public int speedMin;
    public int acceleration;

    private bool moving = false;
    private float speedX = 0;
    private float speedZ = 0;
    private int startX, startZ;
    private int gridX, gridZ;
    private bool readyToSpawnCrate;
    private bool iHaveWon;


    private GameObject[] arrows; //up down left right
	private float[,] arrowDirs = new float[4,2]
    {
		{-1, 0},
		{0, 1},
		{1, 0},
		{0, -1}
	};

	private int timingOffset = 0;
    private Cell[,] grid;
    
    private CameraScript.Element myPlayerEnum;
    private CameraScript cameraScript;
    private PlayerScript otherPlayerScript;
    private int libertyCount;

    private KeyCode[] keycode = new KeyCode[5];
    private String axisHorizontal, axisVertical;

    void Start()
    {
        Debug.Log("PlayerScript.Start(): creating Arrows.");
        arrows = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            arrows[i] = Instantiate(arrow, arrow.transform.position, Quaternion.identity);
            arrows[i].GetComponent<Renderer>().material = arrowMaterial;
            arrows[i].SetActive(true);
            arrows[i].transform.position = transform.position + new Vector3(arrowDirs[i, 0] * .5f, 0, arrowDirs[i, 1] * .5f);
            arrows[i].transform.Rotate(new Vector3(90, 90 * i, 0));
        }


        arrow.SetActive(false);
        playerMaterial = gameObject.GetComponent<Renderer>().material;

        
        if (playerNumber == 1)
        {
            keycode[0] = KeyCode.W;
            keycode[1] = KeyCode.D;
            keycode[2] = KeyCode.S;
            keycode[3] = KeyCode.A;
            keycode[4] = KeyCode.X;
            axisHorizontal = "ArcadeOneHorizontal";
            axisVertical = "ArcadeOneVertical";
            myPlayerEnum = CameraScript.Element.PLAYER1;
        }
        else
        {
            keycode[0] = KeyCode.UpArrow;
            keycode[1] = KeyCode.RightArrow;
            keycode[2] = KeyCode.DownArrow;
            keycode[3] = KeyCode.LeftArrow;
            keycode[4] = KeyCode.Slash;
            axisHorizontal = "ArcadeTwoHorizontal";
            axisVertical = "ArcadeTwoVertical";
            myPlayerEnum = CameraScript.Element.PLAYER2;
            timingOffset = 50;
        }
        libertyCount = 4;
    }


    public void setBoard(CameraScript cameraScript, Cell[,] myGrid)
    {
        this.cameraScript = cameraScript;
        iHaveWon = false;
        grid = myGrid;
        CameraScript.Element otherPlayerEnum = CameraScript.Element.PLAYER1;
        if (otherPlayerEnum == myPlayerEnum) otherPlayerEnum = CameraScript.Element.PLAYER2;
        otherPlayerScript = cameraScript.getPlayerObject(otherPlayerEnum);

        gameObject.GetComponent<Renderer>().material = playerMaterial;
}

    public void setStartPosition(int x, int z)
    {
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(x, 1, z);
        startX = x;
        startZ = z;
        gridX = x;
        gridZ = z;

        readyToSpawnCrate = false;
        moving = false;
        speedX = 0;
        speedZ = 0;

    }


    // Update is called once per main game loop iteration
    void Update()
    {
        if (cameraScript.getGameState() == CameraScript.GameState.WON && iHaveWon)
        {
            transform.localScale *= 0.995f;
            transform.Rotate(Vector3.up * Time.deltaTime * 90);
            float x = transform.position.x + (gridX - transform.position.x) / 4;
            float z = transform.position.z + (gridZ - transform.position.z) / 4;
            transform.position = new Vector3(x, 1, z);

        }
        else if (cameraScript.getGameState() == CameraScript.GameState.PLAYING)
        {
            updateSpawnCrate();
            updateSpeed();
            updateArrows();
        }
    }

    

   private void updateSpeed()
   {
        if (!moving)
        {
            speedX = 0;
            speedZ = 0;
        }

        int toX  = gridX;
        int toZ = gridZ;

        bool playerIsPressingMove = false;
        int joystickX = readJoystickX();
        int joystickZ = readJoystickZ();
        

        if (joystickZ != 0 && speedX == 0)
        {
            playerIsPressingMove = true;
            speedZ += joystickZ * acceleration * Time.deltaTime;
            toZ = gridZ + joystickZ;
        }
        
        else if (joystickX != 0 && speedZ == 0)
        {
            playerIsPressingMove = true;
            speedX += joystickX * acceleration * Time.deltaTime;
            toX = gridX + joystickX;
        }

        speedX = toSpeedBounds(speedX);
        speedZ = toSpeedBounds(speedZ);
        //Debug.Log("Player["+playerNumber+"] speed=(" + tmpx+", "+tmpz+") === > (" + speedX+", "+speedZ+")");


        if ((!moving) && playerIsPressingMove)
        {
            if (cameraScript.isEnterable(toX, toZ, true))
            {
                moving = true;
                playerAudio.Play();
            }
            else
            {
                speedX = 0;
                speedZ = 0;
            }
        }
        if (moving)
        {
            float speed = speedX + speedZ;
            playerAudio.pitch = 0.5f + floatToUnit(speed) * (speed / (speedMax / 2.5f));
        }
    }

    public bool isMoving()
    {
        //if (speedX == 0f && speedZ == 0f) return true;
        //return false;
        return moving;
    }


    public void hit()
    {
        playerAudio.Stop();
        transform.position = new Vector3(gridX, 1, gridZ);
        moving = false;
        speedX = 0;
        speedZ= 0;
    }


    public int getGridX() { return gridX; }
    public int getGridZ() { return gridZ; }

    public float getSpeedX() {return speedX; }
    public float getSpeedZ() { return speedZ; }

    public void updateLocation(float x, float z)
    {
        transform.position = new Vector3(x, 1, z);
        bool moved = false;
        if (speedX > 0)
        {
            if (x - gridX > 0.5f)
            {
                moved = true; gridX++;
            }
        }
        else if (speedX < 0)
        {
            if (gridX -x > 0.5f)
            {
                moved = true; gridX--;
            }
        }
        else if (speedZ > 0)
        {
            if (z - gridZ > 0.5f)
            {
                moved = true; gridZ++;
            }
        }
        else if (speedZ < 0)
        {
            if (gridZ - z > 0.5f)
            {
                moved = true; gridZ--;
            }
        }

       
        if (grid[gridX, gridZ].getType() == CameraScript.Element.GOAL)
        {
            playerAudio.Stop();
            iHaveWon = true;
            cameraScript.setGameState(CameraScript.GameState.WON);
        }
        else if (grid[gridX, gridZ].getType() == CameraScript.Element.CRATE)
        {
            grid[gridX, gridZ].smashCrate();

        }
        else if (moved)
        {
            readyToSpawnCrate = true;
        }
        //Debug.Log("PlayerScript.updateLocation(" + x + ", " + z + "):  grid[" + gridX + ", " + gridZ + "]");

    }

    private void updateSpawnCrate()
    {
        if (moving) return;
        if (!readyToSpawnCrate) return;
        if (Input.GetKey(keycode[4]))
        {
            cameraScript.spawnCrate(gridX, gridZ);
            gridX = startX;
            gridZ = startZ;
            hit();
            readyToSpawnCrate = false;
            if (grid[gridX, gridZ].getType() == CameraScript.Element.CRATE)
            {
                grid[gridX, gridZ].smashCrate();

            }
        }
  
    }


    private int readJoystickX()
    {
        if (Input.GetKey(keycode[1])) return 1;
        if (Input.GetKey(keycode[3])) return -1;

        float value = Input.GetAxis(axisHorizontal);
        //Debug.Log("PlayerScript.readJoystick[" + playerNumber + "]:  value=" + value);
        if (value >  0.5f) return 1;
        if (value < -0.5f) return -1;
        return 0;
    }


    private int readJoystickZ()
    {
        if (Input.GetKey(keycode[0])) return 1;
        if (Input.GetKey(keycode[2])) return -1;

        float value = Input.GetAxis(axisVertical);
        if (value > 0.5f) return 1;
        if (value < -0.5f) return -1;
        return 0;
    }

    private float toSpeedBounds(float speed)
    {
        if (speed == 0f) return 0f;
        if (speed > speedMax) return speedMax;
        if (speed < -speedMax) return -speedMax;

        if (speed > 0 && speed < speedMin) return speedMin;
        if (speed < 0 && speed > -speedMin) return -speedMin;

        return speed;
    }

    private int intToUnit(int n)
    {
        if (n < 0) return -1;
        else if (n > 0) return 1;
        return 0;
    }

    private int floatToUnit(float n)
    {
        if (n < 0) return -1;
        else if (n > 0) return 1;
        return 0;
    }



    private int[] getDistances()
    {
        int[] dists = new int[4];
        float x = transform.position.x;
        float z = transform.position.z;

        int otherPlayerX = otherPlayerScript.getGridX();
        int otherPlayerZ = otherPlayerScript.getGridZ();

        for (int i = 0; i < 4; i++)
        {
            int dist = 0;
            bool foundblock = false;
            while (!foundblock)
            {
                int dx = (int)(x + arrowDirs[i, 0] * (dist + 1));
                int dz = (int)(z + arrowDirs[i, 1] * (dist + 1));
                if (grid[dx, dz].getType() != CameraScript.Element.FLOOR) foundblock = true;
                else if ((dx == otherPlayerX) && (dz == otherPlayerZ)) foundblock = true;
                else
                {
                    dist++;
                }
            }
            dists[i] = dist;
        }
        return dists;
    }
    


    private void updateArrows()
    {
        if (moving)
        {
            for (int i = 0; i < 4; i++)
            {
              arrows[i].SetActive(false);
            }
            
            return;
        }

        
        int[] dists = getDistances();
        //if (speedX == 0) { arrows[1].SetActive(true); arrows[3].SetActive(true); }
        //else { arrows[1].SetActive(false); arrows[3].SetActive(false); }
        //if (speedZ == 0) { arrows[0].SetActive(true); arrows[2].SetActive(true); }
        //else { arrows[0].SetActive(false); arrows[2].SetActive(false); }

        libertyCount = 0;
        for (int i = 0; i < 4; i++)
        {
            //dists[i] = 1; // This just makes the arrows 1 block long
            if (dists[i] > 0)
            {
                libertyCount++;
                arrows[i].SetActive(true);

                arrows[i].transform.position = transform.position + new Vector3(arrowDirs[i, 0] * .5f, 0, arrowDirs[i, 1] * .5f);
                float width = Mathf.Sin((Time.frameCount + timingOffset) / 15f) * .2f + .8f; //ocelation
                arrows[i].transform.localScale = new Vector3((dists[i]), width, 1); //this works because of relative rotation
                                                                                    //Debug.Log(dists[i]);
            }
            else arrows[i].SetActive(false);
        }
        //Debug.Log("PlayerScript["+ playerNumber +"].updateArrows(): libertyCount="+ libertyCount);
    }
}
