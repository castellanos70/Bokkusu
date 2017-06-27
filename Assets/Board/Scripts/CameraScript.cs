using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	//based on the current camera FOV 
	private static float fullWidth = 32;
	private static float fullHeight = 14;
    private int gridWidth, gridHeight;

    public GameObject boardBlock;
    public GameObject crateBlock;
    public GameObject player1, player2;
    public Material[] wallMat = new Material[10];
    public Material[] floorMat = new Material[5];
    public GameObject goalBlock;

    public enum GameState { INTRO, INITIALIZING, PLAYING, LOST, WON };
    private GameState gameState;
    private GameMap gameMap;
    private Element[,] startMap;

    public enum Element                  { FLOOR, WALL, GOAL, CRATE, PLAYER1, PLAYER2, PORTALA, PORTALB, PORTALC, NOTHING };
    public static char[] ELEMENT_ASCII = { '.'  , '#' , '=',  '&',    '1'    , '2'    , 'A',     'B',     'C',     ' '    };


    private static Element[] elementValues;
    private GameMap[] gameMapList;
    private Cell[,] grid;


    //Needed when have have more objects moving than just the players
    //private List<GameObject> entityList = new List<GameObject>();

    private PlayerScript playerScript1, playerScript2;
    private float winTime = 0;



    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();
        elementValues = (Element[])System.Enum.GetValues(typeof(Element));

        gameMapList = MapLoader.loadAllMaps ();
        boardBlock.SetActive(false);
        crateBlock.SetActive(false);
    }



    void Start()
    {
        spawnBoard(0);
        initGame();
    }

    // Update is called once per frame
    void Update()
    {
        goalBlock.transform.Rotate(Vector3.up * Time.deltaTime*40);
        //goalBlock.transform.Rotate(Vector3.right * Time.deltaTime * 5);
        //float scale = 1 + 0.2f*Mathf.Abs(Mathf.Sin(2*Mathf.PI*goalBlock.transform.eulerAngles.y/180f));
        //goalBlock.transform.localScale = new Vector3(scale,1, scale);

        if (gameState == GameState.PLAYING)
        {
            movePlayer(player1, playerScript1);
            movePlayer(player2, playerScript2);

            if (!playerScript1.isMoving() && !playerScript2.isMoving())
            {
                if (!playerScript1.canMove() && !playerScript2.canMove())
                {
                    setGameState(GameState.LOST);
                }
            }
        }

        else if (gameState == GameState.LOST)
        {
            initGame();
        }
        else if (gameState == GameState.WON)
        {
            if (winTime > 0)
            {
                winTime -= Time.deltaTime;


                float dx = (goalBlock.transform.position.x - transform.position.x) * Time.deltaTime * 3;
                float dz = (goalBlock.transform.position.z - transform.position.z) * Time.deltaTime * 3;
                float dy = (transform.position.y - 2) * Time.deltaTime;
                //Debug.Log("transform.position=" + transform.position);
                transform.Translate(dx, dz, dy);
                //if (dist > 2)
                //{
                //    transform.LookAt(goalBlock.transform);
                //    transform.position += Vector3.forward * Time.deltaTime * 1;
                //}
                //transform.Rotate(Vector3.up * Time.deltaTime * 25);
            }
            else
            {
                //Never spawn level 0 board except at start up
                int level = Random.Range(1, gameMapList.Length);
                spawnBoard(level);
                initGame();
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
            float speed = Mathf.Max(Mathf.Abs(script.getSpeedX()), Mathf.Abs(script.getSpeedZ()));
            bool smashCrate = false;
            if (speed >= CrateScript.getStrength()) smashCrate = true;
            if (!isEnterable(toX, toZ, smashCrate))
            {
                script.hit();
                return;
            }
        }
        //Debug.Log("CameraScript.movePlayer(): speed: ("+ script.getSpeedX() + ", "+ script.getSpeedZ()+")");

        script.updateLocation(x, z);
    }


    private void spawnBoard(int level)
    {
        destroyOldBoard();

        gameMap = gameMapList[level];
        startMap = gameMap.getMap();
        gridWidth = startMap.GetLength(0);
        gridHeight = startMap.GetLength(1);

        grid = new Cell[gridWidth, gridHeight];
        bool foundGoal = false;

        // Spawn board blocks
        Debug.Log("Spawn Board(" + level + "): " + grid.GetLength(0) + "(" + gridWidth + ") x " + grid.GetLength(1) + 
            "("+ gridHeight + ")");
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (startMap[x, z] == Element.NOTHING) continue;

                GameObject block = Instantiate(boardBlock, new Vector3(x, 0, z), Quaternion.identity);
                block.SetActive(true);

                Material mat = null;
                if (startMap[x, z] == CameraScript.Element.WALL)
                {
                    block.transform.Translate(Vector3.up);
                    mat = wallMat[Random.Range(0, wallMat.Length)];
                }
                else
                {
                    mat = floorMat[Random.Range(0, floorMat.Length)];
                }

                   
                grid[x, z] = new Cell(startMap[x, z], block, mat);

                if (startMap[x, z] == Element.GOAL)
                {
                    if (foundGoal)
                    {
                        Debug.Log("Each level must have Exactly ONE goal.");
                        UnityEditor.EditorApplication.isPlaying = false;
                    }
                    foundGoal = true;
                    goalBlock.transform.position = new Vector3(x, 1, z);
                    grid[x, z].addOverlay(goalBlock);
                }
            }
        }

        if (!foundGoal)
        {
            Debug.Log("Each level must have Exactly ONE goal.");
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }




    private void initGame()
    {
       
        playerScript1.setBoard(this, grid, gameMap.getMaxMoves(Element.PLAYER1));
        playerScript2.setBoard(this, grid, gameMap.getMaxMoves(Element.PLAYER2));

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
                    Debug.Log("CameraScript.initGame: player 1: (" + x + "," + z + ")");
                    playerScript1.setStartPosition(x, z);
                    grid[x, z].setType(Element.FLOOR);
                }
                else if (startMap[x, z] == Element.PLAYER2)
                {
                    Debug.Log("CameraScript.newGame: player 2: (" + x + "," + z + ")");
                    playerScript2.setStartPosition(x, z);
                    grid[x, z].setType(Element.FLOOR);
                }
                else if (startMap[x, z] == Element.CRATE)
                {
                    GameObject crateClone = Instantiate(crateBlock, new Vector3(x, 1, z), Quaternion.identity);
                    crateClone.SetActive(true);
                    grid[x, z].addOverlay(crateClone);
                    //block.transform.Rotate(new Vector3(180, 180, 180));
                }
            }
        }


        float height = 35;//cameraPosition.y;
        float widthDiff = gridWidth / fullWidth;
        float heightDiff = gridHeight / fullHeight;
        float heightMod = Mathf.Max(widthDiff, heightDiff);

        transform.position = new Vector3(gridWidth / 2.0f - .5f, height * heightMod, gridHeight / 2.0f - .5f);

        gameState = GameState.PLAYING;
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

    public void setGameState(GameState state)
    {
        gameState = state;
        if (gameState == GameState.WON)
        {
            //Time.timeScale = 0;
            //player1.SetActive(false);
            //player2.SetActive(false);
            winTime = 3;
        }
    }

    public GameState getGameState()
    {
        return gameState;
    }

    public bool isEnterable(int x, int z, bool smashCrate)
    {
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
            grid[x, z].smashCrate();
            return true;
        }

        if (type == Element.FLOOR) return true;
        if (type == Element.GOAL) return true;
        return false;
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
        else if (c == 'A') return Element.PORTALA;
        else if (c == 'B') return Element.PORTALB;
        else if (c == 'C') return Element.PORTALC;
		else if (c == ' ') return Element.NOTHING;
        return Element.FLOOR;
    }
}