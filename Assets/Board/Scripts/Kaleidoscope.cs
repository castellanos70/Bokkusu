﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaleidoscope
{
    private static int triangleCount = 5;
    private static int reflectionCount = 8;
    private Vector2[,] triangleList = new Vector2[triangleCount,3];
    private Color[] triangleColor = new Color[triangleCount];
    private Texture2D texture1, texture2;
    private Color32[] colorData1, colorData2;
    private int pixelSize;

    private static int MORPH_PARAM_COUNT = 3;
    private int[] morphTriangleIdx = new int[MORPH_PARAM_COUNT];
    private int[] morphVertexIdx = new int[MORPH_PARAM_COUNT];
    private int[] morphDeltaX = new int[MORPH_PARAM_COUNT];
    private int[] morphDeltaY = new int[MORPH_PARAM_COUNT];

    private static Color32[] palette =
    {
           new Color32(78, 96, 70, 255),
           new Color32(89, 121, 94, 255),
           new Color32(103, 140, 109,255),
           new Color32(124, 138, 99,255),
           new Color32(159, 135, 82,255),
           new Color32(173, 151, 82,255),
           new Color32(192, 165, 123,255),
           new Color32(205, 175, 115,255)
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

        colorData1 = new Color32[pixelSize* pixelSize];
        colorData2 = new Color32[pixelSize * pixelSize];

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

        //DrawUtilities.setTextureColor(texture1, pixelSize, Color.white);
        //DrawUtilities.setTextureColor(texture2, pixelSize, Color.black);
        DrawUtilities.clear(colorData1, new Color32(255,255,255,255));
        DrawUtilities.clear(colorData2, new Color32(0, 0, 0, 255));

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
                DrawUtilities.drawTriangle(colorData1, pixelSize, triangleColor[n], w);
                DrawUtilities.drawTriangle(colorData2, pixelSize, triangleColor[n], w);
            }
        }

        texture1.SetPixels32(colorData1);
        texture1.Apply();

        texture2.SetPixels32(colorData2);
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
