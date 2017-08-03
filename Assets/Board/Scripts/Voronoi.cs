using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi
{
    private Texture2D texture;
    private Color[] colorData;
    private int pixelSize;

    private static int POINTCOUNT = 7;
    private int[] pointListX = new int[POINTCOUNT];
    private int[] pointListY = new int[POINTCOUNT];

    private int[] speedX = new int[POINTCOUNT];
    private int[] speedY = new int[POINTCOUNT];

    //private float red = 178f/256f;
    //private float green = 0f / 256f;
    //private float blue = 21f / 256f;
    private float red0, green0, blue0;




    public Voronoi(Material material, int pixelSize, int r, int g, int b)
    {
        this.pixelSize = pixelSize;
        red0 = r/255f;
        green0 = g / 255f;
        blue0 = b / 255f;


        material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
        material.SetFloat("_Glossiness", 0.0f);
        material.SetFloat("_Metallic", 0.0f);

        texture = new Texture2D(pixelSize, pixelSize, TextureFormat.ARGB32, false);
        colorData = new Color[pixelSize * pixelSize];

        material.mainTexture = texture;

        randomizePoints();
        next();
    }




    public void next()
    {
        for (int i = 0; i < POINTCOUNT; i++)
        {
            pointListX[i] += speedX[i];
            pointListY[i] += speedY[i];
            if (pointListX[i] < 0)
            {
                pointListX[i] = 0;
                speedX[i] = Random.Range(1, 2);
            }
            if (pointListY[i] < 0)
            {
                pointListY[i] = 0;
                speedY[i] = Random.Range(1, 2);
            }
            if (pointListX[i] > pixelSize - 1)
            {
                pointListX[i] = pixelSize - 1;
                speedX[i] = Random.Range(-2, -1);
            }
            if (pointListY[i] > pixelSize - 1)
            {
                pointListY[i] = pixelSize - 1;
                speedY[i] = Random.Range(-2, -1);
            }
        }

        for (int x = 0; x < pixelSize; x++)
        {
            for (int y = 0; y < pixelSize; y++)
            {
                int minDistSquared = pixelSize * pixelSize;
                for (int i = 0; i < POINTCOUNT; i++)
                {
                    int dx = x - pointListX[i];
                    int dy = y - pointListY[i];
                    int distSqu = dx * dx + dy * dy;

                    if (distSqu < minDistSquared) minDistSquared = distSqu;
                }
                float dist = Mathf.Sqrt(minDistSquared);
                float brightness = 1.0f - dist / 80;
                if (brightness < 0f) brightness = 0.0f;
                if (brightness > 1f) brightness = 1.0f;

                float r = red0 * brightness;
                float g = green0 * brightness;
                float b = blue0 * brightness;

 
                colorData[y * pixelSize + x] = new Color(r, g, b);
            }
        }
        texture.SetPixels(colorData);
        texture.Apply();
    }



    private void randomizePoints()
    {
        for (int i = 0; i < POINTCOUNT; i++)
        {
            pointListX[i] = Random.Range(0, pixelSize - 1);
            pointListY[i] = Random.Range(0, pixelSize - 1);

            speedX[i] = Random.Range(-2, 2);
            speedY[i] = Random.Range(-2, 2);
        }
    }
}
