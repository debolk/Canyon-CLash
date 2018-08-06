using UnityEngine;
using System.Collections;

public class Hotkeys : MonoBehaviour
{
	static public int lastframepress = 0;

	static public bool Ctrl(KeyCode otherkey)
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			if (Input.GetKeyDown(otherkey) && Time.frameCount != lastframepress)
			{
				lastframepress = Time.frameCount;
				return true;
				
			}
		}
		return false;
	}

	static public bool Alt(KeyCode otherkey)
	{
		if (Input.GetKey(KeyCode.LeftAlt))
		{
			if (Input.GetKeyDown(otherkey) && Time.frameCount != lastframepress)
			{
				lastframepress = Time.frameCount;
				return true;
			}
		}
		return false;
	}
}
