using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject boardBlock;
    public GameObject player1, player2;
	public Material goalMaterial;
    public Material[] wallMat = new Material[10];

    private const int boardWidth = 24;
    private const int boardHeight = 13;

    public enum Element { FLOOR, WALL, GOAL, PLAYER1, PLAYER2, PORTAL1, PORTAL2, PORTAL3 };
    private Element[,] grid = new Element[boardWidth, boardHeight];

    private PlayerScript playerScript1, playerScript2;

    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();

		/*
        playerScript1.setBoard(grid);
        playerScript2.setBoard(grid);

        playerScript1.setMaxLevelMoves(12);
        playerScript2.setMaxLevelMoves(12);
        for (int x = 0; x < boardWidth; x++)
        {
            for (int z = 0; z < boardHeight; z++)
            {
                grid[x, z] = Element.FLOOR;
                if (x == 0 || z == 0) grid[x, z] = Element.WALL;
                else if (x == boardWidth - 1 || z == boardHeight - 1) grid[x, z] = Element.WALL;
                else if (x == 10 && z == 7) grid[x, z] = Element.PLAYER1;
                else if (x == 14 && z == 7) grid[x, z] = Element.PLAYER2;
                else if (Random.value < 0.04f) grid[x, z] = Element.WALL;
            }
        }
        */

		GameMap[] gameMaps = MapLoader.loadAllMaps ();
		GameMap gameMap = gameMaps [Random.Range (0, gameMaps.Length)];

		playerScript1.setMaxLevelMoves(gameMap.moves);
		playerScript2.setMaxLevelMoves(gameMap.moves);

		playerScript1.setBoard(gameMap.grid);
		playerScript2.setBoard(gameMap.grid);

        // Spawn board blocks
		for (int x = 0; x < gameMap.width; x++)
        {
			for (int z = 0; z < gameMap.height; z++)
            {
				int val = gameMap.grid [x, z];
                GameObject block = Instantiate(boardBlock, new Vector3(x, 0, z), Quaternion.identity);
				if (val == (int)Element.WALL)
                {
                    Renderer renderer = block.GetComponent<Renderer>();
                    renderer.material = wallMat[Random.Range(0, wallMat.Length)];
                    block.transform.Translate(Vector3.up);
                }
				if (val == (int)Element.GOAL) {
					Renderer renderer = block.GetComponent<Renderer>();
					renderer.material = goalMaterial;
					gameMap.grid[x, z] = (int)Element.FLOOR;
				}
				else if (val == (int)Element.PLAYER1)
                {
                    player1.transform.Translate(new Vector3(x, 1, z));
					gameMap.grid[x, z] = (int)Element.FLOOR;
                }
				else if (val == (int)Element.PLAYER2)
                {
                    player2.transform.Translate(new Vector3(x, 1, z));
					gameMap.grid[x, z] = (int)Element.FLOOR;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
