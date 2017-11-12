using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class HighScoreIO : MonoBehaviour
{

    public static bool loadHighScores(GameMap[] gameMapList)
    {
        string line;
        // Create a new StreamReader, tell it which file to read and what encoding the file
        // was saved as
        Debug.Log("loadHighScores()");
        StreamReader myReader = new StreamReader("Assets/Resources/HighScores.txt", Encoding.Default);
        using (myReader)
        {
            line = myReader.ReadLine(); //First line is just header
            line = myReader.ReadLine();
            Debug.Log(line);
            while (line != null)
            {
                setHighScope(gameMapList, line);
                

                line = myReader.ReadLine();
            }
            myReader.Close();
            return true;
        }
    }


    public static bool writeHighScores(GameMap[] gameMapList)
    {
        // Create a new StreamReader, tell it which file to read and what encoding the file
        // was saved as
        Debug.Log("writeHighScores()");
        FileStream file = File.Open("Assets/Resources/HighScores.txt", FileMode.Create, FileAccess.Write);

        StreamWriter myWriter = new StreamWriter(file);
        
        using (myWriter)
        {
            myWriter.WriteLine("LevelName,Moves,Time");
            for (int i = 0; i < gameMapList.Length; i++)
            {
                GameMap map = gameMapList[i];
                string line = map.getName() + "," + map.getLeastMoves() + "," + map.getFastestTime();
                myWriter.WriteLine(line);

            }
            myWriter.Close();
            return true;
        }
    }



    private static void setHighScope(GameMap[] gameMapList, string line)
    {
        string[] fields = line.Split(',');
        Debug.Log("LevelName=" + fields[0] + ", Moves=" + fields[1] + ", Time=" + fields[2]);
        for (int i=0; i<gameMapList.Length; i++)
        {
            GameMap map = gameMapList[i];
            if (map.getName().Equals(fields[0]))
            {
               
                int moveCount;
                float levelTime;
                int.TryParse(fields[1], out moveCount);
                float.TryParse(fields[2], out levelTime);

                map.setLeader(moveCount, levelTime);
            }

        }

    }
}