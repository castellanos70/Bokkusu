using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTexture
{
    private Texture2D texture;
    private Color32[] colorData;

    private int pixelSize;
    private float noiseScale;
    private float morphRate;
    private float textureShift;
    private int paletteIdx;

    private Color32[] palette;
    private float[] colorThreshold;


    public NoiseTexture(Material material, int pixelSize, float noise, float morphRate, int paletteIdx)
    {
        this.pixelSize = pixelSize;
        noiseScale = noise;
        this.morphRate = morphRate;
        this.paletteIdx = paletteIdx;

        material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
        material.SetFloat("_Glossiness", 0.0f);
        material.SetFloat("_Metallic", 0.0f);

        textureShift = Random.value * 100;
        texture = new Texture2D(pixelSize, pixelSize, TextureFormat.ARGB32, false);
        colorData = new Color32[pixelSize * pixelSize];

        material.mainTexture = texture;

        if (paletteIdx == 0)
        { palette = new Color32[]
          {
             new Color32(232, 242, 245,255),
             new Color32(215, 229, 241,255),
             new Color32(198, 216, 238,255),
             new Color32(176, 191, 228,255)
          };

            colorThreshold = new float[]
            { 0.3333f,  0.7f, 0.95f, 1.0f };
        }
        else
        {
            palette = new Color32[]
            {
               new Color32(0, 0, 0,255),
               new Color32(48, 48, 59, 255),
               new Color32(97, 97, 118, 255)
            };

            colorThreshold = new float[]
            { 0.3333f, 0.95f, 1.0f };
        }
    }


    public void next()
    {
        textureShift += Time.deltaTime * morphRate;
        for (int x = 0; x < pixelSize; x++)
        {
            for (int y = 0; y < pixelSize; y++)
            {
                int pixel = y * pixelSize + x;
                if ((paletteIdx == 0) && (x == 0 || y == 0 || x == pixelSize - 1 || y == pixelSize - 1))
                {
                    texture.SetPixel(x, y, Color.white);
                    colorData[pixel] = Color.white;
                }
                else
                {
                    float xCoord = textureShift + x * noiseScale;
                    float yCoord = textureShift + y * noiseScale;
                    float noise = Mathf.PerlinNoise(xCoord, yCoord);

                    float val = (1 + Mathf.Sin((x + noise / 2) * 50)) * 0.501960784f;

                    int idx = palette.Length - 1;
                    for (int i = 0; i < palette.Length; i++)
                    {
                        if (val < colorThreshold[i])
                        {
                            idx = i;
                            break;
                        }
                    }
                    //if (x==1) Debug.Log("val=" + val + ", idx=" + idx);
                    //texture.SetPixel(x, y, palette[idx]);
                    colorData[pixel] = palette[idx];
                }
            }
        }
        texture.SetPixels32(colorData);
        texture.Apply();
    }
}
