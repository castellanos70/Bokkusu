using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Created by Ben Matthews 7/10/2017
 * Triangle Rendering by Joel Catellanos
 * 
 * Info:
 * A Class for creating a drawing utility for a given texture
 * Contains basic drawing functions such as rect, ellipse, triangle and quad
 * 
 * Useful for animating and drawing on unity textures
 * 
 * TODO:
 * 	- implement polygon drawing (see https://en.wikipedia.org/wiki/Polygon_triangulation)
 *  - implement stroke line caps
 *  - implement anti-aliasing (probably requires converting to all floats)
 *  - implement drawing matrix for rotations and affine scaling
 * 
 * 
 * !Note!:
 * Unity uses a scale of 0-1 for all color values. Using values larger than 1 will result
 * in incorrect alpha blending behavior.
*/

public class TextureDrawer {
	private Texture2D texture;
	private Color[] colors;

	private int width, height;

	private float strokeWeight = 1;
	private Color fill, stroke;

	private bool fillShape = true;
	private bool strokeShape = true;

	public TextureDrawer(Texture2D texture){
		this.texture = texture;
		width = texture.width;
		height = texture.height;
		colors = new Color[width*height];
		clear();
		Debug.Log(colors.Length);

		fill = new Color(255, 255, 255);
		stroke = new Color(0, 0, 0);
	}

	//sets all colors to black
	public void clear(){
        Color c = new Color(0, 0, 0);
        for (var i = 0; i < colors.Length; i++){
            colors[i] = c;
		}
	}

	public void setFill(Color fill){this.fill = fill;}
	public void setFill(float r, float g, float b){this.fill = new Color(r, g, b);}
	public void setFill(float r, float g, float b, float a){this.fill = new Color(r, g, b, a);}
	public void setFillHSV(float h, float s, float v){this.fill = Color.HSVToRGB(h, s, v);}

	public void setStroke(Color stroke){this.stroke = stroke;}
	public void setStroke(float r, float g, float b){this.stroke = new Color(r, g, b);}
	public void setStroke(float r, float g, float b, float a){this.stroke = new Color(r, g, b, a);}
	public void setStrokeHSV(float h, float s, float v){this.stroke = Color.HSVToRGB(h, s, v);}

	public void toggleFill(){fillShape ^= true;}
	public void toggleStroke(){strokeShape ^= true;}
	public void setStrokeWeight(int s){strokeWeight = s;}

	//################
	//Basic Utilities:
	//################
	public int getWidth(){return width;}
	public int getHeight(){return height;}
	private bool inBounds(int x, int y){return (x >= 0 && y >= 0 && x < width && y < height);}

	private void setPixel(float x, float y, Color color){setPixel((int)Mathf.Round(x), (int)Mathf.Round(y), color);}

	public void setPixel(int x, int y, Color color){
		if (!inBounds(x, y)) return;
		Color c = colors[y*width + x];
		if (color.a == 1) colors[y*width + x] = color;
	    else colors[y*width + x] = blend(c, color);
	}


	//#########
	//Blending:
	//#########
	private Color blend(Color c1, Color c2){
		Color output = new Color(0, 0, 0, 1);
		float a2 = 1.0f - c2.a;
		output.r = (c2.r*c2.a) + (c1.r*a2);
		output.g = (c2.g*c2.a) + (c1.g*a2);
		output.b = (c2.b*c2.a) + (c1.b*a2);
		return output;
	}


	//###########
	//Background:
	//###########

	public void background(float grayScale){background(new Color(grayScale, grayScale, grayScale));}
	public void background(float grayScale, float alpha){background(new Color(grayScale, grayScale, grayScale, alpha));}
	public void background(float r, float g, float b){background(new Color(r, g, b));}
	public void background(float r, float g, float b, float alpha){background(new Color(r, g, b, alpha));}

	public void background(Color color){
		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				setPixel(x, y, color);
			}
		}
	}


	//###########
	//Line Tools:
	//###########

	//naive 1px width line drawing algorithm
	public void line(int x1, int y1, int x2, int y2){

		if (x2 < x1){
			int temp = x2;
			x2 = x1;
			x1 = temp;

			temp = y2;
			y2 = y1;
			y1 = temp;
		}

		if (strokeWeight != 1){
			lineWithStroke(x1, y1, x2, y2);
			return;
		}

		int w = x2 - x1 ;
		int h = y2 - y1 ;
		int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0 ;
		if (w<0) dx1 = -1 ; else if (w>0) dx1 = 1 ;
		if (h<0) dy1 = -1 ; else if (h>0) dy1 = 1 ;
		if (w<0) dx2 = -1 ; else if (w>0) dx2 = 1 ;
		int longest = Mathf.Abs(w) ;
		int shortest = Mathf.Abs(h) ;
		if (!(longest>shortest)) {
			longest = Mathf.Abs(h) ;
			shortest = Mathf.Abs(w) ;
			if (h<0) dy2 = - 1 ; else if (h>0) dy2 = 1 ;
			dx2 = 0 ;            
		}
		int numerator = longest >> 1 ;
		for (int i=0;i<=longest;i++) {
			setPixel(x1,y1, stroke) ;
			numerator += shortest ;
			if (!(numerator<longest)) {
				numerator -= longest ;
				x1 += dx1 ;
				y1 += dy1 ;
			} else {
				x1 += dx2 ;
				y1 += dy2 ;
			}
		}
	}

	//TODO fix artifacts with this traingle/quad rendering
	private void lineWithStroke(int x1, int y1, int x2, int y2){
		int dx = x2 - x1;
		int dy = y2 - y1;
		float angle = Mathf.Atan2(dy, dx);
		float a1 = angle + Mathf.PI/2;
		float a2 = angle - Mathf.PI/2;
		float halfStroke = strokeWeight/2.0f;
		Vector2 v1 = new Vector2(x1 + Mathf.Cos(a1)*halfStroke, y1 + Mathf.Sin(a1)*halfStroke);
		Vector2 v2 = new Vector2(x1 + Mathf.Cos(a2)*halfStroke, y1 + Mathf.Sin(a2)*halfStroke);
		Vector2 v3 = new Vector2(v2.x + dx, v2.y + dy);
		Vector2 v4 = new Vector2(v1.x + dx, v1.y + dy);

		bool tempStrokeShape = strokeShape;
		bool tempFillShape = fillShape;
		Color tempFill = fill;

		fill = stroke;

		strokeShape = false;
		fillShape = true;

		quad(new Vector2[]{v1, v2, v3, v4});

		fill = tempFill;
		strokeShape = tempStrokeShape;
		fillShape = tempFillShape;
	}

	private void drawHorzLine(int x1, int x2, int y)
	{
		if (x2 < x1){
			int temp = x1;
			x1 = x2;
			x2 = temp;
		}
		for (int x = x1; x <= x2; x++)
		{
			setPixel(x, y, fill);
		}
	}


	//#########
	//Rectangle
	//#########
	public void rect(int x1, int y1, int width, int height){
		if (x1 < 0){
			width += 0 + x1;
			x1 = 0;
		}
		if (y1 < 0){
			height += 0 + y1;
			y1 = 0;
		}
		if (x1 >= this.width){
			width += this.width - x1;
			x1 = this.width;
		}
		if (y1 >= this.height){
			height += this.height - y1;
			y1 = this.height;
		}
		width = Mathf.Clamp(width, 0, this.width);
		height = Mathf.Clamp(height, 0, this.height);

		if (fillShape){
			for (int x = 0; x < width; x++){
				for (int y = 0; y < height; y++){
					setPixel(x + x1, y + y1, fill);
				}
			}
		}

		//TODO needs optimization and lineCaps implemented
		if (strokeShape){
			int halfWeight = (int)(strokeWeight/2);
			Color tempFill = fill;
			bool tempStrokeShape = strokeShape;
			strokeShape = false;
			fill = stroke;

			rect(x1 - halfWeight, y1 - halfWeight, (int)strokeWeight, height + (int)strokeWeight);
			rect(x1 + width - halfWeight, y1 - halfWeight, (int)strokeWeight, height + (int)strokeWeight);
			rect(x1 + halfWeight, y1 - halfWeight, width - (int)strokeWeight, (int)strokeWeight);
			rect(x1 + halfWeight, y1 + height - halfWeight, width - (int)strokeWeight, (int)strokeWeight);

			//also an option:
			//line(x1, y1, x1+width, y1);
			//line(x1, y1, x1, y1+height);
			//line(x1+width, y1, x1+width, y1+height);
			//line(x1, y1+height, x1+width, y1+height);

			strokeShape = tempStrokeShape;
			fill = tempFill;
		}
	}

	//#########
	//Ellipse
	//#########
	public void ellipse(int x1, int y1, int width, int height){
		float halfWidth = width/2.0f;
		float halfHeight = height/2.0f;

		if (fillShape){
			float thresh = halfWidth*halfWidth*halfHeight*halfHeight;
			for (int x = 0; x <= halfWidth; x++){
				for (int y = 0; y <= halfHeight; y++){
					float dx = halfHeight - y;
					float dy = halfWidth - x;
					if (dx*dx*halfWidth*halfWidth + dy*dy*halfHeight*halfHeight <= thresh){
						setPixel(x1 + x, y1 + y, fill);
						setPixel(x1 + width - x, y1 + y, fill);
						setPixel(x1 + x, y1 + height-y, fill);
						setPixel(x1 + width - x, y1 + height - y, fill);
					}
				}
			}
		}

		//TODO needs significant computation improvement
		if (strokeShape){
			float outerWidth = (width + strokeWeight)/2.0f;
			float innerWidth = (width - strokeWeight)/2.0f;
			float outerHeight = (height + strokeWeight)/2.0f;
			float innerHeight = (height - strokeWeight)/2.0f;

			x1 -= (int)strokeWeight/2;
			y1 -= (int)strokeWeight/2;

			float outerThresh = outerWidth*outerWidth*outerHeight*outerHeight;
			float innerThresh = innerWidth*innerWidth*innerHeight*innerHeight;

			for (int x = 0; x <= outerWidth; x++){
				for (int y = 0; y <= outerHeight; y++){
					float dx = outerHeight - y;
					float dy = outerWidth - x;
					float value = dx*dx*outerWidth*outerWidth + dy*dy*outerHeight*outerHeight;
					float value2 = dx*dx*innerWidth*innerWidth + dy*dy*innerHeight*innerHeight;
					if (value <= outerThresh && value2 >= innerThresh){
						setPixel(x1 + x, y1 + y, stroke);
						setPixel(x1 + outerWidth*2 - x, y1 + y, stroke);
						setPixel(x1 + x, y1 + outerHeight*2-y, stroke);
						setPixel(x1 + outerWidth*2 - x, y1 + outerHeight*2 - y, stroke);
					}
				}
			}
		}
	}


	//###########
	//Quad Tools:
	//###########
	public void quad(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4){
		Vector2[] vectors = new Vector2[4];
		vectors[0] = new Vector2(x1, y1);
		vectors[1] = new Vector2(x2, y2);
		vectors[2] = new Vector2(x3, y3);
		vectors[3] = new Vector2(x4, y4);
		quad(vectors);
	}

	public void quad(Vector2[] corners){
		if (corners.Length != 4) return;
		Vector2[] vectors1 = {corners[0], corners[1], corners[2]};
		Vector2[] vectors2 = {corners[2], corners[3], corners[0]};

		bool tempStrokeShape = strokeShape;
		strokeShape = false;

		if (fillShape){
			triangle(vectors1);
			triangle(vectors2);
		}

		strokeShape = tempStrokeShape;

		if (strokeShape){
			for (int i = 0; i < 4; i++){
				Vector2 v1 = corners[i];
				Vector2 v2 = corners[(i+1)%4];
				line((int)v1.x, (int)v1.y, (int)v2.x, (int)v2.y);
			}
		}
	}


	//###############
	//Triangle Tools:
	//###############
	public void triangle(int x1, int y1, int x2, int y2, int x3, int y3){
		Vector2[] vectors = new Vector2[3];
		vectors[0] = new Vector2(x1, y1);
		vectors[1] = new Vector2(x2, y2);
		vectors[2] = new Vector2(x3, y3);
		triangle(vectors);
	}

	public void triangle(Vector2[] v)
	{
		if (v.Length != 3) return;
			if (fillShape){
			sortVerticesAscendingByY(v);
			if (v[1].y == v[2].y) fillBottomFlatTriangle(v[0], v[1], v[2]);
			else if (v[0].y == v[1].y) fillTopFlatTriangle(v[0], v[1], v[2]);
			else
			{
				Vector2 v4 = new Vector2();
				v4.x = (int)Mathf.Round(v[0].x + ((float)(v[1].y - v[0].y) / (float)(v[2].y - v[0].y)) * (v[2].x - v[0].x));
				v4.y=v[1].y;
				fillBottomFlatTriangle(v[0], v[1], v4);
				fillTopFlatTriangle(v[1], v4, v[2]);
			}
		}

		if (strokeShape){
			for (int i = 0; i < 3; i++){
				Vector2 v1 = v[i];
				Vector2 v2 = v[(i+1)%3];
				line((int)v1.x, (int)v1.y, (int)v2.x, (int)v2.y);
			}
		}
	}
		
	private void fillBottomFlatTriangle(Vector2 v1, Vector2 v2, Vector2 v3)
	{
		float invslope1 = (v2.x - v1.x) / (v2.y - v1.y);
		float invslope2 = (v3.x - v1.x) / (v3.y - v1.y);

		float curx1 = v1.x;
		float curx2 = v1.x;

		for (int y = (int)v1.y; y <= v2.y; y++)
		{
			drawHorzLine((int)Mathf.Round(curx1), (int)Mathf.Round(curx2), y);
			curx1 += invslope1;
			curx2 += invslope2;
		}
	}
		
	private void fillTopFlatTriangle(Vector2 v1, Vector2 v2, Vector2 v3)
	{
		float invslope1 = (v3.x - v1.x) / (v3.y - v1.y);
		float invslope2 = (v3.x - v2.x) / (v3.y - v2.y);

		float curx1 = v3.x;
		float curx2 = v3.x;

		for (int y = (int)v3.y; y > v1.y; y--)
		{
			drawHorzLine((int)Mathf.Round(curx1), (int)Mathf.Round(curx2), y);
			curx1 -= invslope1;
			curx2 -= invslope2;
		}
	}

	private void sortVerticesAscendingByY(Vector2[] v)
	{
		if (v[2].y < v[1].y) swap(v, 1,2);
		if (v[1].y < v[0].y) swap(v, 0, 1);
		if (v[2].y < v[1].y) swap(v, 1, 2);
	}

	private void swap(Vector2[] v, int i, int k)
	{
		Vector2 tmp = v[i];
		v[i] = v[k];
		v[k] = tmp;
	}



	/*
	 * Applys the Color array buffer to the texture
	*/
	public void draw(){
		texture.SetPixels(colors);
		texture.Apply();
	}
}
