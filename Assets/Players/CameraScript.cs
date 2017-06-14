using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	//based on the current camera FOV 
	private static float fullWidth = 32;
	private static float fullHeight = 14;

    public GameObject boardBlock;
    public GameObject player1, player2;
    public Material[] wallMat = new Material[10];
    public Material[] floorMat = new Material[4];
    public GameObject goalBlock;

    private const int boardWidth = 24;
    private const int boardHeight = 13;


    public enum GameState { INTRO, PLAYING, LOST, WON };
    private GameState gameState;

    public enum Element                  { FLOOR, WALL, GOAL, PLAYER1, PLAYER2, PORTALA, PORTALB, PORTALC, NOTHING };
    public static char[] ELEMENT_ASCII = { '.'  , '#' , '=',  '1'    , '2'    , 'A',     'B',     'C',     ' '     };


    private static Element[] elementValues;
    private GameMap[] gameMaps;
    private GameMap gameMap;
    private Cell[,] grid;

    private PlayerScript playerScript1, playerScript2;

    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();

        elementValues = (Element[])System.Enum.GetValues(typeof(Element));

        gameMaps = MapLoader.loadAllMaps ();
		gameMap = gameMaps [Random.Range (0, gameMaps.Length)];
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
				if (grid[x, z].getEnvironment() == Element.WALL)
                {
                    Renderer renderer = block.GetComponent<Renderer>();
                    renderer.material = wallMat[Random.Range(0, wallMat.Length)];
                    block.transform.Rotate(new Vector3(0,90*Random.Range(0, 4),0));

                    block.transform.Translate(Vector3.up);
                }
				else if (grid[x, z].getEntity() == Element.PLAYER1)
                {
					playerScript1.setPosition (x, z);
                }
				else if (grid[x, z].getEntity() == Element.PLAYER2)
                {
					playerScript2.setPosition (x, z);
                }
				else if (grid[x, z].getEnvironment() == Element.GOAL)
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

        gameState = GameState.PLAYING;

		float height = cameraPosition.y;
		float widthDiff = gameMap.width/fullWidth;
		float heightDiff = gameMap.height/fullHeight;
		float heightMod = Mathf.Max(widthDiff, heightDiff);

		Debug.Log(widthDiff + ", " + heightDiff);

		transform.position = new Vector3(gameMap.width/2.0f - .5f, height*heightMod, gameMap.height/2.0f - .5f);

    }

    // Update is called once per frame
    void Update()
    {
        goalBlock.transform.Rotate(Vector3.up * Time.deltaTime*40);
        //goalBlock.transform.Rotate(Vector3.right * Time.deltaTime * 5);
        //float scale = 1 + 0.2f*Mathf.Abs(Mathf.Sin(2*Mathf.PI*goalBlock.transform.eulerAngles.y/180f));
        //goalBlock.transform.localScale = new Vector3(scale,1, scale);
    }


    public void setGameState(GameState state)
    {
        gameState = state;
        if (gameState == GameState.WON)
        {
            Time.timeScale = 0;
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
        else if (c == '1') return Element.PLAYER1;
        else if (c == '2') return Element.PLAYER2;
        else if (c == 'A') return Element.PORTALA;
        else if (c == 'B') return Element.PORTALB;
        else if (c == 'C') return Element.PORTALC;
		else if (c == ' ') return Element.NOTHING;
        return Element.FLOOR;
    }
}
