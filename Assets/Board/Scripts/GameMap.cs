using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap
{
    private int width, height;
    private int maxMovesPlayer1 = 0;
    private int maxMovesPlayer2 = 0;
    private CameraScript.Element [,] map;

    public GameMap(string mapString)
    {
        
		string[] lines = mapString.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

		string[] movesString = (lines[0]).Split (new string[] { ", " }, System.StringSplitOptions.None);


        maxMovesPlayer1 = int.Parse(movesString[0]);
        maxMovesPlayer2 = int.Parse (movesString [1]);
		height = lines.Length-1;
		width = 0;

		for (int i = 1; i < lines.Length; i++)
        {
			if (lines[i].Length > width) width = lines[i].Length;
		}

        map = new CameraScript.Element[width, height];

		//fill wih empty
		for (var i = 0; i < width; i++)
        {
			for (var j = 0; j < height; j++)
            {
                map[i, j] = CameraScript.Element.NOTHING;
			}
		}

		//Debug.Log ("width: " + width + ", height: " + height); 

		for (int j = 1; j < lines.Length; j++)
        {
			string row = lines[j];
            //Debug.Log(row+", idx="+ ((lines.Length - 1) - j));
            for (int i = 0; i < row.Length; i++)
            {
                //Debug.Log(i + "=i" + ",    j="+j + "   width: " + width + ", height: " + height);
                map[i, (lines.Length - 1) - j] = CameraScript.getElement(row[i]);
			}
		}
	}

    public CameraScript.Element[,] getMap() { return map; }

    public int getMaxMoves(CameraScript.Element player)
    {
        if (player == CameraScript.Element.PLAYER1) return maxMovesPlayer1;
        if (player == CameraScript.Element.PLAYER2) return maxMovesPlayer2;
        return 0;
    }
}
