using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap
{
    private int width, height;
    private CameraScript.Element [,] map;
    private int leastMoves = 100;
    private float fastestTime = 600; //seconds
    private string levelName;
    private string player1Name;
    private string player2Name;

    private bool rotated;

    public GameMap(string mapString)
    {
		string[] lines = mapString.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

		height = lines.Length-1;
		width = 0;

		for (int i = 0; i < height; i++)
        {
			if (lines[i].Length > width) width = lines[i].Length;
		}

        generateMap(width, height, lines, Random.Range(0, 2) == 1, Random.Range(0, 4));

        levelName = lines[height].Substring(1, lines[height].Length-1);
    }

    public CameraScript.Element[,] getMap() { return map; }
    public string getName() { return levelName; }
    public int getLeastMoves() { return leastMoves; }
    public float getFastestTime() { return fastestTime; }
    public bool isRotated() { return rotated; }

    public void setLeader(int score, float seconds)
    {
        leastMoves = score;
        fastestTime = seconds;
    }

    private void generateMap(int width, int height, string[] lines, bool rotate, int mirror)
    {
        if (rotate)
        {
            rotated = true;

            map = new CameraScript.Element[height, width];

            //fill wih empty
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    map[j, i] = CameraScript.Element.NOTHING;
                }
            }

            for (int j = 0; j < height; j++)
            {
                string row = lines[j];
                for (int i = 0; i < row.Length; i++)
                {
                    if (mirror == 0) map[j, i] = CameraScript.getElement(row[i]);
                    else if (mirror == 1) map[height - 1 - j, i] = CameraScript.getElement(row[i]);
                    else if (mirror == 2) map[j, row.Length - 1 - i] = CameraScript.getElement(row[i]);
                    else if (mirror == 3) map[height - 1 - j, row.Length - 1 - i] = CameraScript.getElement(row[i]);
                }
            }
        }
        else
        {
            rotated = false;

            map = new CameraScript.Element[width, height];

            //fill wih empty
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    map[i, j] = CameraScript.Element.NOTHING;
                }
            }

            for (int j = 0; j < height; j++)
            {
                string row = lines[j];
                for (int i = 0; i < row.Length; i++)
                {
                    if (mirror == 0) map[i, j] = CameraScript.getElement(row[i]);
                    else if (mirror == 1) map[row.Length - 1 - i, j] = CameraScript.getElement(row[i]);
                    else if (mirror == 2) map[i, height - 1 - j] = CameraScript.getElement(row[i]);
                    else if (mirror == 3) map[row.Length - 1 - i, height - 1 - j] = CameraScript.getElement(row[i]);
                }
            }
        }
    }
}
