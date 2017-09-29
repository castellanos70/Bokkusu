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

    public GameObject textLevelName, textScore, textPlayer1, textPlayer2, textCheer;
    private UnityEngine.UI.Text textNameData, textScoreData, textPlayer1Data, textPlayer2Data, textCheerData;


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
   

    public GameObject goalBlock;

    public GameObject backgroundPlane;
    private Background_AbstractScript backgroundScript;

    private Kaleidoscope doorKaleidoscope;
    private Material doorMat1, doorMat2;
    private int doorTextureSize = 256;


    public enum GameState { INTRO, INITIALIZING, PLAYING, WON, GAME_OVER};
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
    private int gameScoreIdx, tmpGameScore;


    private static int[] primes = new int[] {0, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89,
    97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233,
    239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397,
    401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571,
    577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743,
    751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 919, 929, 937,
    941, 947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013 , 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069 , 1087, 1091, 1093,
    1097, 1103, 1109, 1117, 1123, 1129, 1151 , 1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223 , 1229, 1231, 1237, 1249, 1259, 1277,
    1279, 1283, 1289, 1291 , 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373 , 1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447,
    1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511 , 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583 , 1597, 1601,
    1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657 , 1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733 , 1741, 1747, 1753, 1759, 1777,
    1783, 1787, 1789, 1801, 1811 , 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889 , 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973,
    1979, 1987 , 1993, 1997, 1999, 2003, 2011, 2017, 2027, 2029, 2039, 2053 , 2063, 2069, 2081, 2083, 2087, 2089, 2099, 2111, 2113, 2129 , 2131,
    2137, 2141, 2143, 2153, 2161, 2179, 2203, 2207, 2213 , 2221, 2237, 2239, 2243, 2251, 2267, 2269, 2273, 2281, 2287 , 2293, 2297, 2309, 2311,
    2333, 2339, 2341, 2347, 2351, 2357 , 2371, 2377, 2381, 2383, 2389, 2393, 2399, 2411, 2417, 2423 , 2437, 2441, 2447, 2459, 2467, 2473, 2477,
    2503, 2521, 2531 , 2539, 2543, 2549, 2551, 2557, 2579, 2591, 2593, 2609, 2617 , 2621, 2633, 2647, 2657, 2659, 2663, 2671, 2677, 2683, 2687,
    2689, 2693, 2699, 2707, 2711, 2713, 2719, 2729, 2731, 2741 , 2749, 2753, 2767, 2777, 2789, 2791, 2797, 2801, 2803, 2819 , 2833, 2837, 2843,
    2851, 2857, 2861, 2879, 2887, 2897, 2903 , 2909, 2917, 2927, 2939, 2953, 2957, 2963, 2969, 2971, 2999 , 3001, 3011, 3019, 3023, 3037, 3041,
    3049, 3061, 3067, 3079 , 3083, 3089, 3109, 3119, 3121, 3137, 3163, 3167, 3169, 3181 , 3187, 3191, 3203, 3209, 3217, 3221, 3229, 3251, 3253, 3257,
    3259, 3271, 3299, 3301, 3307, 3313, 3319, 3323, 3329, 3331 , 3343, 3347, 3359, 3361, 3371, 3373, 3389, 3391, 3407, 3413 , 3433, 3449, 3457, 3461,
    3463, 3467, 3469, 3491, 3499, 3511 , 3517, 3527, 3529, 3533, 3539, 3541, 3547, 3557, 3559, 3571 , 3581, 3583, 3593, 3607, 3613, 3617, 3623, 3631,
    3637, 3643 , 3659, 3671, 3673, 3677, 3691, 3697, 3701, 3709, 3719, 3727 , 3733, 3739, 3761, 3767, 3769, 3779, 3793, 3797, 3803, 3821 , 3823, 3833,
    3847, 3851, 3853, 3863, 3877, 3881, 3889, 3907 , 3911, 3917, 3919, 3923, 3929, 3931, 3943, 3947, 3967, 3989 , 4001, 4003, 4007, 4013, 4019, 4021,
    4027, 4049, 4051, 4057 , 4073, 4079, 4091, 4093, 4099, 4111, 4127, 4129, 4133, 4139 , 4153, 4157, 4159, 4177, 4201, 4211, 4217, 4219, 4229, 4231,
    4241, 4243, 4253, 4259, 4261, 4271, 4273, 4283, 4289, 4297 , 4327, 4337, 4339, 4349, 4357, 4363, 4373, 4391, 4397, 4409 , 4421, 4423, 4441, 4447,
    4451, 4457, 4463, 4481, 4483, 4493 , 4507, 4513, 4517, 4519, 4523, 4547, 4549, 4561, 4567, 4583 , 4591, 4597, 4603, 4621, 4637, 4639, 4643, 4649,
    4651, 4657 , 4663, 4673, 4679, 4691, 4703, 4721, 4723, 4729, 4733, 4751, 4759, 4783, 4787, 4789, 4793, 4799, 4801, 4813, 4817, 4831 , 4861, 4871,
    4877, 4889, 4903, 4909, 4919, 4931, 4933, 4937 , 4943, 4951, 4957, 4967, 4969, 4973, 4987, 4993, 4999, 5003 , 5009, 5011, 5021, 5023, 5039, 5051,
    5059, 5077, 5081, 5087 , 5099, 5101, 5107, 5113, 5119, 5147, 5153, 5167, 5171, 5179 , 5189, 5197, 5209, 5227, 5231, 5233, 5237, 5261, 5273, 5279,
    5281, 5297, 5303, 5309, 5323, 5333, 5347, 5351, 5381, 5387 , 5393, 5399, 5407, 5413, 5417, 5419, 5431, 5437, 5441, 5443 , 5449, 5471, 5477, 5479,
    5483, 5501, 5503, 5507, 5519, 5521 , 5527, 5531, 5557, 5563, 5569, 5573, 5581, 5591, 5623, 5639 , 5641, 5647, 5651, 5653, 5657, 5659, 5669, 5683,
    5689, 5693 , 5701, 5711, 5717, 5737, 5741, 5743, 5749, 5779, 5783, 5791 , 5801, 5807, 5813, 5821, 5827, 5839, 5843, 5849, 5851, 5857 , 5861, 5867,
    5869, 5879, 5881, 5897, 5903, 5923, 5927, 5939 , 5953, 5981, 5987, 6007, 6011, 6029, 6037, 6043, 6047, 6053 , 6067, 6073, 6079, 6089, 6091, 6101,
    6113, 6121, 6131, 6133 , 6143, 6151, 6163, 6173, 6197, 6199, 6203, 6211, 6217, 6221 , 6229, 6247, 6257, 6263, 6269, 6271, 6277, 6287, 6299, 6301,
    6311, 6317, 6323, 6329, 6337, 6343, 6353, 6359, 6361, 6367 , 6373, 6379, 6389, 6397, 6421, 6427, 6449, 6451, 6469, 6473 , 6481, 6491, 6521, 6529,
    6547, 6551, 6553, 6563, 6569, 6571 , 6577, 6581, 6599, 6607, 6619, 6637, 6653, 6659, 6661, 6673 , 6679, 6689, 6691, 6701, 6703, 6709, 6719, 6733,
    6737, 6761 , 6763, 6779, 6781, 6791, 6793, 6803, 6823, 6827, 6829, 6833, 6841, 6857, 6863, 6869, 6871, 6883, 6899, 6907, 6911, 6917 , 6947, 6949,
    6959, 6961, 6967, 6971, 6977, 6983, 6991, 6997 , 7001, 7013, 7019, 7027, 7039, 7043, 7057, 7069, 7079, 7103 , 7109, 7121, 7127, 7129, 7151, 7159,
    7177, 7187, 7193, 7207, 7211, 7213, 7219, 7229, 7237, 7243, 7247, 7253, 7283, 7297 , 7307, 7309, 7321, 7331, 7333, 7349, 7351, 7369, 7393, 7411,
    7417, 7433, 7451, 7457, 7459, 7477, 7481, 7487, 7489, 7499 , 7507, 7517, 7523, 7529, 7537, 7541, 7547, 7549, 7559, 7561 , 7573, 7577, 7583, 7589,
    7591, 7603, 7607, 7621, 7639, 7643 , 7649, 7669, 7673, 7681, 7687, 7691, 7699, 7703, 7717, 7723 , 7727, 7741, 7753, 7757, 7759, 7789, 7793, 7817,
    7823, 7829 , 7841, 7853, 7867, 7873, 7877, 7879, 7883, 7901, 7907, 7919
    };


    void Awake()
    {
        playerScript1 = player1.GetComponent<PlayerScript>();
        playerScript2 = player2.GetComponent<PlayerScript>();
        elementValues = (Element[])System.Enum.GetValues(typeof(Element));

        textPlayer1Data = textPlayer1.GetComponent<UnityEngine.UI.Text>();
        textPlayer2Data = textPlayer2.GetComponent<UnityEngine.UI.Text>();
        textScoreData = textScore.GetComponent<UnityEngine.UI.Text>();
        textNameData  = textLevelName.GetComponent<UnityEngine.UI.Text>();
        textCheerData = textCheer.GetComponent<UnityEngine.UI.Text>();


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
        Cursor.visible = false;

        newGame();
    }


    private void newGame()
    {
        gameScoreIdx = 0;
        backgroundScript.clear();
        spawnBoard(curLevel);
    }


    private void spawnBoard(int level)
    {
        gameState = GameState.INITIALIZING;

        textScore.SetActive(false);
        textPlayer1.SetActive(false);
        textPlayer2.SetActive(false);
        textCheer.SetActive(false);


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
        textNameData.text = gameMap.getName();
        textScoreData.text = "Par " + gameMap.getPar();
        textLevelName.SetActive(true);
        textScore.SetActive(true);

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
        if (firstMoveWasMade) playTimeOfLevel += Time.deltaTime;

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
                        cellAudioList[cellAudioIdx] = grid[x, z].playAudio(0.4f);
                        cellAudioIdx = (cellAudioIdx + 1 ) % MAX_AUDIO_TRACKS;
                    }
                    else fallingDone = false;
                }
            }
			if (fallingDone)
            {
				gameState = GameState.PLAYING;
                textLevelName.SetActive(false);
                textScore.SetActive(false);
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

        else if (gameState == GameState.WON)
        {
            if (tmpGameScore < primes[gameScoreIdx])
            {
                tmpGameScore++;
                textScoreData.text = "Score: " + tmpGameScore.ToString();
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
                    if (curLevel >= gameMapList.Length) 
                    {
                        gameState = GameState.GAME_OVER;
                        textNameData.text = "You have completed all levels!";

                    }
                    else spawnBoard(curLevel);
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

            playTimeOfLevel = Mathf.Ceil(playTimeOfLevel);

            int levelMoveCount = playerScript1.getMoveCount() + playerScript2.getMoveCount();
            textNameData.text = gameMap.getName() + " in " + levelMoveCount + " moves and " + playTimeOfLevel + " seconds!";

            int parDiff = levelMoveCount - gameMap.getPar();
            int deltaScoreIdx = 1;

            if (parDiff < 0) deltaScoreIdx = 5 + (5 * Mathf.Abs(parDiff));
            else if (parDiff > 0) deltaScoreIdx = (2 * gameMap.getPar()) - levelMoveCount;
            else deltaScoreIdx = 5;

            double parTime = gameMap.getPar() * 1.5;
            if (playTimeOfLevel > parTime)
            {
                deltaScoreIdx -= (int)(playTimeOfLevel - parTime);
            }
            else
            {
                if (parDiff < 0)
                {
                    textCheerData.text = "*** Under Par ***";
                    textCheer.SetActive(true);
                }
                if (parDiff == 0)
                {
                    textCheerData.text = "On Par!";
                    textCheer.SetActive(true);
                }
            }
            deltaScoreIdx = Mathf.Max(1, deltaScoreIdx);

            textScoreData.text = "Score: " + primes[gameScoreIdx].ToString();
            tmpGameScore = primes[gameScoreIdx];

            gameScoreIdx += deltaScoreIdx;

            textPlayer1Data.text = "Goals " + playerScript1.getGoalCount();
            textPlayer2Data.text = "Goals " + playerScript2.getGoalCount();


            Debug.Log("Frames/sec=" + frameCount / timeOfLevel);

            textLevelName.SetActive(true);
            textScore.SetActive(true);
            textPlayer1.SetActive(true);
            textPlayer2.SetActive(true);
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
 