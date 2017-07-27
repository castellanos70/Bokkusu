using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaleidoscope
{
    private static int triangleCount = 5;
    private static int reflectionCount = 8;
    private Vector2[,] triangleList = new Vector2[triangleCount,3];
    private Color[] triangleColor = new Color[triangleCount];
    private Texture2D texture1, texture2;
    private int pixelSize;

    private static int MORPH_PARAM_COUNT = 3;
    private int[] morphTriangleIdx = new int[MORPH_PARAM_COUNT];
    private int[] morphVertexIdx = new int[MORPH_PARAM_COUNT];
    private int[] morphDeltaX = new int[MORPH_PARAM_COUNT];
    private int[] morphDeltaY = new int[MORPH_PARAM_COUNT];

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




    public Kaleidoscope(Material mat1, Material mat2, int pixelSize)
    {
        this.pixelSize = pixelSize;
        mat1.SetFloat("_Glossiness", 0.0f);
        mat1.SetFloat("_Metallic", 0.0f);
        mat1.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mat1.EnableKeyword("_GLOSSYREFLECTIONS_OFF");

        mat2.SetFloat("_Glossiness", 0.0f);
        mat2.SetFloat("_Metallic", 0.0f);
        mat2.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        mat2.EnableKeyword("_GLOSSYREFLECTIONS_OFF");

        texture1 = new Texture2D(pixelSize, pixelSize, TextureFormat.ARGB32, false);
        texture2 = new Texture2D(pixelSize, pixelSize, TextureFormat.ARGB32, false);

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

        mat1.mainTexture = texture1;
        mat2.mainTexture = texture2;

        setMorphParams();
    }


    private void setMorphParams()
    {
        for (int i = 0; i < MORPH_PARAM_COUNT; i++)
        {
            morphTriangleIdx[i] = Random.Range(0, triangleCount);
            morphVertexIdx[i] = Random.Range(0, 3);
            morphDeltaX[i] = 0;
            morphDeltaY[i] = 0;
            if (Random.value < 0.5f)
            {
                if (Random.value < 0.5f) morphDeltaX[i] = -1; else morphDeltaX[i] = 1;
            }
            else
            {
                if (Random.value < 0.5f) morphDeltaY[i] = -1; else morphDeltaY[i] = 1;
            }
        }
    }



    public void updateTexture()
    {

        DrawUtilities.setTextureColor(texture1, pixelSize, Color.white);
        DrawUtilities.setTextureColor(texture2, pixelSize, Color.black);
        Vector2[] v = new Vector2[3];
        Vector2[] w = new Vector2[3];


        if (Random.value < 0.01f) setMorphParams();

        for (int i = 0; i < MORPH_PARAM_COUNT; i++)
        {
            float x = triangleList[morphTriangleIdx[i], morphVertexIdx[i]].x + morphDeltaX[i];
            float y = triangleList[morphTriangleIdx[i], morphVertexIdx[i]].y + morphDeltaY[i];

            if (x > pixelSize / 2 - 1)
            {
                x = pixelSize / 2 - 1;
                morphDeltaX[i] = -1;
            }
            else if (x < 0)
            {
                x = 0;
                morphDeltaX[i] = 1;
            }


            if (y > pixelSize / 2 - 1)
            {
                y = pixelSize / 2 - 1;
                morphDeltaY[i] = -1;
            }
            else if (y < 0)
            {
                y = 0;
                morphDeltaY[i] = 1;
            }

            // Confine initial pattern to lower right quadrant
            if (x > y)
            {
                float tmp = x;
                x = y;
                y = tmp;
            }

            triangleList[morphTriangleIdx[i], morphVertexIdx[i]].x = x;
            triangleList[morphTriangleIdx[i], morphVertexIdx[i]].y = y;
        }

        for (int n = 0; n < triangleCount; n++)
        {
            for (int k = 0; k < 3; k++)
            {
                v[k].x = triangleList[n, k].x;
                v[k].y = triangleList[n, k].y;
            }



            for (int k = 0; k < reflectionCount; k++)
            {
                kaleidoscopicReflect(v, w, k, pixelSize / 2);
                DrawUtilities.drawTriangle(texture1, triangleColor[n], w);
                DrawUtilities.drawTriangle(texture2, triangleColor[n], w);
            }
        }
        texture1.Apply();
        texture2.Apply();

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
