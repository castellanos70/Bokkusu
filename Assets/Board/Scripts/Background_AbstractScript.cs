using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Background_AbstractScript : MonoBehaviour
{
    protected static int textureSize = 2048;
    protected Texture2D texture;

    public abstract Texture2D create();
    public abstract void update();
}