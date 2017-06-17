using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
	public GameObject arrow;
	public Material arrowMaterial;
    public AudioSource playerAudio;

    public int playerNumber;
    public TextMesh levelMovesTextMesh;
    public int speedMax;
    public int speedMin;
    public int acceleration;

    private int movesRemaining;
    private bool moving = false;
    private float speedX = 0;
    private float speedZ = 0;
    //private int boardWidth, boardHeight;
	//private int prevX = 0;
	//private int prevZ = 0;

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
    private List<GameObject> entityList;
    private CameraScript.Element myPlayer;
    private CameraScript cameraScript;

    private KeyCode[] keycode = new KeyCode[4];
    private HashSet<GameObject> entities;


    public void setMaxLevelMoves(int moveCount)
    {
        movesRemaining = moveCount;
    }

    public void setBoard(CameraScript cameraScript, Cell[,] myGrid, List<GameObject> entityList)
    {
        this.cameraScript = cameraScript;
        grid = myGrid;
        this.entityList = entityList;
        //boardWidth = grid.GetLength(0);
        //boardHeight = grid.GetLength(1);
    }

	public void setPosition(int x, int z)
    {
		transform.position = new Vector3(x, 1, z);
		//prevX = x;
		//prevZ = z;
		arrows = new GameObject[4];

		for (int i = 0; i < 4; i++)
        {
			arrows[i] = Instantiate(arrow, arrow.transform.position, Quaternion.identity);
			arrows[i].GetComponent<Renderer>().material = arrowMaterial;
			arrows[i].SetActive(true);
			arrows[i].transform.position = transform.position + new Vector3(arrowDirs[i, 0]*.5f, 0, arrowDirs[i, 1]*.5f);
			arrows[i].transform.Rotate(new Vector3(90, 90*i, 0));
		}
		arrow.SetActive(false);
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
			timingOffset = 50;
        }
    }
		
	
	// Update is called once per
	void Update ()
    {
        int x0; // current position (rounded down)
        int z0;

        x0 = (int)(transform.position.x);
        z0 = (int)(transform.position.z);

        float speed = (speedX + speedZ);

        playerAudio.pitch = 0.5f + floatToUnit(speed) * (speed / (speedMax / 2.5f));

        displayArrows();

        if (moving)
        {

			//arrows:
			/*for (int i = 0; i < 4; i++){
				arrows[i].SetActive(false);
			}*/

            float x = transform.position.x + speedX * Time.deltaTime;
            float z = transform.position.z + speedZ * Time.deltaTime;

            CameraScript.Element hit = checkMove(x0, z0);

            
            float buff = 0.05f + floatToUnit(speed) * (speed / (speedMax / 0.15f));
            if (hit == CameraScript.Element.WALL && ((speedX >= 0 || Math.Abs(x - x0) < buff) && (speedZ >= 0 || Math.Abs(z - z0) < buff)))
            {
                x = x0; z = z0;
                speedX = 0;
                speedZ = 0;
                moving = false;
                playerAudio.Stop();
            }

            else if (hit == CameraScript.Element.GOAL)
            {
                cameraScript.setGameState(CameraScript.GameState.WON);
                playerAudio.Stop();
            }

            else if (hit == CameraScript.Element.PLAYER1)
            {
                x = x0; z = z0;
                speedX = 0;
                speedZ = 0;
                moving = false;
                playerAudio.Stop();
            }

            else if (Input.GetKey(keycode[0]) && speedX == 0)
            {
                speedZ += acceleration * Time.deltaTime;
				speedZ = toSpeedBounds(speedZ);
            }

            else if (Input.GetKey(keycode[2]) && speedX == 0)
            {
                speedZ -= acceleration * Time.deltaTime;
				speedZ = toSpeedBounds(speedZ);
            }

            else if (Input.GetKey(keycode[1]) && speedZ == 0)
            {
                speedX += acceleration * Time.deltaTime;
				speedX = toSpeedBounds(speedX);
            }

            else if (Input.GetKey(keycode[3]) && speedZ == 0)
            {
                speedX -= acceleration * Time.deltaTime;
				speedX = toSpeedBounds(speedX);
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
                    speedZ = acceleration * Time.deltaTime;
                    speedX = 0;
                    z1 += 1;
                    if (!moving) playerAudio.Play();
                    Debug.Log(playerAudio.isPlaying);
                    moving = true;
                }
                else if (Input.GetKey(keycode[2]))
                {
                    speedZ = -acceleration * Time.deltaTime;
                    speedX = 0;
                    z1 -= 1;
                    
                    if (!moving) playerAudio.Play();
                    Debug.Log(playerAudio.isPlaying);
                    moving = true;
                }
                else if (Input.GetKey(keycode[1]))
                {
                    speedZ = 0;
                    speedX = acceleration * Time.deltaTime;
                    x1 += 1;
                    if (!moving) playerAudio.Play();
                    Debug.Log(playerAudio.isPlaying);
                    moving = true;
                }
                else if (Input.GetKey(keycode[3]))
                {
                    speedZ = 0;
                    speedX = -acceleration * Time.deltaTime;
                    x1 -= 1;
                    if (!moving) playerAudio.Play();
                    
                    moving = true;
                }


                if (moving)
                {
                    CameraScript.Element hit = checkSpot(x1, z1);
                    if (hit != CameraScript.Element.NOTHING)
                    {
                        speedX = 0;
                        speedZ = 0;
                        moving = false;
                        playerAudio.Stop();
                    }
                    else
                    {
                        movesRemaining--;
                        levelMovesTextMesh.text = movesRemaining.ToString();
                    }
                }
                //else grid[prevX, prevZ].setEntity(myPlayer, true);
            }
        }
    }

    public void setEntities(HashSet<GameObject> entities)
    {
        this.entities = entities;
    }

   public int getDirectionX()
    {
        return floatToUnit(speedX);
    }

    public int getDirectionZ()
    {
        return floatToUnit(speedZ);
    }

    private CameraScript.Element checkMove(int x0, int z0)
    {
        

        int x1 = x0;
        int z1 = z0;

        /*if (speedX > 0) x1 = (int)(x + 1);
        else if (speedX < 0) x1 = (int)(x);
        else if (speedZ > 0) z1 = (int)(z + 1);
        else if (speedZ < 0) z1 = (int)(z);*/

        if (speedX > 0) x1 = x0 + 1;
        else if (speedX < 0) x1 = x0 - 1;
        else if (speedZ > 0) z1 = z0 + 1;
        else if (speedZ < 0) z1 = z0 - 1;

        //Debug.Log(Time.time + ": [" + x0 + "," + z0 + "]===> "+ " grid[" +x1+","+z1+"]="+grid[x1,z1]);

        /*if (grid[x1, z1].getEnvironment() == CameraScript.Element.FLOOR) return false;
		if (grid[x1, z1].getEnvironment() == CameraScript.Element.GOAL) return false;
        if (grid[x1, z1].getEntity() == myPlayer) return false;*/

        if (grid[x1, z1].getEntity() == CameraScript.Element.CRATE) // returning NOTHING is like returning false
        {

            if (speedX > CrateScript.getStrength() || speedX < -CrateScript.getStrength()
                || speedZ > CrateScript.getStrength() || speedZ < -CrateScript.getStrength())
            {
                return CameraScript.Element.NOTHING;
            }
            else return CameraScript.Element.WALL;
        }

        else if (grid[x1, z1].getEntity() == CameraScript.Element.GOAL) return CameraScript.Element.GOAL;

        else if ((grid[x1, z1].getEntity() == CameraScript.Element.NOTHING
            || grid[x1, z1].getEntity() == myPlayer)
            && grid[x1, z1].getEnvironment() != CameraScript.Element.WALL) { return CameraScript.Element.NOTHING; }

        else if (grid[x1, z1].getEnvironment() == CameraScript.Element.WALL) { return CameraScript.Element.WALL; }

        return CameraScript.Element.PLAYER1; // Collision detection will only use PLAYER1 regardless of which player it is
    }
		
    private CameraScript.Element checkSpot(int x1, int z1) // returning NOTHING is like returning false
    {
        /*if (grid[x1, z1].getEnvironment() == CameraScript.Element.FLOOR) return false;
        if (grid[x1, z1].getEnvironment() == CameraScript.Element.GOAL) return false;
        if (grid[x1, z1].getEntity() == myPlayer) return false;*/

        if (grid[x1, z1].getEntity() == CameraScript.Element.CRATE)
        {
            if (speedX > CrateScript.getStrength() || speedX < -CrateScript.getStrength()
                || speedZ > CrateScript.getStrength() || speedZ < -CrateScript.getStrength())
            {
                return CameraScript.Element.NOTHING;
            }
            else return CameraScript.Element.WALL;
        }

        else if (grid[x1, z1].getEntity() == CameraScript.Element.GOAL) return CameraScript.Element.GOAL;

        else if ((grid[x1, z1].getEntity() == CameraScript.Element.NOTHING
            || grid[x1, z1].getEntity() == myPlayer)
            && grid[x1, z1].getEnvironment() != CameraScript.Element.WALL) { return CameraScript.Element.NOTHING; }

        else if (grid[x1, z1].getEnvironment() == CameraScript.Element.WALL) { return CameraScript.Element.WALL; }

        return CameraScript.Element.PLAYER1; // Collision detection will only use PLAYER1 regardless of which player it is
    }

	private float toSpeedBounds(float n){
		if (n > speedMax) n = speedMax;
		else if (n < -speedMax) n = -speedMax;
		else if (n > 0 && n < speedMin) n = speedMin;
		else if (n < 0 && n > -speedMin) n = -speedMin;

		return n;
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

    private int[] getDistances(){
		int[] dists = new int[4];
		float x = transform.position.x;
		float z = transform.position.z;

		for (int i = 0; i < 4; i++){
			int dist = 0;
			bool foundblock = false;
			while(!foundblock){
				int dx = (int)(x + arrowDirs[i, 0]*(dist+1));
				int dz = (int)(z + arrowDirs[i, 1]*(dist+1));
				Cell c = grid[dx, dz];
				if ((c.getEntity() == CameraScript.Element.NOTHING
					|| c.getEntity() == myPlayer)
					&& c.getEnvironment() != CameraScript.Element.WALL){
					dist++;
				} else {
					foundblock = true;
				}
			}
			dists[i] = dist;
		}
		return dists;
	}

    private void displayArrows()
    {
        int[] dists = getDistances();
        //arrows:
        if (speedX == 0) { arrows[1].SetActive(true); arrows[3].SetActive(true); }
        else { arrows[1].SetActive(false); arrows[3].SetActive(false); }
        if (speedZ == 0) { arrows[0].SetActive(true); arrows[2].SetActive(true); }
        else { arrows[0].SetActive(false); arrows[2].SetActive(false); }
        // I know this could've gone in the loop, but I couldn't get it to work there

        for (int i = 0; i < 4; i++)
        {
            //dists[i] = 1; // This just makes the arrows 1 block long
            arrows[i].transform.position = transform.position + new Vector3(arrowDirs[i, 0] * .5f, 0, arrowDirs[i, 1] * .5f);
            float width = Mathf.Sin((Time.frameCount + timingOffset) / 15f) * .2f + .8f; //ocelation
            arrows[i].transform.localScale = new Vector3((dists[i]), width, 1); //this works because of relative rotation
                                                                                //Debug.Log(dists[i]);
        }
    }
}
