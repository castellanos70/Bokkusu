using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawUtilities : MonoBehaviour
{
    



public static void drawTriangle(Color32[] colorData, int imageWidth, Color color, Vector2[] v)
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


    private static void fillBottomFlatTriangle(Color32[] colorData, int imageWidth, Color color, Vector2 v1, Vector2 v2, Vector2 v3)
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


    private static void fillTopFlatTriangle(Color32[] colorData, int imageWidth, Color color, Vector2 v1, Vector2 v2, Vector2 v3)
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


    private static void drawHorzLine(Color32[] colorData, int imageWidth, Color color, int x1, int x2, int y)
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
            //colorData[yy + x] = color;
            //The statement:
            //    colorData[yy + x] = color
            //does an implicit object conversion of Color to Color32,
            //  instantiating a new object for every pixel for every frame. 
            //The three lines below offer many times speed up.
            colorData[yy + x].r = (byte)(color.r * 255);
            colorData[yy + x].g = (byte)(color.g * 255);
            colorData[yy + x].b = (byte)(color.b * 255);
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


    public static void clear(Color32[] colorData, Color32 color)
    {
        for (int i = 0; i < colorData.Length; i++)
        {
            //colorData[i] = c;
            colorData[i].r = color.r;
            colorData[i].g = color.g;
            colorData[i].b = color.b;
        }
    }
}
