using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background_DLA_Script : Background_AbstractScript
{
    //DLA: Diffusion Limited Aggregation
    //   The total number of moving points == POINT_COUNT and these are stored in pointList.
    //   Elements 0 through STAR_POINTS-1 of pointList are "stars". These are points that
    //      1) Are rendered every frame.
    //      2) 95% of the timesteps, do not move but do twinkle.
    //      3) 5% of the timesteps, move in a random walk.
    //      4) Crystalize if next to a crystal point in 8 directions.
    //      5) After forming a crystal, respawn in a random, empty location.
    //
    //    Elements STAR_POINTS through the end of pointList:
    //      1) Are never rendered.
    //      2) Are assigned an random attractor point when spawned.
    //      3) Every timestep move directly toward that attractor point.
    //      4) Crystalize if next to a crystal point in 8 directions.
    //      5) After forming a crystal, respawn in a random, empty location with a new random attractor.
    private static int POINT_COUNT = 3000;
    private static int STAR_POINTS = 1000;   
    private Particle[] pointList = new Particle[POINT_COUNT];


    private static int ATTRACTOR_COUNT = 13;
    private Particle[] attractorList = new Particle[ATTRACTOR_COUNT];

    private int crystalCount;

    private Color[,] palette =
    {
        //Palette from Paul Cezanne's La Montagne Saint Victoire Barnes
        {
           new Color(0.306f, 0.376f, 0.275f),
           new Color(0.404f, 0.549f, 0.427f),
           new Color(0.804f, 0.686f, 0.451f),
           new Color(0.667f, 0.412f, 0.341f),
           new Color(0.737f, 0.522f, 0.208f),
           new Color(0.820f, 0.741f, 0.612f),
           new Color(0.686f, 0.749f, 0.710f)
        },

        //Palette from Vincent Van Gogh's Starry Night
        {
           new Color(0.082f, 0.235f, 0.600f),
           new Color(0.231f, 0.400f, 0.808f),
           new Color(0.290f, 0.498f, 0.757f),
           new Color(0.573f, 0.808f, 0.941f),
           new Color(0.835f, 0.914f, 0.867f),
           new Color(1.000f, 0.820f, 0.310f),
           new Color(0.898f, 0.686f, 0.271f),
        }
    };
    private int paletteSize, paletteCount;

    private int secPerColor;
    private float totalSec;
    private int paletteIdx;


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

        paletteCount = palette.GetLength(0);
        paletteSize = palette.GetLength(1);

        return texture;
    }


    override public void clear(int level)
    {
        //It is such stupid syntax that reading a value changes the value.
        //  If C# were not stupid, this would be a function call: Random.value()
        //Debug.Log(Random.value + ", " + Random.value + ", " + Random.value);
        totalSec = 0;
        secPerColor = Random.Range(20, 50);
        paletteIdx = level % paletteCount;

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
            attractorList[i].spawn(texture, palette[paletteIdx,0]);

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

                int colorIdx = (((int)totalSec) / secPerColor) % paletteSize;
                texture.SetPixel(x, y, palette[paletteIdx,colorIdx]);
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
                    //By erasing and drawing each frame, even when the 
                    //   point does not move, it gives a twinkling effect.
                    texture.SetPixel(x, y, Color.black);

                    if (Random.value < 0.05f)
                    {
                        int r = Random.Range(0, 3);
                        if (r == 0) x++;
                        else if (r == 1) x--;
                        else if (r == 2) y++;
                        else y--;

                        pointList[i].x = x;
                        pointList[i].y = y;
                    }
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
