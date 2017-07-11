using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawUtilies : MonoBehaviour
{
    public static void drawTriangle(Texture2D texture, Color color, Vector2[] v)
    {
        // at first sort the three vertices by y-coordinate ascending so v1 is the topmost vertice
        sortVerticesAscendingByY(v);

        // here we know that v1.y <= v2.y <= v3.y 
        // check for trivial case of bottom-flat triangle
        if (v[1].y == v[2].y)
        {
              fillBottomFlatTriangle(texture, color, v[0], v[1], v[2]);
        }
        // check for trivial case of top-flat triangle 
        else if (v[0].y == v[1].y)
        {
            fillTopFlatTriangle(texture, color, v[0], v[1], v[2]);
        }
        else
        {
            Vector2 v4 = new Vector2();
            v4.x = (int)(v[0].x + ((float)(v[1].y - v[0].y) / (float)(v[2].y - v[0].y)) * (v[2].x - v[0].x));
            v4.y=v[1].y;

            fillBottomFlatTriangle(texture, color, v[0], v[1], v4);
            fillTopFlatTriangle(texture, color, v[1], v4, v[2]);
        }
    }


    private static void fillBottomFlatTriangle(Texture2D texture, Color color, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float invslope1 = (v2.x - v1.x) / (v2.y - v1.y);
        float invslope2 = (v3.x - v1.x) / (v3.y - v1.y);

        float curx1 = v1.x;
        float curx2 = v1.x;

        for (int y = (int)v1.y; y <= v2.y; y++)
        {
            drawHorzLine(texture, color, (int)curx1, (int)curx2, y);
            curx1 += invslope1;
            curx2 += invslope2;
        }
    }


    private static void fillTopFlatTriangle(Texture2D texture, Color color, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float invslope1 = (v3.x - v1.x) / (v3.y - v1.y);
        float invslope2 = (v3.x - v2.x) / (v3.y - v2.y);

        float curx1 = v3.x;
        float curx2 = v3.x;

        for (int y = (int)v3.y; y > v1.y; y--)
        {
            drawHorzLine(texture, color, (int)curx1, (int)curx2, y);
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
    //It is totally unclear to me when Unity and or C# pass by reference verses by value
    //  For example, this swap, passing an array, works by reference, but
    //  Passing objects: private static void swap(Vector2 v1, Vector2 v2) is passed by value.
    //===================================================================================
    private static void swap(Vector2[] v, int i, int k)
    {
        Vector2 tmp = v[i];
        v[i] = v[k];
        v[k] = tmp;
    }


    private static void drawHorzLine(Texture2D texture, Color color, int x1, int x2, int y)
    {
        for (int x = x1; x <= x2; x++)
        {
            texture.SetPixel(x, y, color);
        }
    }

}
