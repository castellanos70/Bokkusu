﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap
{
    private int width, height;
    private CameraScript.Element [,] map;
    private int leastMoves = 100;
    private float fastestTime = 600; //seconds
    private string levelName;
    private string player1Name = "Morgan";
    private string player2Name = "";



    //=======================================================================================================================
    //=======================================================================================================================
    public GameMap(string mapString)
    {
		string[] lines = mapString.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

		height = lines.Length-1;
		width = 0;

		for (int i = 0; i < height; i++)
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

		for (int j = 0; j < height; j++)
        {
			string row = lines[j];
            //Debug.Log(row+", j=" + j + "  idx="+ ((height - 1) - j));
            for (int i = 0; i < row.Length; i++)
            {
                //Debug.Log(i + "=i" + ",    j="+j + "   width: " + width + ", height: " + height);
                map[i, (height - 1)- j] = CameraScript.getElement(row[i]);
			}
		}
        //Debug.Log("name: " + lines[height]);
        //Debug.Log("par: " + lines[height+1]);

        levelName = lines[height].Substring(1, lines[height].Length-1);
       
        //Debug.Log("Level: " + levelName);
    }


    //=======================================================================================================================
    //=======================================================================================================================
    public CameraScript.Element[,] getMap() { return map; }


    //=======================================================================================================================
    //=======================================================================================================================
    public string getLevelName() { return levelName; }


    //=======================================================================================================================
    //=======================================================================================================================
    public string getPlayerNames()
    { if (player1Name.Length > 0 && player2Name.Length>0)
      {
            return player1Name + " & " + player2Name;
      }
        return player1Name + player2Name;
    }


    //=======================================================================================================================
    //=======================================================================================================================
    public string getPlayerName(int playerNum)
    {
        if (playerNum == 1) return player1Name;
        return player2Name;
    }


    //=======================================================================================================================
    // setPlayerName(int playerNum, string str)
    //
    // Dear Robin, add fuck replacer here.
    //=======================================================================================================================
    private void setPlayerName(int playerNum, string str)
    {
        str = str.Trim();
        if (playerNum == 1) player1Name = str;
        else player2Name = str;
    }


    //=======================================================================================================================
    //=======================================================================================================================
    public int getLeastMoves() { return leastMoves; }


    //=======================================================================================================================
    //=======================================================================================================================
    public float getFastestTime() { return fastestTime; }



    //=======================================================================================================================
    //=======================================================================================================================
    public void setLeader(int score, float seconds, string name1, string name2)
    {
        leastMoves = score;
        fastestTime = seconds;
        setPlayerName(1, name1);
        setPlayerName(2, name2);
    }
}
