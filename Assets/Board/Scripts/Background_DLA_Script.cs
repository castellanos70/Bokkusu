using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background_DLA_Script : Background_AbstractScript
{
    
    private static int POINT_COUNT = 1000;
    private static int ATTRACTOR_COUNT = 50;
    private Particle[] pointList = new Particle[POINT_COUNT];

    private Particle[] attractorList = new Particle[ATTRACTOR_COUNT];

    //hexColor = { 0xa8a8a8, 0xc0c090, 0x789078, 0xc09048, 0x603000};


    override public Texture2D create()
    {
        // Create a texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        clear();

        for (int i = 0; i < ATTRACTOR_COUNT; i++)
        {
            attractorList[i] = new Particle(texture, Color.green);
            
        }

        for (int i = 0; i < POINT_COUNT; i++)
        {
            pointList[i] = new Particle(texture, Color.white);
            int goalIdx = Random.Range(0, ATTRACTOR_COUNT - 1);
            pointList[i].goalx = attractorList[goalIdx].x;
            pointList[i].goaly = attractorList[goalIdx].y;
        }
        

        // Apply all SetPixel calls
        texture.Apply();
        return texture;
    }


    private void clear()
    {
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                if (x==0 || y == 0 || x == textureSize-1 || y == textureSize-1)
                {
                    texture.SetPixel(x, y, Color.red);
                }
                else texture.SetPixel(x, y, Color.black);
            }
        }
    }

    override public void next()
    {
        for (int i = 0; i < POINT_COUNT; i++)
        {
            int x = pointList[i].x;
            int y = pointList[i].y;
            if (pointList[i].x < 0) continue;
            texture.SetPixel(x, y, Color.black);
        }

        for (int i = 0; i < POINT_COUNT; i++)
        {
            int x = pointList[i].x;
            int y = pointList[i].y;
            if (pointList[i].x < 0) continue;

            bool crystalize = false;
            if (isCrystal(x - 1, y)) crystalize = true;
            else if (isCrystal(x + 1, y)) crystalize = true;
            else if (isCrystal(x, y-1)) crystalize = true;
            else if (isCrystal(x, y+1)) crystalize = true;

            else if (isCrystal(x-1, y - 1)) crystalize = true;
            else if (isCrystal(x - 1, y + 1)) crystalize = true;
            else if (isCrystal(x + 1, y - 1)) crystalize = true;
            else if (isCrystal(x + 1, y + 1)) crystalize = true;

            if (crystalize)
            {
                texture.SetPixel(x, y, Color.red);
                pointList[i].spawn(texture, Color.white);
            }
            else
            {
                if (Random.value < 0.2f)
                {
                    if (Random.value < 0.2f)
                    {
                        int r = Random.Range(0, 3);
                        if (r == 0) x++;
                        else if (r == 1) x--;
                        else if (r == 2) y++;
                        else y--;
                    }
                    else
                    {
                        if (Random.value > 0.5f)
                        {
                            if (x < pointList[i].goalx) x++;
                            else if (x > pointList[i].goalx) x--;
                        }
                        else
                        {
                            if (y < pointList[i].goaly) y++;
                            else if (y > pointList[i].goaly) y--;
                        }
                    }

                    pointList[i].x = x;
                    pointList[i].y = y;
                }
                texture.SetPixel(x, y, Color.white);
            }
        }
        texture.Apply();
    }


    private bool isCrystal(int x, int y)
    {
        Color color = texture.GetPixel(x, y);
        if (color.Equals(Color.black)) return false;
        if (color.Equals(Color.white)) return false;
        return true;
    }

    class Particle
    {
        public int x = -1;
        public int y = -1;

        public int goalx, goaly;
        public Particle(Texture2D texture, Color color)
        {
            spawn(texture, color);
        }

        public void spawn(Texture2D texture, Color color)
        {
            int n = 0;
            while (true)
            {
                n++;
                x = Random.Range(2, textureSize-3);
                y = Random.Range(2, textureSize - 3);
                if (texture.GetPixel(x, y).Equals(Color.black)) break;

                if (n > 100) break;

            }

            if (x > 0)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }
}
