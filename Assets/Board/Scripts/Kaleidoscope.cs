using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaleidoscope
{
    private static int triangleCount = 6;
    private Vector2[,] triangleList = new Vector2[triangleCount,3];
    private Color[] triangleColor = new Color[triangleCount];
    private Texture2D texture;
    private int pixelSize;

    private int morphTriangleIdx;
    private int morphVertexIdx;
    private int morphDeltaX, morphDeltaY;

    private static Color[] palette =
    {
           new Color(0.306f, 0.376f, 0.275f),
           new Color(0.349f, 0.475f, 0.369f),
           new Color(0.404f, 0.549f, 0.427f),
           new Color(0.486f, 0.541f, 0.388f),
           new Color(0.624f, 0.529f, 0.322f),
           new Color(0.678f, 0.592f, 0.322f),
           new Color(0.753f, 0.647f, 0.443f),
           new Color(0.804f, 0.686f, 0.451f)
     };




    public Kaleidoscope(Material material, int pixelSize)
    {
        this.pixelSize = pixelSize;
        texture = new Texture2D(pixelSize, pixelSize, TextureFormat.ARGB32, false);

        for (int n = 0; n < triangleCount; n++)
        {

            Vector2[] v = new Vector2[3];
            for (int i = 0; i < v.Length; i++)
            {
                triangleList[n, i].x = Random.Range(0, pixelSize / 2);
                triangleList[n, i].y = Random.Range(0, pixelSize / 2);

                // Confine initial pattern to lower right quadrant
                if (triangleList[n, i].x > triangleList[n, i].y)
                {
                    float tmp = triangleList[n, i].x;
                    triangleList[n, i].x = triangleList[n, i].y;
                    triangleList[n, i].y = tmp;
                }
            }
            triangleColor[n] = palette[Random.Range(0, 6)];
        }

        material.mainTexture = texture;

        setMorphParams();
    }


    private void setMorphParams()
    {
        morphTriangleIdx = Random.Range(0, triangleCount);
        morphVertexIdx = Random.Range(0, 3);
        morphDeltaX = 1;
        morphDeltaY = 1;
        if (Random.value < 0.5f) morphDeltaX = -1;
        if (Random.value < 0.5f) morphDeltaY = -1;
    }



    public void updateTexture()
    {

        DrawUtilities.setTextureColor(texture, pixelSize, Color.black);
        Vector2[] v = new Vector2[3];
        Vector2[] w = new Vector2[3];


        if (Random.value < 0.025f) setMorphParams();

        float x = triangleList[morphTriangleIdx, morphVertexIdx].x + morphDeltaX;
        float y = triangleList[morphTriangleIdx, morphVertexIdx].y + morphDeltaY;

        if (x > pixelSize / 2 - 1)
        {
            x = pixelSize / 2 - 1;
            morphDeltaX = -1;
        }
        else if (x < 0)
        {   x = 0;
            morphDeltaX = 1;
        }


        if (y > pixelSize / 2 - 1)
        {
            y = pixelSize / 2 - 1;
            morphDeltaY = -1;
        }
        else if (y < 0)
        {
            y = 0;
            morphDeltaY = 1;
        }

        // Confine initial pattern to lower right quadrant
        if (x > y)
        {
            float tmp = x;
            x = y;
            y = tmp;
        }

        triangleList[morphTriangleIdx, morphVertexIdx].x = x;
        triangleList[morphTriangleIdx, morphVertexIdx].y = y;

        for (int n = 0; n < triangleCount; n++)
        {
            for (int i = 0; i < 3; i++)
            {
                //Debug.Log("r=" + r + ": triangle(" + i + ") = (" + triangleList[n, i].x + ", " + triangleList[n, i].y + ")");
               
                v[i].x = triangleList[n, i].x;
                v[i].y = triangleList[n, i].y;
                //Debug.Log("v=" + v[i].x + ", " + v[i].y);
            }



            for (int k = 0; k < 8; k++)
            {
                kaleidoscopicReflect(v, w, k, pixelSize / 2);
                DrawUtilities.drawTriangle(texture, triangleColor[n], w);
            }
        }
        texture.Apply();
        
    }


    private static void kaleidoscopicReflect(Vector2[] v, Vector2[] w, int n, int offset)
    {
        for (int i = 0; i < v.Length; i++)
        {
            if (n == 0)
            {
                w[i].x = v[i].x + offset;
                w[i].y = v[i].y + offset;
            }
            else if (n == 1)
            {
                w[i].x = -v[i].x + offset;
                w[i].y = v[i].y + offset;
            }
            else if (n == 2)
            {
                w[i].x = v[i].x + offset;
                w[i].y = -v[i].y + offset;
            }
            else if (n == 3)
            {
                w[i].x = -v[i].x + offset;
                w[i].y = -v[i].y + offset;
            }
            else if (n == 4)
            {
                w[i].x = v[i].y + offset;
                w[i].y = v[i].x + offset;
            }
            else if (n == 5)
            {
                w[i].x = -v[i].y + offset;
                w[i].y = v[i].x + offset;
            }
            else if (n == 6)
            {
                w[i].x = v[i].y + offset;
                w[i].y = -v[i].x + offset;
            }
            else if (n == 7)
            {
                w[i].x = -v[i].y + offset;
                w[i].y = -v[i].x + offset;
            }
        }
    }

}
