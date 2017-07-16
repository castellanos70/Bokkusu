using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	//based on the current camera FOV 
	private static float fullWidth = 32;
	private static float fullHeight = 14;
    private int gridWidth, gridHeight;
	private float audioPriority = 255;
	private float audioDecrement = 1;
    private float doorToggleSeconds;

	//private AudioSource audio;
	private AudioClip[] harpAudio;

    public GameObject boardBlock;
    public GameObject crateBlock;
    public GameObject player1, player2;


    private Material wallMat;
    private int wallTextureSize = 64;
    private float wallMorphScale = 0.03f;
    private float wallTextureShift;

    private Material floorMat;
    private int floorTextureSize = 64;
    private float floorMorphScale = 0.06f;
    private float floorTextureShift;
   

    public GameObject goalBlock;

    public GameObject backgroundPlane;
    private Background_AbstractScript backgroundScript;

    private Kaleidoscope doorKaleidoscope;
    private Material doorMat;
    private int doorTextureSize = 64;
    private float doorUpdateTime;
    

    public enum GameState { INTRO, INITIALIZING, PLAYING, WON };
    private GameState gameState;
    private GameMap gameMap;
    private Element[,] startMap;

    public enum Element                  { FLOOR, WALL, GOAL, CRATE, PLAYER1, PLAYER2, DOOR_A, DOOR_B, NOTHING };
    public static char[] ELEMENT_ASCII = { '.'  , '#' , '=',  '&',    '1'    , '2'    , 'A',     'B',     ' '    };


    private static Element[] elementValues;
    private GameMap[] gameMapList;
    private Cell[,] grid;
    private int curLevel = 0;
    
    private PlayerScript playerScript1, playerScript2;
    //private float winTime = 0;

    private Vector3 eyePosition1, eyePosition2, eyePosition3, eyePositonAboveGoal;
    private Quaternion eyeRotation1, eyeRotation2, eyeRotation3;
    private int eyeMovingTo;
    private float eyeSpeed = 0.05f;


    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();
        elementValues = (Element[])System.Enum.GetValues(typeof(Element));

        gameMapList = MapLoader.loadAllMaps();
        //audio = gameObject.AddComponent<AudioSource>();
        harpAudio = Resources.LoadAll<AudioClip>("Audio/harpsichord");

        boardBlock.SetActive(false);
        crateBlock.SetActive(false);


        doorMat = new Material(Shader.Find("Standard"));
        doorMat.SetFloat("_Glossiness", 0.0f);
        doorMat.SetFloat("_Metallic", 0.0f);

        wallMat = new Material(Shader.Find("Standard"));
        wallMat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        wallMat.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
        wallTextureShift = Random.value * 100;
        DrawUtilities.generateWallTexture(wallMat, wallTextureSize, wallTextureShift);


        floorMat = new Material(Shader.Find("Standard"));
        floorMat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        floorMat.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
        floorTextureShift = Random.value * 100;
        DrawUtilities.generateFloorTexture(floorMat, floorTextureSize, floorTextureShift);

    }



    void Start()
    {

        backgroundScript = GetComponent<Background_AbstractScript>();
        Texture2D texture = backgroundScript.create();
        Renderer renderer = backgroundPlane.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
        spawnBoard(0);
    }




    private void spawnBoard(int level)
    {
        gameState = GameState.INITIALIZING;
        doorKaleidoscope = new Kaleidoscope(doorMat, doorTextureSize);
        doorUpdateTime = 0;

        audioPriority = 255;
        doorToggleSeconds = -1f;
        curLevel = level;
        destroyOldBoard();


        gameMap = gameMapList[level];
        startMap = gameMap.getMap();
        gridWidth = startMap.GetLength(0);
        gridHeight = startMap.GetLength(1);

        int numCells = 0;

        grid = new Cell[gridWidth, gridHeight];
        bool foundGoal = false;

        int[] pentatonic = { 0, 2, 4, 7, 9 };

        // Spawn board blocks
        //Debug.Log("Spawn Board(" + level + "): " + grid.GetLength(0) + "(" + gridWidth + ") x " + grid.GetLength(1) + "(" + gridHeight + ")");
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (startMap[x, z] == Element.NOTHING) continue;

                numCells++;

                int y = Random.Range(2, 40);
                GameObject block = Instantiate(boardBlock, new Vector3(x, y, z), Quaternion.identity);
                block.SetActive(true);

                Material mat = null;
                if (startMap[x, z] == CameraScript.Element.WALL)
                {
                    //mat = wallMat[Random.Range(0, wallMat.Length)];
                    mat = wallMat;
                }
                else if (startMap[x, z] == CameraScript.Element.DOOR_A || startMap[x, z] == CameraScript.Element.DOOR_B)
                {
                    mat = doorMat;
                }
                else
                {
                    mat = floorMat;//floorMat[Random.Range(0, floorMat.Length)];
                }


                grid[x, z] = new Cell(startMap[x, z], block, mat);
                int audioIndex = pentatonic[Random.Range(0, 5)] + (Random.Range(0, (harpAudio.Length / 12) - 1) * 12);
                grid[x, z].setAudioClip(harpAudio[audioIndex]);

                if (startMap[x, z] == Element.GOAL)
                {
                    if (foundGoal)
                    {
                        Debug.Log("Each level must have Exactly ONE goal.");
                        //UnityEditor.EditorApplication.isPlaying = false;
                    }
                    foundGoal = true;
                    goalBlock.transform.position = new Vector3(x, 1, z);
                }
            }
        }

        audioDecrement = 127.0f / numCells;

        if (!foundGoal)
        {
            Debug.Log("Each level must have Exactly ONE goal.");
            //UnityEditor.EditorApplication.isPlaying = false;
        }


        playerScript1.setBoard(this, grid);
        playerScript2.setBoard(this, grid);

        //Needed when have have more objects moving than just the players
        //entityList.Add(player1);
        //entityList.Add(player2);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (startMap[x, z] == Element.NOTHING) continue;


                if (startMap[x, z] == Element.PLAYER1)
                {
                    //Debug.Log("CameraScript.initGame: player 1: (" + x + "," + z + ")");
                    playerScript1.setStartPosition(x, z);
                    grid[x, z].setType(Element.FLOOR);
                }
                else if (startMap[x, z] == Element.PLAYER2)
                {
                    //Debug.Log("CameraScript.newGame: player 2: (" + x + "," + z + ")");
                    playerScript2.setStartPosition(x, z);
                    grid[x, z].setType(Element.FLOOR);
                }
                else if (startMap[x, z] == Element.CRATE)
                {
                    GameObject crateClone = Instantiate(crateBlock, new Vector3(x, 1, z), Quaternion.identity);
                    crateClone.SetActive(true);
                    grid[x, z].addCrate(crateClone);
                }
            }
        }


        float height = 35;//cameraPosition.y;
        float widthDiff = gridWidth / fullWidth;
        float heightDiff = gridHeight / fullHeight;
        float heightMod = Mathf.Max(widthDiff, heightDiff);

        Vector3 boradCenter = new Vector3(gridWidth / 2.0f - .5f, 0, gridHeight / 2.0f - .5f);

        eyePosition1 = new Vector3(gridWidth / 2.0f - .5f, height * heightMod, gridHeight / 2.0f - .5f);
        transform.position = eyePosition1;
        transform.LookAt(boradCenter, Vector3.up);
        eyeRotation1 = transform.rotation;


        eyePosition2 = new Vector3(3*gridWidth / 4.0f - .5f, height * heightMod * .7f, -(gridHeight + 1));
        transform.position = eyePosition2;
        transform.LookAt(boradCenter, Vector3.up);
        eyeRotation2 = transform.rotation;

        eyePosition3 = new Vector3(gridWidth / 4.0f - .5f, height * heightMod * .7f, -(gridHeight + 1));
        transform.position = eyePosition3;
        transform.LookAt(boradCenter, Vector3.up);
        eyeRotation3 = transform.rotation;

        eyeMovingTo = 1;

        eyePositonAboveGoal = new Vector3(goalBlock.transform.position.x, 2, goalBlock.transform.position.z);

        float scale = Mathf.Max(gridWidth, gridHeight) * 0.25f;
        backgroundPlane.transform.position = new Vector3(gridWidth / 2, 0.4f, gridHeight / 2);
        backgroundPlane.transform.localScale = new Vector3(scale, 1, scale);
        backgroundScript.clear(curLevel);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        backgroundScript.next();
        goalBlock.transform.Rotate(Vector3.up * Time.deltaTime*40);
        backgroundPlane.transform.Rotate(Vector3.up * Time.deltaTime*.5f);

        wallTextureShift += Time.deltaTime * wallMorphScale;
        DrawUtilities.generateWallTexture(wallMat, wallTextureSize, wallTextureShift);

        floorTextureShift += Time.deltaTime * floorMorphScale;
        DrawUtilities.generateFloorTexture(floorMat, floorTextureSize, floorTextureShift);

        if (Time.time > doorUpdateTime)
        {
            doorUpdateTime = Time.time + 0.1f;
            doorKaleidoscope.updateTexture();
        }

        //Debug.Log(wallTextureShift + "," + floorTextureShift);

        if (gameState == GameState.INITIALIZING)
        {
            bool fallingDone = true;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    if (startMap[x, z] == Element.NOTHING) continue;

                    float fallSpeed = grid[x, z].getFallSpeed();

					if (fallSpeed == 0f){
						if (!grid[x,z].hasPlayedAudio()){
							grid[x, z].playAudioClip(audioPriority);
							audioPriority -= audioDecrement;
						}
						continue;
					};

                    float y = grid[x, z].getY() - fallSpeed * Time.deltaTime;

                    grid[x, z].fallTo(y);
                    fallingDone = false;
                }
            }
            if (fallingDone) gameState = GameState.PLAYING;
        }

        else if (gameState == GameState.PLAYING)
        {
            

            movePlayer(player1, playerScript1);
            movePlayer(player2, playerScript2);

            if (eyeMovingTo == 1)
            {
                transform.position = Vector3.Lerp(transform.position, eyePosition1, Time.deltaTime * eyeSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, eyeRotation1, Time.deltaTime * eyeSpeed);
                //Debug.Log("moveTo=1: "+Vector3.Distance(transform.position, eyePosition1));
                if (Vector3.Distance(transform.position, eyePosition1) < 0.5f) eyeMovingTo = 2;
            }
            else if (eyeMovingTo == 2)
            {
                transform.position = Vector3.Lerp(transform.position, eyePosition2, Time.deltaTime * eyeSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, eyeRotation2, Time.deltaTime * eyeSpeed);
                //Debug.Log("moveTo=2: " + Vector3.Distance(transform.position, eyePosition2));
                if (Vector3.Distance(transform.position, eyePosition2) < 0.5f) eyeMovingTo = 3;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, eyePosition3, Time.deltaTime * eyeSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, eyeRotation3, Time.deltaTime * eyeSpeed);
                //Debug.Log("moveTo=3: " + Vector3.Distance(transform.position, eyePosition3));
                if (Vector3.Distance(transform.position, eyePosition3) < 0.5f) eyeMovingTo = 1;
            }


            


            if (doorToggleSeconds >= 0f)
            {
                doorToggleSeconds -= Time.deltaTime;
                if (doorToggleSeconds < 0f) doorToggleSeconds = 0f;
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int z = 0; z < gridHeight; z++)
                    {
                        if (startMap[x, z] == Element.NOTHING) continue;

                        Element type = grid[x, z].getType();
                        if (type == Element.DOOR_A || type == Element.DOOR_B)
                        {
                            grid[x, z].updateDoor(doorToggleSeconds);
                        }
                    }
                }
                if (doorToggleSeconds == 0f) doorToggleSeconds = -1f;
            }
        }

        else if (gameState == GameState.WON)
        {
            if (Vector2.Distance(transform.position, eyePositonAboveGoal) > 3)
            {
                //winTime -= Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, eyePositonAboveGoal, Time.deltaTime * eyeSpeed*4);
                transform.rotation = Quaternion.Lerp(transform.rotation, eyeRotation1, Time.deltaTime * eyeSpeed*2);

                //float dx = (goalBlock.transform.position.x - transform.position.x) * Time.deltaTime * 3;
                //float dz = (goalBlock.transform.position.z - transform.position.z) * Time.deltaTime * 3;
                //float dy = (transform.position.y - 2) * Time.deltaTime;
                //transform.Translate(dx, dz, dy);
            }
            else
            {

                //Do not spawn a board that is the same as the current board.
                int level = curLevel;
                while (level == curLevel)
                {
                    level = Random.Range(1, gameMapList.Length);
                }
                spawnBoard(level);
            }
        }
    }


    private void movePlayer(GameObject player, PlayerScript script)
    {
        if (!script.isMoving()) return;

        float x = player.transform.position.x + script.getSpeedX() * Time.deltaTime;
        float z = player.transform.position.z + script.getSpeedZ() * Time.deltaTime;
        int gridX = script.getGridX();
        int gridZ = script.getGridZ();
        int toX = gridX;
        int toZ = gridZ;

        if (script.getSpeedX() > 0f)
        {
            if (x > gridX) toX = gridX + 1;
        }
        else if (script.getSpeedX() < 0f)
        {
            if (x < gridX) toX = gridX - 1;
        }
        else if (script.getSpeedZ() > 0f)
        {
            if (z > gridZ) toZ = gridZ + 1;
        }
        else if (script.getSpeedZ() < 0f)
        {
            if (z < gridZ) toZ = gridZ - 1;
        }

        if ((toX != gridX) || (toZ != gridZ))
        {
            float speed = script.getSpeedMagnitude();
            bool smashCrate = false;
            if (speed >= CrateScript.getStrength()) smashCrate = true;
            if (!enterIfPossible(toX, toZ, smashCrate, speed))
            {
                script.hit();
                return;
            }
        }
        //Debug.Log("CameraScript.movePlayer(): speed: ("+ script.getSpeedX() + ", "+ script.getSpeedZ()+")");

        script.updateLocation(x, z);
    }





    private void destroyOldBoard()
    {
        if (grid == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (grid[x, z] != null) grid[x, z].destroyObjects();
            }
        }

        //Will be needed when there are more objects moving than just the players
        //foreach (GameObject obj in entityList)
        //{
        //    if (obj == null) continue;
        //    if (obj.tag != "Player")
        //    {
        //        Destroy(obj);
        //    }
        //}
        //entityList.Clear();
    }

    public PlayerScript getPlayerObject(Element playerEnum)
    {
        if (playerEnum == Element.PLAYER1) return playerScript1;
        if (playerEnum == Element.PLAYER2) return playerScript2;
        return null;
    }

    public void spawnCrate(int x, int z, GameObject player)
    {
        //Debug.Log("CameraScript.spawnCrate(" + x + "," + z + ")");
        GameObject crateClone = Instantiate(crateBlock, new Vector3(x, 1, z), Quaternion.identity);
        crateClone.SetActive(true);
        grid[x, z].addCrate(crateClone);

        if (player == player1) crateClone.GetComponent<CrateScript>().spawnAnimation(true);
        else crateClone.GetComponent<CrateScript>().spawnAnimation(false);
    }

    public void setGameState(GameState state)
    {
        gameState = state;
    }

    public GameState getGameState()
    {
        return gameState;
    }


    public bool enterIfPossible(int x, int z, bool smashCrate, float speed)
    {
        if (isEnterable(x, z)) return true;

        Element type = grid[x, z].getType();
        if ((x == playerScript1.getGridX()) && (z == playerScript1.getGridZ()))
        {
            playerScript1.hit();
            return false;
        }
        if ((x == playerScript2.getGridX()) && (z == playerScript2.getGridZ()))
        {
            playerScript2.hit();
            return false;
        }

        if (type == Element.CRATE && smashCrate)
        {
            grid[x, z].smashCrate(speed);
            return true;
        }

        return false;
    }

    public bool isEnterable(int x, int z)
    {
        Element type = grid[x, z].getType();
        if ((x == playerScript1.getGridX()) && (z == playerScript1.getGridZ()))
        {
            return false;
        }
        if ((x == playerScript2.getGridX()) && (z == playerScript2.getGridZ()))
        {
            return false;
        }

        if (type == Element.FLOOR) return true;
        if (type == Element.GOAL) return true;
        if (type == Element.DOOR_A || type == Element.DOOR_B)
        {
            if (grid[x, z].getY() == 0) return true;
        }


        return false;
    }

    public void toggleDoors()
    {
        doorToggleSeconds = 1f;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (startMap[x, z] == Element.NOTHING) continue;

                Element type = grid[x, z].getType();
                if (type == Element.DOOR_A || type == Element.DOOR_B)
                {
                    if ((x == playerScript1.getGridX()) && (z == playerScript1.getGridZ())) continue;
                    
                    if ((x == playerScript2.getGridX()) && (z == playerScript2.getGridZ())) continue;

                    grid[x, z].toggleDoor();
                }
            }
        }
    }






    public static Element getElement(int idx)
    {
		if (idx == -1) return elementValues[0];
        return elementValues[idx];
    }

 


    public static Element getElement(char c)
    {
        if (c == '.') return Element.FLOOR;
        else if (c == '#') return Element.WALL;
        else if (c == '=') return Element.GOAL;
        else if (c == '&') return Element.CRATE;
        else if (c == '1') return Element.PLAYER1;
        else if (c == '2') return Element.PLAYER2;
        else if (c == 'A') return Element.DOOR_A;
        else if (c == 'B') return Element.DOOR_B;
		else if (c == ' ') return Element.NOTHING;
        return Element.FLOOR;
    }
}
 