using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateScript : MonoBehaviour {

    private static int strength = 15;
    private GameObject crateObject;
    public ParticleSystem crateParticles;
    //private ParticleSystem particleSystem;
    private CameraScript cameraScript;
    private Cell[,] grid;

	// Use this for initialization
	void Start () {

    }

    // Update is called once per frame
    void Update () {

        int x = (int) transform.position.x;
        int z = (int)transform.position.z;

        if (grid[x, z].getEntity() != CameraScript.Element.CRATE)
        {
            //ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
            //settings.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 0, 1));
            crateParticles.Emit(1);
            GetComponent<Renderer>().enabled = false;
            //crateObject.GetComponent<Renderer>().enabled = false;
            //crateObject.SetActive(false);
            Destroy(crateObject, 1);
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
