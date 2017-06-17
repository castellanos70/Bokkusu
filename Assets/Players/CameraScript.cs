using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	//based on the current camera FOV 
	private static float fullWidth = 32;
	private static float fullHeight = 14;

    public GameObject boardBlock;
    public GameObject crateBlock;
    public GameObject player1, player2;
    public Material[] wallMat = new Material[10];
    public Material[] floorMat = new Material[5];
    public GameObject goalBlock;

    public enum GameState { INTRO, PLAYING, LOST, WON };
    private GameState gameState;

    public enum Element                  { FLOOR, WALL, GOAL, CRATE, PLAYER1, PLAYER2, PORTALA, PORTALB, PORTALC, NOTHING };
    public static char[] ELEMENT_ASCII = { '.'  , '#' , '=',  '&',    '1'    , '2'    , 'A',     'B',     'C',     ' '     };


    private static Element[] elementValues;
    private GameMap[] gameMaps;
    private GameMap gameMap;
    private Cell[,] grid;

    private PlayerScript playerScript1, playerScript2;

    private List<HitObject> hitList = new List<HitObject>();

    private float winTime = 0;



    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();

        elementValues = (Element[])System.Enum.GetValues(typeof(Element));

        gameMaps = MapLoader.loadAllMaps ();
        newGame(0);
    }

    // Update is called once per frame
    void Update()
    {
        clearMapOfPlayers();
        goalBlock.transform.Rotate(Vector3.up * Time.deltaTime*40);
        //goalBlock.transform.Rotate(Vector3.right * Time.deltaTime * 5);
        //float scale = 1 + 0.2f*Mathf.Abs(Mathf.Sin(2*Mathf.PI*goalBlock.transform.eulerAngles.y/180f));
        //goalBlock.transform.localScale = new Vector3(scale,1, scale);

        if (gameState == GameState.WON)
        {
            if (winTime > 0)
            {
                winTime -= Time.deltaTime;


                float dx = (goalBlock.transform.position.x - transform.position.x) * Time.deltaTime * 3;
                float dz = (goalBlock.transform.position.z - transform.position.z) * Time.deltaTime * 3;
                float dy = (transform.position.y - 2) * Time.deltaTime;
                Debug.Log("transform.position=" + transform.position);
                transform.Translate(dx, dz, dy);
                //if (dist > 2)
                //{
                //    transform.LookAt(goalBlock.transform);
                //    transform.position += Vector3.forward * Time.deltaTime * 1;
                //}
                //transform.Rotate(Vector3.up * Time.deltaTime * 25);
            }
            else
            {   int level = Random.Range(0, gameMaps.Length);
                newGame(level);
            }

        }
    }


    private void newGame(int level)
    {

        // destroy old board
        if (gameMap != null)
        {
            for (int x = 0; x < gameMap.width; x++)
            {
                for (int z = 0; z < gameMap.height; z++)
                {
                    grid[x, z].destroyEnvironment();
                }
            }
        }




        gameMap = gameMaps[level];
        grid = gameMap.grid;


        playerScript1.setMaxLevelMoves(gameMap.moves[0]);
        playerScript2.setMaxLevelMoves(gameMap.moves[1]);

        playerScript1.setBoard(this, grid);
        playerScript2.setBoard(this, grid);

        Vector3 cameraPosition = transform.position;

        // Spawn board blocks
        for (int x = 0; x < gameMap.width; x++)
        {
            for (int z = 0; z < gameMap.height; z++)
            {
                if (grid[x, z].getEnvironment() == Element.NOTHING) continue;

                GameObject block = Instantiate(boardBlock, new Vector3(x, 0, z), Quaternion.identity);
                grid[x, z].setEnvironmentObj(block);

                GameObject crateClone;

                if (grid[x, z].getEnvironment() == Element.WALL)
                {
                    
                    Renderer renderer = block.GetComponent<Renderer>();
                    renderer.material = wallMat[Random.Range(0, wallMat.Length)];
                    block.transform.Rotate(new Vector3(0, 90 * Random.Range(0, 4), 0));

                    block.transform.Translate(Vector3.up);
                }
                else if (grid[x, z].getEntity() == Element.CRATE)
                {
                    crateClone = Instantiate(crateBlock, new Vector3(x, 1, z), Quaternion.identity);
                    crateClone.GetComponent<CrateScript>().assignGameObject(crateClone);
                    crateClone.GetComponent<CrateScript>().setBoard(this, grid);

                    block.transform.Rotate(new Vector3(180, 180, 180));
                }
                else if (grid[x, z].getEntity() == Element.PLAYER1)
                {
                    playerScript1.setPosition(x, z);
                }
                else if (grid[x, z].getEntity() == Element.PLAYER2)
                {
                    playerScript2.setPosition(x, z);
                }
				else if (grid[x, z].getEntity() == Element.GOAL)
                {
                    goalBlock.transform.position = new Vector3(x, 1, z);
                }

                if (grid[x, z].getEnvironment() == Element.FLOOR)

                {
                    Renderer renderer = block.GetComponent<Renderer>();
                    renderer.material = floorMat[Random.Range(0, floorMat.Length)];
                    block.transform.Rotate(new Vector3(0, 90 * Random.Range(0, 4), 0));
                }
            }
        }

        boardBlock.SetActive(false);
        crateBlock.SetActive(false);

        gameState = GameState.PLAYING;

        float height = cameraPosition.y;
        float widthDiff = gameMap.width / fullWidth;
        float heightDiff = gameMap.height / fullHeight;
        float heightMod = Mathf.Max(widthDiff, heightDiff);

        Debug.Log(widthDiff + ", " + heightDiff);

        transform.position = new Vector3(gameMap.width / 2.0f - .5f, height * heightMod, gameMap.height / 2.0f - .5f);


    }

    public void setGameState(GameState state)
    {
        gameState = state;
        if (gameState == GameState.WON)
        {
            //Time.timeScale = 0;
            player1.SetActive(false);
            player2.SetActive(false);
            winTime = 3;
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
        else if (c == 'A') return Element.PORTALA;
        else if (c == 'B') return Element.PORTALB;
        else if (c == 'C') return Element.PORTALC;
		else if (c == ' ') return Element.NOTHING;
        return Element.FLOOR;
    }

    public void clearMapOfPlayers()
    {
        for (int i = 0; i < gameMap.width; i++)
        {
            for (int j = 0; j < gameMap.height; j++)
            {
                grid[i, j].removeEntity(Element.PLAYER1);
                grid[i, j].removeEntity(Element.PLAYER2);
            }
        }
        int x1 = (int)player1.transform.position.x;
        int z1 = (int)player1.transform.position.z;
        int x2 = (int)player2.transform.position.x;
        int z2 = (int)player2.transform.position.z;
        //grid[(int)player1.transform.position.x, (int)player1.transform.position.z].setEntity(Element.PLAYER1, false);
        //grid[(int)player2.transform.position.x, (int)player2.transform.position.z].setEntity(Element.PLAYER2, false);
        grid[x1, z1].setEntity(Element.PLAYER1, true);
        grid[x2, z2].setEntity(Element.PLAYER2, true);
        grid[x1 + playerScript1.getDirectionX(), z1 + playerScript1.getDirectionZ()].setEntity(Element.PLAYER1, false);
        grid[x2 + playerScript2.getDirectionX(), z2 + playerScript2.getDirectionZ()].setEntity(Element.PLAYER2, false);
        //Debug.Log("x1: " + playerScript1.getDirectionX());
        //Debug.Log("z1: " + playerScript1.getDirectionZ());
        //if (playerScript1.getDirectionX() == 0 && playerScript1.getDirectionZ() == 0) Debug.Log("testing");
    }



    class HitObject
    {
        //GameObject.
    }
}
