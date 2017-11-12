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


    private static int attractorCount;
    private Particle[] attractorList;

    private int crystalCount, maxCrystals;

    public static Color[,] palette =
    {
        //Palette from Paul Cezanne's La Montagne Saint Victoire Barnes
        {
           new Color(0.306f, 0.376f, 0.275f),
           new Color(0.349f, 0.475f, 0.369f),
           new Color(0.404f, 0.549f, 0.427f),
           new Color(0.486f, 0.541f, 0.388f),
           new Color(0.624f, 0.529f, 0.322f),
           new Color(0.678f, 0.592f, 0.322f),
           new Color(0.753f, 0.647f, 0.443f),
           new Color(0.804f, 0.686f, 0.451f)
        },

        //Palette from Vincent Van Gogh's Starry Night
        {
           new Color(0.082f, 0.235f, 0.600f),
           new Color(0.231f, 0.400f, 0.808f),
           new Color(0.357f, 0.525f, 0.902f),
           new Color(0.478f, 0.647f, 1.000f),
           new Color(0.827f, 0.812f, 0.769f),
           new Color(0.502f, 0.788f, 0.580f),
           new Color(0.898f, 0.686f, 0.271f),
           new Color(1.000f, 0.820f, 0.310f)
        }
    };
    private int paletteSize, paletteCount;

 
    private float totalSec;
    private int paletteIdx;


    override public Texture2D create()
    {
        // Create a texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        maxCrystals = 500000;//(textureSize * textureSize) / 4;
        //Debug.Log("Background_DLA_Script.create(): maxCrystals="+ maxCrystals);

        attractorCount = Random.Range(5,30);
        attractorList = new Particle[attractorCount];


        for (int i = 0; i < attractorCount; i++)
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

    public override bool isDone()
    {
        if (crystalCount < maxCrystals) return false;
        return true;
    }


    override public void clear()
    {
        //It is such stupid syntax that reading a value changes the value.
        //  If C# were not stupid, this would be a function call: Random.value()
        //Debug.Log(Random.value + ", " + Random.value + ", " + Random.value);
        totalSec = 0;
        crystalCount = 0;
        //paletteIdx = Random.Range(0,paletteCount);
        paletteIdx = 1;
        Color nearBlack = new Color(0.01f, 0f, 0f);

        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                if (x==0 || y == 0 || x == textureSize-1 || y == textureSize-1)
                {
                    texture.SetPixel(x, y, nearBlack);
                }
                else texture.SetPixel(x, y, Color.black);
            }
        }

        for (int i = 0; i < attractorCount; i++)
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
            if (i >= STAR_POINTS && crystalCount >= maxCrystals) break;

            int x = pointList[i].x;
            int y = pointList[i].y;
            if (pointList[i].x < 0) continue;

            

            bool crystalize = false;
            if (crystalCount < maxCrystals)
            {
                if (isCrystal(x - 1, y)) crystalize = true;
                else if (isCrystal(x + 1, y)) crystalize = true;
                else if (isCrystal(x, y - 1)) crystalize = true;
                else if (isCrystal(x, y + 1)) crystalize = true;

                else if (isCrystal(x - 1, y - 1)) crystalize = true;
                else if (isCrystal(x - 1, y + 1)) crystalize = true;
                else if (isCrystal(x + 1, y - 1)) crystalize = true;
                else if (isCrystal(x + 1, y + 1)) crystalize = true;
            }

            if (crystalize)
            {
                crystalCount++;
                //if ((crystalCount % 1000 == 0) || (crystalCount > maxCrystals)) Debug.Log("Background_DLA_Script.next(): " + crystalCount);

                int colorIdx = (int)(paletteSize * (float)crystalCount/(maxCrystals +1f));
                texture.SetPixel(x, y, palette[paletteIdx, colorIdx]);
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
                    if (Random.value < 0.75f) texture.SetPixel(x, y, Color.white);
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

                if (n > 10) break;
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
            int goalIdx = Random.Range(0, attractorCount - 1);
            goalx = attractorList[goalIdx].x;
            goaly = attractorList[goalIdx].y;
        }
    }
}
