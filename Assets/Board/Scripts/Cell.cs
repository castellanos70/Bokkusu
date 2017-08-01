using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    private CameraScript.Element type = CameraScript.Element.NOTHING;
    private GameObject baseObj = null;
    private GameObject crateObj = null;
    private float bottomY = 0;
    private float fallSpeed;
    private int x, z;
	private int audioX, audioZ;
	//private bool playedSound = false;
    private bool doorRaising = false;
    private bool doorLowering = false;


    private bool doorIsDown = false;

    private AudioSource audio;
    private AudioClip audioClip;

    private static Material doorMat1, doorMat2;
    

    

    public Cell(CameraScript.Element element, GameObject block, Material mat)
    {
        this.type = element;
        this.baseObj = block;
        this.x = (int)baseObj.transform.position.x;
        this.z = (int)baseObj.transform.position.z;


        if (type == CameraScript.Element.FLOOR || type == CameraScript.Element.WALL)
        {
            block.transform.Rotate(new Vector3(0, 90 * Random.Range(0, 4), 0));
        }

        bottomY = 0f;
        if (type == CameraScript.Element.WALL) bottomY = 1f;
        else if (type == CameraScript.Element.DOOR_B) bottomY = 1f;
  

        Renderer renderer = baseObj.GetComponent<Renderer>();
        renderer.material = mat;


        audio = baseObj.AddComponent<AudioSource>();
		audioX = audioZ = -1;

        fallSpeed = 8 + Random.value * 15;
		//fallSpeed = 11;
    }


    public static void setDoorMat(Material mat1, Material mat2)
    {
        doorMat1 = mat1;
        doorMat2 = mat2;
    }


    public void setAudioClip(AudioClip audioClip)
    {
		this.audioClip = audioClip;
	}

    //public bool hasPlayedAudio()
    //{
    //	return audioIndex;
    //}

    public void playAudioClip(float priority)
    {
		//audio.Stop();
		if (audioClip == null) return;
		audio.priority = (int)priority;
		audio.PlayOneShot(audioClip, 0.5f);
		//playedSound = true;
		//Debug.Log(priority);
	}

	public void playAudioClip(float priority, int x, int z){
		if (audioClip == null) return;
		if (audioX == x && audioZ == z) return;
		audioX = x;
		audioZ = z;

		audio.Stop();
		audio.priority = (int)priority;
		audio.PlayOneShot(audioClip, 0.5f);
		//playedSound = true;
	}
		
    public float getFallSpeed() { return fallSpeed; }

    public float getY() { return baseObj.transform.position.y; }

    public bool fallTo(float y)
    {
        if (y <= bottomY)
        {
            fallSpeed = 0f;
            y = bottomY;
            if (type == CameraScript.Element.DOOR_A) doorIsDown = true;
        }
        baseObj.transform.Translate(0, y-getY(), 0);
        if (y <= bottomY) return true;
        return false;
    }

    public void addCrate(GameObject obj)
    {
        this.crateObj = obj;
        type = CameraScript.Element.CRATE;
        //obj.transform.Rotate(new Vector3(0, 90 * Random.Range(0, 4), 0));
    }


    public void toggleDoor()
    {
        //Debug.Log("toggleDoor(): y=" + getY());
        if (getY() == 1f) doorLowering = true;
        else if (doorIsDown)
        {
            if (crateObj != null) return;

            //baseObj.transform.localScale = new Vector3(1f, 1f, 1f);
            baseObj.transform.position = new Vector3(x, 0f, z);
            baseObj.transform.rotation = Quaternion.identity;
            doorRaising = true;
            doorIsDown = false;
            Renderer renderer = baseObj.GetComponent<Renderer>();
            renderer.material = doorMat2;
        }

    }


    public bool isDoorDown()
    {
        if (type == CameraScript.Element.DOOR_A || type == CameraScript.Element.DOOR_B)
        {
            return doorIsDown;
        }
        return false;
    }




    public void updateDoor(float doorToggleSeconds)
    {
        //if (doorIsDown)
        //{
        //    baseObj.transform.rotation = Quaternion.Euler(0, 180f*doorDownAngle/Mathf.PI, 0);
        //}


        if (!doorLowering && !doorRaising) return;

        if (doorToggleSeconds < 0f) doorToggleSeconds = 0f;
        float yy = doorToggleSeconds;
        if (doorRaising) yy = 1f - doorToggleSeconds;

        if (doorToggleSeconds <= 0f)
        {
            if (doorLowering)
            {
                yy = - 0f;
                doorIsDown = true;
                //baseObj.transform.localScale = new Vector3(0.76f, 0.9f, 0.76f);
                Renderer renderer = baseObj.GetComponent<Renderer>();
                renderer.material = doorMat1;
            }
            
            doorLowering = false;
            doorRaising  = false;
        }
        baseObj.transform.position = new Vector3(x, yy, z);
    }


    public CameraScript.Element getType()
    {
        //if (crateObj != null)
        //{
        //    return CameraScript.Element.CRATE;
        //}
        return type;
    }


    public void setType(CameraScript.Element type)
    {
        this.type = type;
    }

    public void smashCrate(float speed)
    {
        if (type != CameraScript.Element.CRATE) return;
        if (crateObj == null) return;

        CrateScript crate = (CrateScript)crateObj.GetComponent(typeof(CrateScript));
        crate.detonate(speed);
        type = CameraScript.Element.FLOOR;
        crateObj = null;
    }


    public void destroyObjects()
    {
        if (baseObj != null) Object.Destroy(baseObj);
        if (crateObj != null)
        {
            if (!crateObj.CompareTag("Finish")) Object.Destroy(crateObj);
        }
    }
}
