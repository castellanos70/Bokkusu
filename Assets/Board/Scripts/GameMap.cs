﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap{
	public int width, height;
	public int moves;
	public CameraScript.Element[,] grid;

	public GameMap(int width, int height, int moves){
		this.width = width;
		this.height = height;
		this.moves = moves;
		grid = new CameraScript.Element[width,height];
	}

	public GameMap(CameraScript.Element[,] grid){
		width = grid.GetLength (0);
		height = grid.GetLength (1);
		this.grid = grid;
	}

	public GameMap(string mapString){
		string[] lines = mapString.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

		moves = int.Parse(lines[0]);
		height = lines.Length-2;
		width = 0;

		for (int i = 1; i < lines.Length; i++){
			if (lines[i].Length > width) width = lines[i].Length;
		}

		grid = new CameraScript.Element[width, height];
		//Debug.Log ("width: " + width + ", height: " + height); 

		for (int i = 1; i < lines.Length; i++){
			string row = lines[i];
            Debug.Log(row+", idx="+ ((lines.Length - 1) - i));
            for (int j = 0; j < row.Length; j++){
				int val = System.Array.IndexOf (MapLoader.tempMapping, row[j]);
                grid [j, (lines.Length-2)-i] = CameraScript.getElement(val);
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
