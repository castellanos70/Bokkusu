using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap{
	public int width, height;
	public int[] moves = {0, 0};
	public Cell[,] grid;

    public GameMap(int width, int height, int[] moves){
		this.width = width;
		this.height = height;
		this.moves = moves;
		grid = new Cell[width,height];
	}

	public GameMap(Cell[,] grid){
		width = grid.GetLength (0);
		height = grid.GetLength (1);
		this.grid = grid;
	}

	public GameMap(string mapString){
		string[] lines = mapString.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

		string[] movesString = (lines[0]).Split (new string[] { ", " }, System.StringSplitOptions.None);

		moves[0] = int.Parse(movesString[0]);
		moves [1] = int.Parse (movesString [1]);
		height = lines.Length-1;
		width = 0;

		for (int i = 1; i < lines.Length; i++){
			if (lines[i].Length > width) width = lines[i].Length;
		}

		grid = new Cell[width, height];
		Debug.Log ("width: " + width + ", height: " + height); 

		for (int i = 1; i < lines.Length; i++){
			string row = lines[i];
            //Debug.Log(row+", idx="+ ((lines.Length - 1) - i));
            for (int j = 0; j < row.Length; j++){
                CameraScript.Element element = CameraScript.getElement(row[j]);
                if (element == CameraScript.Element.PLAYER1
                    || element == CameraScript.Element.PLAYER2)
                {
                    grid[j, (lines.Length - 1) - i] = new Cell(CameraScript.Element.FLOOR,
                                                               CameraScript.getElement(row[j]));
                }
                else grid [j, (lines.Length-1)-i] = new Cell(CameraScript.getElement(row[j]));
			}
		}
	}

	public CameraScript.Element[,] getElementMap(){
		CameraScript.Element[,] elements = new CameraScript.Element [width, height];
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				//TODO
			}
		}

		return elements;
	}
}
