using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background_DLA_Script : Background_AbstractScript
{
    
    private static int POINT_COUNT = 3000;
    private static int ATTRACTOR_COUNT = 13;
    private static int STAR_POINTS = 1000;
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

    private float totalSec;


    override public Texture2D create()
    {
        // Create a texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);

        for (int i = 0; i < ATTRACTOR_COUNT; i++)
        {
            attractorList[i] = new Particle();

        }

        for (int i = 0; i < POINT_COUNT; i++)
        {
            pointList[i] = new Particle();
        }


        return texture;
    }


    override public void clear()
    {
        totalSec = 0;
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

        for (int i = 0; i < ATTRACTOR_COUNT; i++)
        {
            attractorList[i].spawn(texture, palette[0]);

        }

        for (int i = 0; i < POINT_COUNT; i++)
        {
            pointList[i].spawn(texture, Color.white);
            pointList[i].setAttractor(attractorList);
        }


        // Apply all SetPixel calls
        texture.Apply();
    }

    override public void next()
    {
        totalSec += Time.deltaTime;
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

                int colorIdx = (((int)totalSec) / 20) % palette.Length;
                texture.SetPixel(x, y, palette[colorIdx]);
                if (i < STAR_POINTS) pointList[i].spawn(texture, Color.white);
                else
                {
                    pointList[i].spawn(texture);
                    pointList[i].setAttractor(attractorList);
                }
            }
            else
            {
                if (i < STAR_POINTS)
                {
                    if (Random.value > 0.05f) continue;

                    texture.SetPixel(x, y, Color.black);

                    int r = Random.Range(0, 3);
                    if (r == 0) x++;
                    else if (r == 1) x--;
                    else if (r == 2) y++;
                    else y--;

                    pointList[i].x = x;
                    pointList[i].y = y;
                    texture.SetPixel(x, y, Color.white);

                }
                else
                {
                    if (x < pointList[i].goalx) x++;
                    else if (x > pointList[i].goalx) x--;

                    if (y < pointList[i].goaly) y++;
                    else if (y > pointList[i].goaly) y--;

                    pointList[i].x = x;
                    pointList[i].y = y;

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

        public void spawn(Texture2D texture)
        {
            int n = 0;
            while (true)
            {
                n++;
                x = Random.Range(2, textureSize - 3);
                y = Random.Range(2, textureSize - 3);
                if (texture.GetPixel(x, y).Equals(Color.black)) break;

                if (n > 100) break;
            }
        }

        public void spawn(Texture2D texture, Color color)
        {
            spawn(texture);

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
