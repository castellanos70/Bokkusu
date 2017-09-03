using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	//based on the current camera FOV 
	private static float fullWidth = 32;
	private static float fullHeight = 14;
    private int gridWidth, gridHeight;

    private float doorToggleSeconds;

	//private AudioSource audio;
	private AudioClip[] harpAudio;
	private AudioClip[] pentAudio; //pantatonic scale
    public AudioClip[] harpEndLevel; 
    public AudioSource cameraAudio;

    private static int MAX_AUDIO_TRACKS = 32;
    private AudioSource[] cellAudioList = new AudioSource[MAX_AUDIO_TRACKS];
    private int cellAudioIdx;

    public GameObject boardBlock;
    public GameObject crateBlock;
    public GameObject player1, player2;
    public GameObject dustBunny1, dustBunny2;

    public GameObject textLevelName, textScore1, textScore2;
    private UnityEngine.UI.Text textScore1Data, textScore2Data;

    private int dustBunnyPhase;

    private int frameCount;
    private float timeOfLevel;


    private Material wallMat;
    private NoiseTexture wallScript;
    private int wallTextureSize = 128;
    private float wallMorphScale = 0.05f;

    private Material floorMat;
    private NoiseTexture floorScript;
    private int floorTextureSize = 128;
    private float floorMorphScale = 0.1f;
   

    public GameObject goalBlock;

    public GameObject backgroundPlane;
    private Background_AbstractScript backgroundScript;

    private Kaleidoscope doorKaleidoscope;
    private Material doorMat1, doorMat2;
    private int doorTextureSize = 256;


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
    private float eyeSpeed = 0.09f;
    private int player1Score, player2Score, player1ScoreEndLevel, player2ScoreEndLevel;


    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();
        elementValues = (Element[])System.Enum.GetValues(typeof(Element));

        textScore1Data = textScore1.GetComponent<UnityEngine.UI.Text>();
        textScore2Data = textScore2.GetComponent<UnityEngine.UI.Text>();

        player1Score = 0;
        player2Score = 0;

        gameMapList = MapLoader.loadAllMaps();

		{//audio stuff
			harpAudio = Resources.LoadAll<AudioClip>("Audio/harpsichord");
			int[] pentatonic = { 0, 2, 4, 7, 9 };

			pentAudio = new AudioClip[(int)(harpAudio.Length*(5f/12f))];

			//Debug.Log("pent: " + pentAudio.Length);

			int pentLength = 0;
			for (int i = 0; i < harpAudio.Length; i++){
				bool isPent = false;
				for (var j = 0; j < 5; j++){ 
					if (i%12 == pentatonic[j]){
						isPent = true; break;
					}
				}
				if (isPent && pentLength < pentAudio.Length){
					pentAudio[pentLength] = harpAudio[i];
					pentLength++;
				}
			}
		}
			
        boardBlock.SetActive(false);
        crateBlock.SetActive(false);
    }


    void Start()
    {
        doorMat1 = new Material(Shader.Find("Standard"));
        doorMat2 = new Material(Shader.Find("Standard"));
        doorKaleidoscope = new Kaleidoscope(doorMat1, doorMat2, doorTextureSize);
        Cell.setDoorMat(doorMat1, doorMat2);

        wallMat = new Material(Shader.Find("Standard"));
        wallScript = new NoiseTexture(wallMat, wallTextureSize, 0.02f, wallMorphScale, 1);

        floorMat = new Material(Shader.Find("Standard"));
        floorScript = new NoiseTexture(floorMat, floorTextureSize, 0.03f, floorMorphScale, 0);




        backgroundScript = GetComponent<Background_AbstractScript>();
        Texture2D texture = backgroundScript.create();
        Renderer renderer = backgroundPlane.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
        backgroundScript.clear();

        Cursor.visible = false;
        spawnBoard(0);
    }




    private void spawnBoard(int level)
    {
        gameState = GameState.INITIALIZING;
        textLevelName.SetActive(false);
        textScore1.SetActive(false);
        textScore2.SetActive(false);


        dustBunny1.SetActive(false);
        dustBunny2.SetActive(false);
        dustBunnyPhase = 0;

        doorToggleSeconds = -1f;
        frameCount = 0;
        timeOfLevel = 0;
        cellAudioIdx = 0;

        curLevel = level;
        destroyOldBoard();

        gameMap = gameMapList[level];
        startMap = gameMap.getMap();
        gridWidth = startMap.GetLength(0);
        gridHeight = startMap.GetLength(1);

        int numCells = 0;

        grid = new Cell[gridWidth, gridHeight];
        bool foundGoal = false;

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
                    mat = wallMat;
                }
                else if (startMap[x, z] == CameraScript.Element.DOOR_A)
                {
                    mat = doorMat1;
                }
                else if (startMap[x, z] == CameraScript.Element.DOOR_B)
                {
                    mat = doorMat2;
                }
                else
                {
                    mat = floorMat;
                }

				grid[x, z] = new Cell(startMap[x, z], block, mat);

				int audioIndex = (int)(((40 - y)/40.0f)*pentAudio.Length);
				//Debug.Log("audioIndex="+audioIndex);

                grid[x, z].setAudioClip(pentAudio[audioIndex]);

                if (startMap[x, z] == Element.GOAL)
                {
                    if (foundGoal)
                    {
                        Debug.Log("Each level must have Exactly ONE goal.");
                    }
                    foundGoal = true;
                    goalBlock.transform.position = new Vector3(x, 1, z);
                }
            }
        }

        if (!foundGoal)
        {
            Debug.Log("Each level must have Exactly ONE goal.");
        }


        playerScript1.setBoard(this, grid);
        playerScript2.setBoard(this, grid);

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

        //eyePosition1 = new Vector3(gridWidth / 2.0f - .5f, height * heightMod, -3*gridHeight / 5.0f);
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
        if (backgroundScript.isDone()) backgroundScript.clear();

    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;
        timeOfLevel += Time.deltaTime;

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKey(KeyCode.Equals))
        {
            if (gameState == GameState.PLAYING)
            {
                Debug.Log("Advance to Next Level....");
                setGameState(GameState.WON);
            }
        }

        backgroundScript.next();
        goalBlock.transform.Rotate(Vector3.up * Time.deltaTime*40);
        backgroundPlane.transform.Rotate(Vector3.up * Time.deltaTime*.5f);

        wallScript.next();
        floorScript.next();

        doorKaleidoscope.updateTexture();

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

                    if (fallSpeed == 0f) continue;

                    float y = grid[x, z].getY() - fallSpeed * Time.deltaTime;

                    bool hitBottom = grid[x, z].fallTo(y);
                    if (hitBottom)
                    {
                        if (cellAudioList[cellAudioIdx] != null)
                        {
                            if (cellAudioList[cellAudioIdx].isPlaying) cellAudioList[cellAudioIdx].Stop();
                        }
                        cellAudioList[cellAudioIdx] = grid[x, z].playAudio();
                        cellAudioIdx = (cellAudioIdx + 1 ) % MAX_AUDIO_TRACKS;
                    }
                    else fallingDone = false;
                }
            }
			if (fallingDone){
				gameState = GameState.PLAYING;
                playerScript1.startPlaying();
                playerScript2.startPlaying();
            }
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
                if (Vector3.Distance(transform.position, eyePosition1) < 2.0f) eyeMovingTo = 2;
            }
            else if (eyeMovingTo == 2)
            {
                transform.position = Vector3.Lerp(transform.position, eyePosition2, Time.deltaTime * eyeSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, eyeRotation2, Time.deltaTime * eyeSpeed);
                //Debug.Log("moveTo=2: " + Vector3.Distance(transform.position, eyePosition2));
                if (Vector3.Distance(transform.position, eyePosition2) < 1.0f) eyeMovingTo = 3;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, eyePosition3, Time.deltaTime * eyeSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, eyeRotation3, Time.deltaTime * eyeSpeed);
                //Debug.Log("moveTo=3: " + Vector3.Distance(transform.position, eyePosition3));
                if (Vector3.Distance(transform.position, eyePosition3) < 1.0f) eyeMovingTo = 1;
            }

            //doorDownAngle = doorDownAngle - 0.1f * Time.deltaTime;
            //float doorScale = 1 - 0.2f* Mathf.Abs(Mathf.Sin(2 * doorDownAngle));


            if (doorToggleSeconds >= 0f)
            {
                doorToggleSeconds -= Time.deltaTime;
                if (doorToggleSeconds < 0f)
                {
                    doorToggleSeconds = 0f;
                    //doorDownAngle = 0f;
                }
            }



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




            //Dustbunny
            if ((dustBunnyPhase == 0) && (Random.value < 0.002))
            {
                float dustBunnyY1 = gridWidth + gridHeight - Random.value * 4f;
                float dustBunnyY2 = gridWidth + gridHeight - Random.value * 4f;
                float dustBunnyX = goalBlock.transform.position.x;
                float dustBunnyZ = goalBlock.transform.position.z;
                dustBunny1.transform.position = new Vector3(dustBunnyX, dustBunnyY1, dustBunnyZ);
                dustBunny2.transform.position = new Vector3(dustBunnyX, dustBunnyY2, dustBunnyZ);

                dustBunny1.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                dustBunny2.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                dustBunny1.SetActive(true);
                dustBunny2.SetActive(true);
                dustBunnyPhase = 1;
                //Debug.Log(" dustBunnyY=" + dustBunnyY);
            }
            else if (dustBunnyPhase == 1)
            {
                updateDustBunny(dustBunny1, 0f);
                updateDustBunny(dustBunny2, Mathf.PI);

                if (!dustBunny1.activeSelf && !dustBunny2.activeSelf)
                {
                    dustBunnyPhase = 0;
                }
            }
        }

        else if (gameState == GameState.WON)
        {
            if (player1Score < player1ScoreEndLevel)
            {
                if (player1ScoreEndLevel - player1Score > 100) player1Score+=23;
                else player1Score++;

                textScore1Data.text = player1Score.ToString();
            }
            if (player2Score < player2ScoreEndLevel)
            {
                if (player2ScoreEndLevel - player2Score > 100) player2Score += 23;
                else player2Score++;
                textScore2Data.text = player2Score.ToString();
            }


            if (Vector2.Distance(transform.position, eyePositonAboveGoal) > 4)
            {
                Vector3 lookPos = goalBlock.transform.position - transform.position;
                Quaternion lookRot = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, 1.7f * Time.deltaTime);

                transform.position += transform.forward * Time.deltaTime * 5;
                if (transform.position.y < 2) transform.position = new Vector3(transform.position.x, 2, transform.position.z);
            }
            else
            {
                if (!cameraAudio.isPlaying)
                {  
                    curLevel++;
                    if (curLevel >= gameMapList.Length) curLevel = 1;

                    spawnBoard(curLevel);
                }
            }
        }
    }



    private void updateDustBunny(GameObject dustBunny, float phase)
    {
        if (dustBunny.activeSelf)
        {
            float dustBunnyY = dustBunny.transform.position.y - Time.deltaTime * 3;
            if (dustBunnyY < 1)
            {
                dustBunnyY = 1;
                float scale = dustBunny.transform.localScale.x;
                scale = scale - Time.deltaTime / 10;
                if (scale < 0)
                {
                    dustBunny.SetActive(false);
                }
                else
                {
                    dustBunny.transform.localScale = new Vector3(scale, scale, scale);
                }
            }
            float dustBunnyX = (dustBunnyY - 1) / 3 * Mathf.Cos(Time.time+phase) + goalBlock.transform.position.x;
            float dustBunnyZ = (dustBunnyY - 1) / 3 * Mathf.Sin(Time.time + phase) + goalBlock.transform.position.z;
            dustBunny.transform.position = new Vector3(dustBunnyX, dustBunnyY, dustBunnyZ);
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

        bool moved = script.updateLocation(x, z);
        if (moved) grid[gridX, gridZ].playAudio();
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

        crateClone.GetComponent<CrateScript>().spawnAnimation(cameraAudio, player);
    }

    public void setGameState(GameState state)
    {
        if (gameState == state) return;

        gameState = state;
        if (gameState==GameState.WON)
        {
            
            cameraAudio.PlayOneShot(harpEndLevel[curLevel % harpEndLevel.Length], 1.0f);
            textLevelName.GetComponent<UnityEngine.UI.Text>().text = gameMap.getName();

            textScore1Data.text = player1Score.ToString();
            textScore2Data.text = player2Score.ToString();

            player1ScoreEndLevel = player1Score + playerScript1.getLevelScore(gameMap.getPar());
            player2ScoreEndLevel = player2Score + playerScript2.getLevelScore(gameMap.getPar());

            Debug.Log("Frames/sec=" + frameCount / timeOfLevel + ",    score: " + player1ScoreEndLevel + ", " + player2ScoreEndLevel);


            textLevelName.SetActive(true);
            textScore1.SetActive(true);
            textScore2.SetActive(true);
        }
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
            playerScript1.spawnCrate(true);
            playerScript2.spawnCrate(true);
            //playerScript1.hit();
            return false;
        }
        if ((x == playerScript2.getGridX()) && (z == playerScript2.getGridZ()))
        {
            playerScript1.spawnCrate(true);
            playerScript2.spawnCrate(true);
            //playerScript2.hit();
            return false;
        }

        if (type == Element.CRATE && smashCrate)
        {
            grid[x, z].smashCrate();
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
        if (grid[x, z].isDoorDown()) return true;


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
 