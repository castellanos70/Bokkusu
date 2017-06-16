using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateScript : MonoBehaviour {

    private GameObject crateObject;
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
            crateObject.SetActive(false);
            Destroy(crateObject);
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
    }
}
