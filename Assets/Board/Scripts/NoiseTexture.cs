using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTexture //: MonoBehaviour
{
    private Texture2D texture;
    private int pixelSize;
    private float noiseScale;
    private float morphRate;
    private float textureShift;
    private int paletteIdx;

    private Color[] palette;
    private float[] colorThreshold;


    //float noiseScale = 0.03f;
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

        material.mainTexture = texture;

        if (paletteIdx == 0)
        { palette = new Color[]
          {
             new Color(0.910f, 0.949f, 0.961f),
             new Color(0.843f, 0.898f, 0.947f),
             new Color(0.776f, 0.847f, 0.933f),
             new Color(0.690f, 0.749f, 0.893f)
          };

            colorThreshold = new float[]
            { 0.3333f,  0.7f, 0.95f, 1.0f };
        }
        else
        {
            palette = new Color[]
            {
               new Color(0.000f, 0.000f, 0.000f),
               new Color(0.190f, 0.190f, 0.231f),
               new Color(0.380f, 0.380f, 0.463f)
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
                if ((paletteIdx == 0) && (x == 0 || y == 0 || x == pixelSize - 1 || y == pixelSize - 1))
                {
                    texture.SetPixel(x, y, Color.white);
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
                    texture.SetPixel(x, y, palette[idx]);
                }
            }
        }
        texture.Apply();
        //material.mainTexture = texture;
    }
}
