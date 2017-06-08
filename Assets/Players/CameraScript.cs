using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject boardBlock;
    public GameObject player1, player2;
    public Material[] wallMat = new Material[10];

    private const int boardSize = 25;

    public enum Element { FLOOR, WALL, GOAL, PLAYER1, PLAYER2, PORTAL1, PORTAL2, PORTAL3 };
    private Element[,] grid = new Element[boardSize, boardSize];

    private PlayerScript playerScript1, playerScript2;

    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();

        playerScript1.setBoard(grid, boardSize);
        playerScript2.setBoard(grid, boardSize);


        //TODO: (Ben) populate grid from ASCII file	
        playerScript1.setMaxLevelMoves(15);
        playerScript2.setMaxLevelMoves(8);
        for (int x = 0; x < boardSize; x++)
        {
            for (int z = 0; z < boardSize; z++)
            {
                grid[x, z] = Element.FLOOR;
                if (x == 0 || z == 0) grid[x, z] = Element.WALL;
                else if (x == boardSize - 1 || z == boardSize - 1) grid[x, z] = Element.WALL;
                else if (x == 12 && z == 10) grid[x, z] = Element.PLAYER1;
                else if (x == 12 && z == 14) grid[x, z] = Element.PLAYER2;
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

                    player1.transform.Translate(new Vector3(x, 1, z));
                    grid[x, z] = Element.FLOOR;
                }
                else if (grid[x, z] == Element.PLAYER2)
                {

                    player2.transform.Translate(new Vector3(x, 1, z));
                    grid[x, z] = Element.FLOOR;
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
