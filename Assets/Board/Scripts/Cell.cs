using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private CameraScript.Element type = CameraScript.Element.NOTHING;
    private GameObject baseObj = null;
    private GameObject overlayObj = null;
    private float bottomY = 0;
    private float fallSpeed;
    private int x, z;
    private bool doorRaising = false;
    private bool doorLowering = false;

    public Cell(CameraScript.Element element, GameObject block, Material mat)
    {
        this.type = element;
        this.baseObj = block;
        this.x = (int)baseObj.transform.position.x;
        this.z = (int)baseObj.transform.position.z;

        Renderer renderer = baseObj.GetComponent<Renderer>();
        renderer.material = mat;

        if (type == CameraScript.Element.FLOOR || type == CameraScript.Element.WALL)
        {
            block.transform.Rotate(new Vector3(0, 90 * Random.Range(0, 4), 0));
        }

        bottomY = 0f;
        if (type == CameraScript.Element.WALL) bottomY = 1f;
        else if (type == CameraScript.Element.DOOR_B) bottomY = 1f;

        fallSpeed = 8 + Random.value * 15;
    }


    public float getFallSpeed() { return fallSpeed; }

    public float getY() { return baseObj.transform.position.y; }
    public void fallTo(float y)
    {
        if (y <= bottomY)
        {
            fallSpeed = 0f;
            y = bottomY;
        }
        baseObj.transform.Translate(0, y-getY(), 0);
    }

    public void addCrate(GameObject obj)
    {
        this.overlayObj = obj;
        type = CameraScript.Element.CRATE;
        //obj.transform.Rotate(new Vector3(0, 90 * Random.Range(0, 4), 0));
    }


    public void toggleDoor()
    {
        if (getY() == 1f) doorLowering = true;
        else if (getY() == 0f) doorRaising = true;
    }


    public void updateDoor(float seconds)
    {
        if (!doorLowering && !doorRaising) return;

        float yy = seconds;
        if (doorRaising) yy = 1f - seconds;

        if (seconds == 0f)
        {
            doorLowering = false;
            doorRaising = false;
        }
        baseObj.transform.position = new Vector3(x, yy, z);
    }


    public CameraScript.Element getType()
    {
        return type;
    }


    public void setType(CameraScript.Element type)
    {
        this.type = type;
    }

    public void smashCrate(float speed)
    {
        if (type != CameraScript.Element.CRATE) return;
        if (overlayObj == null) return;

        CrateScript crate = (CrateScript)overlayObj.GetComponent(typeof(CrateScript));
        crate.detonate(speed);
        type = CameraScript.Element.FLOOR;
        overlayObj = null;
    }


    public void destroyObjects()
    {
        if (baseObj != null) Object.Destroy(baseObj);
        if (overlayObj != null)
        {
            if (!overlayObj.CompareTag("Finish")) Object.Destroy(overlayObj);
        }
    }
}
