using UnityEngine;
using System.Collections;

public enum HeightAnchor
{
	Top,
	Mid,
	Bot
}

public enum WidthAnchor
{
	Left,
	Mid,
	Right
}

//[ExecuteInEditMode]
public class ScalableGuiTex : MonoBehaviour
{
	public float widthPercentage = 0.5f;
	public float heightPerecentage = 0.5f;
	public bool rectangular = false;

	public WidthAnchor widthAnchor = WidthAnchor.Mid;
	public HeightAnchor heightAnchor = HeightAnchor.Mid;

	void Start () 
	{

	}

	void Update () 
	{
		if (guiTexture)
		{
			float width = Screen.width * widthPercentage;
			float height = Screen.height * heightPerecentage;

			if(rectangular)
				height = width;

			float hWidht = width/2;
			float hHeight = height/2;

			float addedPixelsX = 0;
			float addedPixelsY = 0;

			if (heightAnchor == HeightAnchor.Mid)
				addedPixelsY -= hHeight;
			else if (heightAnchor == HeightAnchor.Top)
				addedPixelsY -= height;
			
			if (widthAnchor == WidthAnchor.Mid)
				addedPixelsX -= hWidht;
			else if (widthAnchor == WidthAnchor.Right)
				addedPixelsX -= width;

			Rect imageRect = new Rect(addedPixelsX, addedPixelsY, width, height);

			guiTexture.pixelInset = imageRect;
		}
	}
}

