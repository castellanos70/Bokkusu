using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background_DLA_Script : Background_AbstractScript
{
    
    private static int POINT_COUNT = 1000;
    private static int ATTRACTOR_COUNT = 13;
    private Particle[] pointList = new Particle[POINT_COUNT];

    private Particle[] attractorList = new Particle[ATTRACTOR_COUNT];

    private int crystalCount;

    private Color[] palette =
    {
        new Color(0.404f,0.549f, 0.427f),
        new Color(0.804f,0.686f, 0.451f),
        new Color(0.667f,0.412f, 0.341f),
        new Color(0.737f,0.522f, 0.208f),
        new Color(0.820f,0.741f, 0.612f),
        new Color(0.686f,0.749f, 0.710f)
    };


    override public Texture2D create()
    {
        // Create a texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        clear();

        for (int i = 0; i < ATTRACTOR_COUNT; i++)
        {
            attractorList[i] = new Particle(texture, palette[0]);
            
        }

        for (int i = 0; i < POINT_COUNT; i++)
        {
            pointList[i] = new Particle(texture, Color.white);
            pointList[i].setAttractor(attractorList);
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
        //for (int i = 0; i < POINT_COUNT; i++)
        //{
        //    int x = pointList[i].x;
        //    int y = pointList[i].y;
        //    if (pointList[i].x < 0) continue;
        //    texture.SetPixel(x, y, Color.black);
        //}

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
                crystalCount++;

                int colorIdx = (crystalCount / 2000) % palette.Length;
                texture.SetPixel(x, y, palette[colorIdx]);
                pointList[i].spawn(texture, Color.white);
                pointList[i].setAttractor(attractorList);
            }
            else
            {
                if (Random.value < 0.2f)
                {
                    texture.SetPixel(x, y, Color.black);
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
                        if (i % 2 == 0)
                        {
                            if (x < pointList[i].goalx) x++;
                            else if (x > pointList[i].goalx) x--;
                            else if (y < pointList[i].goaly) y++;
                            else if (y > pointList[i].goaly) y--;
                        }
                        else
                        {
                            if (y < pointList[i].goaly) y++;
                            else if (y > pointList[i].goaly) y--;
                            else if (x < pointList[i].goalx) x++;
                            else if (x > pointList[i].goalx) x--;

                        }
                    }

                    pointList[i].x = x;
                    pointList[i].y = y;
                    texture.SetPixel(x, y, Color.white);
                }
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

        public void setAttractor(Particle[] attractorList)
        {
            int goalIdx = Random.Range(0, ATTRACTOR_COUNT - 1);
            goalx = attractorList[goalIdx].x;
            goaly = attractorList[goalIdx].y;
        }
    }
}
