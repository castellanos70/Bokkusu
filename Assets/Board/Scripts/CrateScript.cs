using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateScript : MonoBehaviour {

    private static int strength = 15;
    private GameObject crateObject;
    public ParticleSystem crateParticles;
    public AudioSource crateAudio;
    private CameraScript cameraScript;
    private Cell[,] grid;
    private bool destroying;

	// Use this for initialization
	void Start () {
        //Debug.Log(crateAudio.isPlaying);
        destroying = false;
    }

    // Update is called once per frame
    void Update () {

        int x = (int) transform.position.x;
        int z = (int) transform.position.z;

        if (grid[x, z].getEntity() != CameraScript.Element.CRATE && !destroying)
        {
            //ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
            //settings.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 0, 1));
            crateParticles.Emit(10);
            //crateAudio.Stop();
            crateAudio.Play();
            Debug.Log(crateAudio.isPlaying);
            GetComponent<Renderer>().enabled = false;

            //crateObject.GetComponent<Renderer>().enabled = false;
            //crateObject.SetActive(false);
            Destroy(crateObject, 1);
            destroying = true;
        }

    }

    public void setBoard(CameraScript cameraScript, Cell[,] myGrid)
    {
        this.cameraScript = cameraScript;
        grid = myGrid;
    }

    public void assignGameObject(GameObject crateObject)
    {
        this.crateObject = crateObject;
        //this.crateParticles = crateParticles;
    }

    public static int getStrength()
    {
        return strength;
    }
}
