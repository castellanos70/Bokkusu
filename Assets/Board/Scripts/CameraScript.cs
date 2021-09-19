using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public AudioClip[] harpEndLevel;
    public AudioSource cameraAudio;

    public GameObject boardBlock;
    public GameObject crateBlock;
    public GameObject player1, player2;

    public GameObject textCurrentScore, textHighScore, textPlayerGoals1, textPlayerGoals2;
    public ParticleSystem textEffectHighScore;

    public GameObject goalBlock;
    public GameObject backgroundPlane;

    public enum Element { FLOOR, WALL, GOAL, CRATE, PLAYER1, PLAYER2, DOOR_A, DOOR_B, NOTHING };
    public static char[] ELEMENT_ASCII = { '.', '#', '=', '&', '1', '2', 'A', 'B', ' ' };


    //based on the current camera FOV 
    private static float fullWidth = 32;
	private static float fullHeight = 14;
    private int gridWidth, gridHeight;

    private float doorToggleSeconds;

	//private AudioSource audio;
	private AudioClip[] harpAudio;
	private AudioClip[] pentAudio; //pantatonic scale
    

    private static int MAX_AUDIO_TRACKS = 32;
    private AudioSource[] cellAudioList = new AudioSource[MAX_AUDIO_TRACKS];
    private int cellAudioIdx;

    
    private UnityEngine.UI.Text textNameData, textScoreData, textPlayer1Data, textPlayer2Data;


    private int frameCount;
    private float timeOfLevel, playTimeOfLevel;
    private bool firstMoveWasMade;


    private Material wallMat;
    private NoiseTexture wallScript;
    private int wallTextureSize = 128;
    private float wallMorphScale = 0.05f;

    private Material floorMat;
    private NoiseTexture floorScript;
    private int floorTextureSize = 128;
    private float floorMorphScale = 0.1f;
   

    
    private Background_AbstractScript backgroundScript;

    private Kaleidoscope doorKaleidoscope;
    private Material doorMat1, doorMat2;
    private int doorTextureSize = 256;


    public enum GameState { INTRO, INITIALIZING, PLAYING, WON, ENTER_HIGHSCORE, GAME_OVER};
    private GameState gameState;
    private GameMap gameMap;
    private Element[,] startMap;

    


    private static Element[] elementValues;
    private GameMap[] gameMapList;
    private Cell[,] grid;
    private int curLevel = 0;
    
    private PlayerScript playerScript1, playerScript2;

    private Vector3 eyePosition1, eyePosition2, eyePosition3, eyePositonAboveGoal;
    private Quaternion eyeRotation1, eyeRotation2, eyeRotation3;
    private int eyeMovingTo;
    private float eyeSpeed = 0.09f;


  


    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();
        elementValues = (Element[])System.Enum.GetValues(typeof(Element));

        textPlayer1Data = textPlayerGoals1.GetComponent<UnityEngine.UI.Text>();
        textPlayer2Data = textPlayerGoals2.GetComponent<UnityEngine.UI.Text>();
        textScoreData = textHighScore.GetComponent<UnityEngine.UI.Text>();
        textNameData  = textCurrentScore.GetComponent<UnityEngine.UI.Text>();


        gameMapList = MapLoader.loadAllMaps();


        HighScoreIO.loadHighScores(gameMapList);


        //audio stuff
		harpAudio = Resources.LoadAll<AudioClip>("Audio/harpsichord");
		int[] pentatonic = { 0, 2, 4, 7, 9 };

		pentAudio = new AudioClip[(int)(harpAudio.Length*(5f/12f))];

		//Debug.Log("pent: " + pentAudio.Length);

		int pentLength = 0;
        for (int i = 0; i < harpAudio.Length; i++)
        {
            bool isPent = false;
            for (var j = 0; j < 5; j++)
            {
                if (i % 12 == pentatonic[j])
                {
                    isPent = true; break;
                }
            }
            if (isPent && pentLength < pentAudio.Length)
            {
                pentAudio[pentLength] = harpAudio[i];
                pentLength++;
            }
        }
        //end audio stuff

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
        Cursor.visible = false;

        newGame();
    }


    private void newGame()
    {
        backgroundScript.clear();
        spawnBoard(curLevel);
    }


    private void spawnBoard(int level)
    {
        gameState = GameState.INITIALIZING;

        textEffectHighScore.Stop();
        textEffectHighScore.Clear();

        textHighScore.SetActive(false);
        textPlayerGoals1.SetActive(false);
        textPlayerGoals2.SetActive(false);


        doorToggleSeconds = -1f;
        frameCount = 0;
        timeOfLevel = 0;
        playTimeOfLevel = 0;
        firstMoveWasMade = false;
        cellAudioIdx = 0;

        curLevel = level;
        destroyOldBoard();

        gameMap = gameMapList[level];
        startMap = gameMap.getMap();
        gridWidth = startMap.GetLength(0);
        gridHeight = startMap.GetLength(1);

        int numCells = 0;
        textNameData.text = gameMap.getLevelName();
        textScoreData.text = gameMap.getLeastMoves() + " moves in " + gameMap.getFastestTime() + " sec by " +
            gameMap.getPlayerNames(); ;
        textCurrentScore.SetActive(true);
        textHighScore.SetActive(true);

        grid = new Cell[gridWidth, gridHeight];
        bool foundGoal = false;

        // Spawn board blocks
        //Debug.Log("Spawn Board(" + level + "): " + grid.GetLength(0) + "(" + gridWidth + ") x " + grid.GetLength(1) + "(" + gridHeight + ")");
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (startMap[x, z] == Element.NOTHING) continue;
                //Debug.Log("startMap[" + x + ", " + z + "]=" + startMap[x, z]);

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

				
                //Debug.Log("harpAudio.Length=" + harpAudio.Length);

                if (curLevel == 0)
                {
                    if (x == 1 && z == 5) grid[x, z].setAudioClip(harpAudio[37]);

                    else if (x == 2 && z == 4) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 2 && z == 5) grid[x, z].setAudioClip(harpAudio[35]);

                    else if (x == 3 && z == 3) grid[x, z].setAudioClip(harpAudio[30]);
                    else if (x == 3 && z == 4) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 3 && z == 5) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 3 && z == 6) grid[x, z].setAudioClip(harpAudio[35]);
                    else if (x == 3 && z == 7) grid[x, z].setAudioClip(harpAudio[37]);

                    else if (x == 4 && z == 2) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 4 && z == 3) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 4 && z == 4) grid[x, z].setAudioClip(harpAudio[30]);
                    else if (x == 4 && z == 5) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 4 && z == 6) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 4 && z == 7) grid[x, z].setAudioClip(harpAudio[35]);
                    else if (x == 4 && z == 8) grid[x, z].setAudioClip(harpAudio[33]);

                    else if (x == 5 && z == 1) grid[x, z].setAudioClip(harpAudio[37]);
                    else if (x == 5 && z == 2) grid[x, z].setAudioClip(harpAudio[35]);
                    else if (x == 5 && z == 3) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 5 && z == 4) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 5 && z == 6) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 5 && z == 7) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 5 && z == 8) grid[x, z].setAudioClip(harpAudio[35]);
                    else if (x == 5 && z == 9) grid[x, z].setAudioClip(harpAudio[37]);

                    else if (x == 6 && z == 2) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 6 && z == 3) grid[x, z].setAudioClip(harpAudio[35]);
                    else if (x == 6 && z == 4) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 6 && z == 5) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 6 && z == 6) grid[x, z].setAudioClip(harpAudio[30]);
                    else if (x == 6 && z == 7) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 6 && z == 8) grid[x, z].setAudioClip(harpAudio[33]);

                    else if (x == 7 && z == 3) grid[x, z].setAudioClip(harpAudio[37]);
                    else if (x == 7 && z == 4) grid[x, z].setAudioClip(harpAudio[35]);
                    else if (x == 7 && z == 5) grid[x, z].setAudioClip(harpAudio[33]);
                    else if (x == 7 && z == 6) grid[x, z].setAudioClip(harpAudio[32]);
                    else if (x == 7 && z == 7) grid[x, z].setAudioClip(harpAudio[35]);

                    else if (x == 8 && z == 5) grid[x, z].setAudioClip(harpAudio[35]);
                    else if (x == 8 && z == 6) grid[x, z].setAudioClip(harpAudio[33]);

                    else if (x == 9 && z == 5) grid[x, z].setAudioClip(harpAudio[37]);

                }
                else
                {
                    int audioIndex = (int)(((40 - y) / 40.0f) * pentAudio.Length);
                    grid[x, z].setAudioClip(pentAudio[audioIndex]);
                }

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
        if (firstMoveWasMade) playTimeOfLevel += Time.deltaTime;

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
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
                        cellAudioList[cellAudioIdx] = grid[x, z].playAudio(0.4f);
                        cellAudioIdx = (cellAudioIdx + 1 ) % MAX_AUDIO_TRACKS;
                    }
                    else fallingDone = false;
                }
            }
			if (fallingDone)
            {
				gameState = GameState.PLAYING;
                textCurrentScore.SetActive(false);
                textHighScore.SetActive(false);
                playerScript1.startLevel();
                playerScript2.startLevel();
            }
        }

        else if (gameState == GameState.PLAYING)
        {


            movePlayer(player1, playerScript1, playerScript2);
            movePlayer(player2, playerScript2, playerScript1);

            
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
            
            //Vector3 boradCenter = new Vector3(gridWidth / 2.0f - .5f, 0, gridHeight / 2.0f - .5f);
            //
            //Vector3 eyePositionFront = new Vector3(gridWidth / 2.0f - .5f, 45, -3 * gridHeight / 2.0f);
            //transform.position = eyePositionFront;
            //transform.LookAt(boradCenter, Vector3.up);
    




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
        }

        else if (gameState == GameState.WON || gameState == GameState.ENTER_HIGHSCORE)
        {
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
                if (gameState == GameState.WON && !cameraAudio.isPlaying)
                {
                    curLevel = (curLevel + 1) % gameMapList.Length; 
                    spawnBoard(curLevel);
                }
            }
        }
    }




    private void movePlayer(GameObject player, PlayerScript script, PlayerScript scriptOther)
    {
        if (!script.isMoving()) return;
        if (!firstMoveWasMade) firstMoveWasMade = true;

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
            //float speed = script.getSpeedMagnitude();
            bool smashCrate = false;
            //if (speed >= CrateScript.getStrength()) smashCrate = true;
            if (!enterIfPossible(script, scriptOther, toX, toZ, smashCrate, false))
            {
                return;
            }
        }
        //Debug.Log("CameraScript.movePlayer(): speed: ("+ script.getSpeedX() + ", "+ script.getSpeedZ()+")");

        bool moved = script.updateLocation(x, z);
        if (moved) grid[gridX, gridZ].playAudio(1.0f);
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

            playTimeOfLevel = Mathf.Round(playTimeOfLevel*10)/10.0f;
            string timeStr = playTimeOfLevel.ToString("F1");

            int levelMoveCount = playerScript1.getMoveCount() + playerScript2.getMoveCount();
            textNameData.text = gameMap.getLevelName() + " in " + levelMoveCount + " moves and " + timeStr + " seconds!";
            
            if ((levelMoveCount < gameMap.getLeastMoves()) || (levelMoveCount == gameMap.getLeastMoves() && playTimeOfLevel < gameMap.getFastestTime()))
            {
                gameMap.setLeader(levelMoveCount, playTimeOfLevel, "Joel", "");
                //HighScoreIO.writeHighScores(gameMapList);
                textEffectHighScore.transform.position = new Vector3(goalBlock.transform.position.x, 1.5f, goalBlock.transform.position.z);
                textEffectHighScore.Play();
                playerScript1.showEditBoxForPlayerName();
                playerScript2.showEditBoxForPlayerName();
            }
            else
            {
                textScoreData.text = "Leader: " + gameMap.getLeastMoves() + " moves in " + gameMap.getFastestTime() + " sec.";
                textHighScore.SetActive(true);
            }


            textPlayer1Data.text = "Goals " + playerScript1.getGoalCount();
            textPlayer2Data.text = "Goals " + playerScript2.getGoalCount();


            Debug.Log("Frames/sec=" + frameCount / timeOfLevel);

            textCurrentScore.SetActive(true);
            textPlayerGoals1.SetActive(true);
            textPlayerGoals2.SetActive(true);
        }
    }

    public GameState getGameState()
    {
        return gameState;
    }



    public bool enterIfPossible(Element player, int x, int z, bool smashCrate, bool smashPlayer)
    {
       if (player == Element.PLAYER1) return enterIfPossible(playerScript1, playerScript2, x, z, smashCrate, smashPlayer);
       return enterIfPossible(playerScript2, playerScript1, x, z, smashCrate, smashPlayer);
    }

    public bool enterIfPossible(PlayerScript player, PlayerScript other, int x, int z, bool smashCrate, bool smashPlayer)
    {
        if (isEnterable(x, z)) return true;

        Element type = grid[x, z].getType();
        if ((x == other.getGridX()) && (z == other.getGridZ()))
        {
            if (smashPlayer && other.getSpeedMagnitude() == 0)
            {
                other.spawnCrate(false);
            }
            else
            {
                player.hit(true);
                other.hit(true);
                return false;
            }
        }
        

        if (type == Element.CRATE && smashCrate)
        {
            grid[x, z].smashCrate();
            return true;
        }

        player.hit(false);
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
 