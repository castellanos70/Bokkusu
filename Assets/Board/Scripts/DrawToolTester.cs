using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawToolTester : MonoBehaviour {
	Texture2D texture;
	TextureDrawer td;
	int x, y;

	// Use this for initialization
	void Start () {
		texture = new Texture2D((int)gameObject.transform.localScale.x, (int)gameObject.transform.localScale.z);
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		mr.material.mainTexture = texture;

		Debug.Log(texture.width + ", " + texture.height);
		td = new TextureDrawer(texture);
		td.setFillHSV(0, .8f, .5f);
		td.background(Color.white);
		//td.toggleFill();

		td.setStrokeWeight(5);
		td.setStroke(0, .5f, 0);

		td.ellipse(td.getWidth() - 201, td.getHeight() - 101, 201, 101);
		td.quad(50, 50, 20, 200, 120, 100, 75, 0);
		td.rect(td.getWidth()/2, 0, 100, 200);
		td.triangle(200, 250, 50, 300, 100, 150);
		td.line(200, 300, 300, 350);

		td.setStrokeWeight(10);
		td.setStroke(0, 0, 1, 0.2f);

		int width = td.getWidth();
		int height = td.getHeight();
		for (float i = 0; i < 10; i++){
			int x = (int)(width*(i/10));
			td.line(0, 0, x, height);
		}
		//td.triangle(new Vector2[]{new Vector2(100, 150), new Vector2(200, 250), new Vector2(50, 300)});
		td.draw();
		x = 0; y = 0;
	}
	
	// Update is called once per frame
	void Update () {
	}
}
