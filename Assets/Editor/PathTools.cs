using UnityEngine;
using UnityEditor;
using System.Collections;

public class PathTools : MonoBehaviour
{
	[MenuItem("Path Tools/Connect 2 nodes")]
	static void ConnectNodes()
	{
		if (Selection.gameObjects.Length != 2)
		{
			Debug.LogError("Please make sure you select 2 objects");
			return;
		}

		PathNode node1 = Selection.gameObjects[0].GetComponent<PathNode>();
		PathNode node2 = Selection.gameObjects[1].GetComponent<PathNode>();

		if (!node1.mNextNode && node1.mPrevNode)
		{
			node1.mNextNode = node2.transform;
			node2.mPrevNode = node1.transform;
		}
		else if (!node2.mNextNode && node2.mPrevNode)
		{
			node2.mNextNode = node1.transform;
			node1.mPrevNode = node2.transform;
		}
	}

}

