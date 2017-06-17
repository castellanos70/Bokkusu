using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Splash_Script : MonoBehaviour {


	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		if (Time.realtimeSinceStartup > 8){
			SceneManager.LoadSceneAsync("Arcade", LoadSceneMode.Single);
			SceneManager.SetActiveScene(SceneManager.GetSceneByName("Arcade"));
			SceneManager.UnloadSceneAsync("Splash");
		}
	}
}
