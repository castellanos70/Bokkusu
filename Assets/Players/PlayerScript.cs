﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
	public GameObject arrow;
	public Material arrowMaterial;
    public AudioSource playerAudio;
    public GameObject editBoxPlayerName;

    public int playerNumber;
    private static float speedMax = 25;
    private static float speedMin = 1;
    private static float acceleration = 30;
    public GameObject spawnSpotObj;

    private bool moving = false;
    private float speedX = 0;
    private float speedZ = 0;
    private int startX, startZ;
    private int gridX, gridZ;
    private bool readyToSpawnCrate;
    private bool iHaveWon;

    private int moveCount, goalCount;


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
    private int libertyCount;

    private KeyCode[] keycode = new KeyCode[5];
    private String axisHorizontal, axisVertical;

    private Material playerMat;
    private Voronoi voronoiScript;
    private float nextVoronoiUpdateTime;

    void Start()
    {
        //Debug.Log("PlayerScript.Start(): creating Arrows.");

        arrows = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            arrows[i] = Instantiate(arrow, arrow.transform.position, Quaternion.identity);
            arrows[i].GetComponent<Renderer>().material = arrowMaterial;
            arrows[i].SetActive(false);
            arrows[i].transform.position = transform.position + new Vector3(arrowDirs[i, 0] * .5f, 0, arrowDirs[i, 1] * .5f);
            arrows[i].transform.Rotate(new Vector3(90, 90 * i, 0));
        }


        arrow.SetActive(false);
        
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


        playerMat = gameObject.GetComponent<Renderer>().material;
        if (playerNumber == 1) voronoiScript = new Voronoi(playerMat, 256, 240, 0, 21);
        else voronoiScript = new Voronoi(playerMat, 256, 218, 216, 29);

        startGame();
    }


    public void setBoard(CameraScript cameraScript, Cell[,] myGrid)
    {
        nextVoronoiUpdateTime = 0;
        this.cameraScript = cameraScript;
        iHaveWon = false;
        grid = myGrid;
        CameraScript.Element otherPlayerEnum = CameraScript.Element.PLAYER1;
        if (otherPlayerEnum == myPlayerEnum) otherPlayerEnum = CameraScript.Element.PLAYER2;
        //otherPlayerScript = cameraScript.getPlayerObject(otherPlayerEnum);
        spawnSpotObj.SetActive(false);
        gameObject.SetActive(false);
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

        //spawnSpotObj.transform.position =  new Vector3(transform.position.x, 0.51f, transform.position.z);
        spawnSpotObj.transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
    }


    public void startGame()
    {
        goalCount = 0;
    }

    public void startLevel()
    {
        spawnSpotObj.SetActive(true);
        gameObject.SetActive(true);
        //editBoxPlayerName.SetActive(false);
        moveCount = 0;
    }


    public void showEditBoxForPlayerName()
    {
        //editBoxPlayerName.SetActive(true);
    }


    // Update is called once per main game loop iteration
    void Update()
    {
        if (Time.time > nextVoronoiUpdateTime)
        {
            voronoiScript.next();
            nextVoronoiUpdateTime = Time.time + 0.1f;
        }
        if (cameraScript.getGameState() == CameraScript.GameState.WON && iHaveWon)
        {
            transform.localScale *= 0.985f;
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
        updateArrows();
    }

    

   private void updateSpeed()
   {
        if (!moving)
        {
            speedX = 0;
            speedZ = 0;
        }

        int toX = gridX;
        int toZ = gridZ;

        bool playerIsPressingMove = false;
        int joystickX = readJoystickX();
        int joystickZ = readJoystickZ();
        

        if (joystickX != 0 && speedZ == 0)
        {
            playerIsPressingMove = true;
            float speed = speedX + joystickX*acceleration * Time.deltaTime;
            if (speedX > 0)
            {
                if (speed > speedMax) speed = speedMax;
                else if (speed < speedMin) speed = speedMin;
            }
            else if (speedX < 0)
            {
                if (speed < -speedMax) speed = -speedMax;
                else if (speed > -speedMin) speed = -speedMin;
            }
            speedX = speed;
            toX = gridX + joystickX;
        }

        else if (joystickZ != 0 && speedX == 0)
        {
            playerIsPressingMove = true;
            float speed = speedZ + joystickZ * acceleration * Time.deltaTime;
            if (speedZ > 0)
            {
                if (speed > speedMax) speed = speedMax;
                else if (speed < speedMin) speed = speedMin;
            }
            else if (speedZ < 0)
            {
                if (speed < -speedMax) speed = -speedMax;
                else if (speed > -speedMin) speed = -speedMin;
            }
            speedZ = speed;
            toZ = gridZ + joystickZ;
        }



        if ((!moving) && playerIsPressingMove)
        {
            if (cameraScript.enterIfPossible(myPlayerEnum, toX, toZ, true, true))
            {
                moving = true;
                moveCount++;
            }
            else
            {
                speedX = 0;
                speedZ = 0;
            }
        }
    }

    public bool isMoving()
    {
        return moving;
    }


    public void hit(bool hitOtherPlayer)
    {
        snapToGrid();
    }

    private void snapToGrid()
    {
        transform.position = new Vector3(gridX, 1, gridZ);
        moving = false;
        speedX = 0;
        speedZ = 0;
    }


    public int getGridX() { return gridX; }
    public int getGridZ() { return gridZ; }

    public float getSpeedX() {return speedX; }
    public float getSpeedZ() { return speedZ; }

    public float getSpeedMagnitude() { return Mathf.Max(Mathf.Abs(speedX), Mathf.Abs(speedZ)); }

    public bool updateLocation(float x, float z)
    {
        transform.position = new Vector3(x, 1, z);
        bool moved = false;
        int x0 = gridX;
        int z0 = gridZ;
      
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

        CameraScript.Element type = grid[gridX, gridZ].getType();
        if (type == CameraScript.Element.GOAL)
        {
            //playerAudio.Stop();
            iHaveWon = true;
            goalCount++;
            cameraScript.setGameState(CameraScript.GameState.WON);
        }
        else if (type == CameraScript.Element.CRATE)
        {
            grid[gridX, gridZ].smashCrate();

        }
        else if (moved)
        {
            readyToSpawnCrate = true;

            type = grid[x0, z0].getType();
            if (type == CameraScript.Element.DOOR_A || type == CameraScript.Element.DOOR_B)
            {
                cameraScript.toggleDoors();
            }
        }
        return moved;
        //Debug.Log("PlayerScript.updateLocation(" + x + ", " + z + "):  grid[" + gridX + ", " + gridZ + "]");

    }


    private bool isButtonPressed()
    {
        if (Input.GetKey(keycode[4])) return true;
        if (myPlayerEnum == CameraScript.Element.PLAYER1)
        {

            //if (Input.GetButtonDown("ArcadeOne0")) return true;

            if (Input.GetKeyDown("joystick 1 button 0")) return true;
            if (Input.GetKeyDown("joystick 1 button 1")) return true;
            if (Input.GetKeyDown("joystick 1 button 2")) return true;
            if (Input.GetKeyDown("joystick 1 button 3")) return true;
            if (Input.GetKeyDown("joystick 1 button 4")) return true;
            if (Input.GetKeyDown("joystick 1 button 5")) return true;
            if (Input.GetKeyDown("joystick 1 button 6")) return true;
            if (Input.GetKeyDown("joystick 1 button 7")) return true;
        }
        else
        {
            //    //if (Input.GetButtonDown("ArcadeTwo0")) return true;
            if (Input.GetKeyDown("joystick 2 button 0")) return true;
            if (Input.GetKeyDown("joystick 2 button 1")) return true;
            if (Input.GetKeyDown("joystick 2 button 2")) return true;
            if (Input.GetKeyDown("joystick 2 button 3")) return true;
            if (Input.GetKeyDown("joystick 2 button 4")) return true;
            if (Input.GetKeyDown("joystick 2 button 5")) return true;
            if (Input.GetKeyDown("joystick 2 button 6")) return true;
            if (Input.GetKeyDown("joystick 2 button 7")) return true;
        }
        return false;
    }


    private void updateSpawnCrate()
    {
        if (moving) return;
        if (!readyToSpawnCrate) return;
        if (isButtonPressed()) spawnCrate(true);
    }


    public void spawnCrate(bool byButton)
    {
        if (byButton) moveCount++;
        cameraScript.spawnCrate(gridX, gridZ, gameObject);
        if (grid[gridX, gridZ].isDoor())
        {
            cameraScript.toggleDoors();
        }
        gridX = startX;
        gridZ = startZ;
        snapToGrid();
        readyToSpawnCrate = false;
        if (grid[gridX, gridZ].getType() == CameraScript.Element.CRATE)
        {
            grid[gridX, gridZ].smashCrate();
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
        if (value > 0.5f) return -1;
        if (value < -0.5f) return 1;
        return 0;
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

        //int otherPlayerX = otherPlayerScript.getGridX();
        //int otherPlayerZ = otherPlayerScript.getGridZ();

        for (int i = 0; i < 4; i++)
        {
            int dist = 0;
            bool foundblock = false;
            while (!foundblock)
            {
                int xx = (int)(x + arrowDirs[i, 0] * (dist + 1));
                int zz = (int)(z + arrowDirs[i, 1] * (dist + 1));

                if (!cameraScript.isEnterable(xx,zz)) foundblock = true;

                //if (grid[xx, zz].getType() != CameraScript.Element.FLOOR) foundblock = true;
                //else if ((dx == otherPlayerX) && (dz == otherPlayerZ)) foundblock = true;
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
        if (moving || (cameraScript.getGameState() != CameraScript.GameState.PLAYING))
        {
            for (int i = 0; i < 4; i++)
            {
              arrows[i].SetActive(false);
            }
            
            return;
        }

        
        int[] dists = getDistances();

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

    public int getMoveCount() { return moveCount; }
    public int getGoalCount() { return goalCount; }
}
