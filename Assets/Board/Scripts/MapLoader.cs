using UnityEngine;

public static class MapLoader : object{
	                                  
	public static int[] tempMapping = {
		'.', //floor
		'#', //wall
		'=', //goal
		'1', //player1
		'2', //player2
	};

	public static GameMap[] loadAllMaps(){
		
		Object[] mapTexts = Resources.LoadAll ("Maps", typeof (TextAsset));
        if (mapTexts.Length == 0)
        {
            Debug.Log("ERROR: MapLoader.loadAllMaps(): files not found.");
        }
		GameMap[] gameMaps = new GameMap[mapTexts.Length];

		for (int i = 0; i < mapTexts.Length; i++){
			gameMaps [i] = new GameMap (((TextAsset)mapTexts[i]).text);
		}

		return gameMaps;
	}
}
