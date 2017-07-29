using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawUtilities : MonoBehaviour
{
    



public static void drawTriangle(Color[] colorData, int imageWidth, Color color, Vector2[] v)
    {
        // at first sort the three vertices by y-coordinate ascending so v1 is the topmost vertice
        sortVerticesAscendingByY(v);

        // here we know that v1.y <= v2.y <= v3.y 
        // check for trivial case of bottom-flat triangle
        if (v[1].y == v[2].y)
        {
              fillBottomFlatTriangle(colorData, imageWidth, color, v[0], v[1], v[2]);
        }
        // check for trivial case of top-flat triangle 
        else if (v[0].y == v[1].y)
        {
            fillTopFlatTriangle(colorData, imageWidth, color, v[0], v[1], v[2]);
        }
        else
        {
            Vector2 v4 = new Vector2();
            v4.x = (int)(v[0].x + ((float)(v[1].y - v[0].y) / (float)(v[2].y - v[0].y)) * (v[2].x - v[0].x));
            v4.y=v[1].y;

           // Debug.Log("v4=(" + v4.x + ", " + v4.y + ")");

            fillBottomFlatTriangle(colorData, imageWidth, color, v[0], v[1], v4);
            fillTopFlatTriangle(colorData, imageWidth, color, v[1], v4, v[2]);
        }
    }


    private static void fillBottomFlatTriangle(Color[] colorData, int imageWidth, Color color, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        //Debug.Log("fillBottomFlatTriangle   v={ (" + v1.x + "," + v1.y + "), (" +
        //        v2.x + "," + v2.y + "), (" +
        //        v3.x + "," + v3.y + ") }");

        float invslope1 = (v2.x - v1.x) / (v2.y - v1.y);
        float invslope2 = (v3.x - v1.x) / (v3.y - v1.y);

        float curx1 = v1.x;
        float curx2 = v1.x;

        for (int y = (int)v1.y; y <= v2.y; y++)
        {
            drawHorzLine(colorData, imageWidth, color, (int)curx1, (int)curx2, y);
            //Debug.Log("    curx1=" + curx1 + ", curx2=" + curx2);
            curx1 += invslope1;
            curx2 += invslope2;
        }
    }


    private static void fillTopFlatTriangle(Color[] colorData, int imageWidth, Color color, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float invslope1 = (v3.x - v1.x) / (v3.y - v1.y);
        float invslope2 = (v3.x - v2.x) / (v3.y - v2.y);

        float curx1 = v3.x;
        float curx2 = v3.x;

        for (int y = (int)v3.y; y > v1.y; y--)
        {
            drawHorzLine(colorData, imageWidth, color, (int)curx1, (int)curx2, y);
            curx1 -= invslope1;
            curx2 -= invslope2;
        }
    }

    private static void sortVerticesAscendingByY(Vector2[] v)
    {
        //Debug.Log("sortVerticesAscendingByY(enter)   v={ (" + v[0].x + "," + v[0].y + "), (" +
        //        v[1].x + "," + v[1].y + "), (" +
        //        v[2].x + "," + v[2].y + ") }");

        if (v[2].y < v[1].y)
        {
            swap(v, 1,2);
        }
        if (v[1].y < v[0].y) swap(v, 0, 1);
        if (v[2].y < v[1].y) swap(v, 1, 2); 
    }


    //===================================================================================
    //It is totally unclear to me (joel) when Unity and or C# pass by reference verses by value
    //  For example, this swap, passing an array, works by reference, but
    //  Passing objects: private static void swap(Vector2 v1, Vector2 v2) is passed by value.
    //===================================================================================
    private static void swap(Vector2[] v, int i, int k)
    {
        Vector2 tmp = v[i];
        v[i] = v[k];
        v[k] = tmp;
    }


    private static void drawHorzLine(Color[] colorData, int imageWidth, Color color, int x1, int x2, int y)
    {
        if (x1 > x2)
        {
            int tmp = x1;
            x1 = x2;
            x2 = tmp;
        }
        int yy = y * imageWidth;
        for (int x = x1; x <= x2; x++)
        {
            //texture.SetPixel(x, y, color);
            colorData[yy + x] = color;
        }
    }


    public static void setTextureColor(Texture2D texture, int pixelSize, Color color)
    {
        for (int x = 0; x < pixelSize; x++)
        {
            for (int y = 0; y < pixelSize; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }


    public static void clear(Color[] colorData, Color c)
    {
        for (var i = 0; i < colorData.Length; i++)
        {
            colorData[i] = c;
        }
    }


    /*


    private static void generateWallTexture(Material material, int pixelSize, float textureShift)
    {
        material.SetFloat("_Glossiness", 0.0f);
        material.SetFloat("_Metallic", 0.0f);
        Texture2D texture = new Texture2D(pixelSize, pixelSize, TextureFormat.ARGB32, false);


      

        Color[] palette = {
           new Color(0.000f, 0.000f, 0.000f),
           new Color(0.190f, 0.190f, 0.231f),
           new Color(0.380f, 0.380f, 0.463f)
        };


        float noiseScale = 0.02f;

        float x0 = textureShift;// Random.value*32768;
        float y0 = textureShift;// Random.value * 32768;

        for (int x = 0; x < pixelSize; x++)
        {
            for (int y = 0; y < pixelSize; y++)
            {
                //(1 + sin((x + noise(x * 5, y * 5) / 2) * 50)) / 2
                //var val = (1 + sin((x + noise(x*noiseScale, y*noiseScale)/2)*50))*128;

                //float xCoord = noiseScale*(x0 + (x / pixelSize));
                //float yCoord = noiseScale*(y0 + (y / pixelSize));

                //float noise = Mathf.PerlinNoise(xCoord, yCoord);

                //float val = (1 + Mathf.Sin((x/pixelSize + noise) * pixelSize/2)) / 2;
                //Debug.Log("noise=" + noise);
                //float val = noise;


                float xCoord = x0+ x * noiseScale;
                float yCoord = y0 + y * noiseScale;
                float noise = Mathf.PerlinNoise(xCoord, yCoord);

                float val = (1 + Mathf.Sin((x + noise / 2) * 50)) * 0.501960784f;


                int idx = 0;
                if (val < 0.3333f) idx = 0;
                else if (val < 0.95f) idx = 1;
                else idx = 2;
                texture.SetPixel(x, y, palette[idx]);
                //texture.SetPixel(x, y, new Color(val, val, val));
            }
        }

        texture.Apply();
        material.mainTexture = texture;
    }









    private static void generateFloorTexture(Material material, int pixelSize, float textureShift)
    {
        material.SetFloat("_Glossiness", 0.0f);
        material.SetFloat("_Metallic", 0.0f);
        Texture2D texture = new Texture2D(pixelSize, pixelSize, TextureFormat.ARGB32, false);


        Color[] palette = {
           new Color(0.910f, 0.949f, 0.961f),
           new Color(0.843f, 0.898f, 0.947f),
           new Color(0.776f, 0.847f, 0.933f),
           new Color(0.690f, 0.749f, 0.893f)
        };


        float noiseScale = 0.03f;
        for (int x = 0; x < pixelSize; x++)
        {
            for (int y = 0; y < pixelSize; y++)
            {
                if (x == 0 || y == 0 || x == pixelSize - 1 || y == pixelSize - 1) texture.SetPixel(x, y, Color.white);
                else
                {
                    float xCoord = textureShift + x * noiseScale;
                    float yCoord = textureShift + y * noiseScale;
                    float noise = Mathf.PerlinNoise(xCoord, yCoord);

                    float val = (1 + Mathf.Sin((x + noise / 2) * 50)) * 0.501960784f;


                    int idx = 0;
                    if (val < 0.3333f) idx = 0;
                    else if (val < 0.7f) idx = 1;
                    else if (val < 0.95f) idx = 2;
                    else idx = 3;
                    texture.SetPixel(x, y, palette[idx]);
                }
            }
        }

        texture.Apply();
        material.mainTexture = texture;
    }

    */
}
