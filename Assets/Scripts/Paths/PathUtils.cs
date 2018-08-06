using UnityEngine;
using System.Collections;

public class PathUtils : MonoBehaviour
{
	static public int mNumPathNodes;
	static GameObject mFirstNode;

	static public void RebuildPaths()
	{
		//first see if we need to connect the level chunk starting and ending nodes (client only)
		if (Network.isClient)
		{
			LevelChunk[] allChunks = GameObject.Find("LevelContainer").GetComponentsInChildren<LevelChunk>();
			int numChunks = allChunks.Length;

			foreach (LevelChunk chunk in allChunks)
			{
				int chunkNumber = chunk.mChunkNumber;
				if (chunkNumber != numChunks - 1) //ignore the last chunk
				{
					LevelChunk nextChunk = PathUtils.GetChunkNumberFromArray(allChunks, chunkNumber + 1);
					PathUtils.ConnectChunkPathNodes(chunk.gameObject, nextChunk.gameObject);
				}
			}
		}


		mFirstNode = null;

		//get the first path node
		foreach (GameObject node in GameObject.FindGameObjectsWithTag("PathNode"))
		{
			if (node.GetComponent<PathNode>().mPrevNode == null)
			{
				mFirstNode = node;
			}

			node.GetComponent<PathNode>().RecalcPathWidth();
		}

		if (!mFirstNode)
			Debug.LogError("Could not find first path node");

		//loop over all nodes
		mNumPathNodes = 0;
		PathNode curnode = mFirstNode.GetComponent<PathNode>();

		while (curnode != null)
		{
			//set the node number
			curnode.SetNumber(mNumPathNodes);
			mNumPathNodes++;

			if (curnode.mNextNode) //see if there is a next node
				curnode = curnode.mNextNode.GetComponent<PathNode>();
			else //or if this is the last node
				curnode = null;	
		}
	}

	static public LevelChunk GetChunkNumberFromArray(LevelChunk[] array, int chunkNumber)
	{
		foreach (LevelChunk chunk in array)
		{
			if (chunk.mChunkNumber == chunkNumber)
				return chunk;
		}

		Debug.LogError("Could not find path node with number "+chunkNumber);
		return null;
	}

	static public PathNode GetNearestPathNode(Vector3 position, GameObject savedInfo)
	{
		//savedInfo = null;
		if (savedInfo != null)
		{
			//retrieve saved info
			return savedInfo.GetComponent<PlayerPlace>().GetSavedNearestNode();
		}
		else
		{
			PathNode returner = null;
			float nearestdist = Mathf.Infinity;

			foreach (GameObject node in GameObject.FindGameObjectsWithTag("PathNode"))
			{
				float sqrdist = (position - node.transform.position).sqrMagnitude;

				if (sqrdist < nearestdist)
				{
					nearestdist = sqrdist;
					returner = node.GetComponent<PathNode>();
				}
			}
				
			return returner;
		}
	}

	static public PathNode GetCurrentPathNode(Vector3 inPosition, GameObject savedInfo)
	{
		PathNode nearest = GetNearestPathNode(inPosition,savedInfo);
		float posonnearest = nearest.GetPositionOnPath(inPosition);

		if (posonnearest < 0 && nearest.mPrevNode)
		{
			nearest = nearest.mPrevNode.GetComponent<PathNode>();
		}

		return nearest;
	}

	static public float GetPositionOnTrack(Vector3 inPosition,GameObject savedInfo)
	{
		PathNode current = GetCurrentPathNode(inPosition,savedInfo);
		float posoncurrent = current.GetPositionOnPath(inPosition);
		int currentnumber = current.GetNumber();

		float position = (float)currentnumber + posoncurrent;
		
		return position;
	}

	static public Vector3 CalcDirectionAhead(Vector3 inPosition, float inUnitsAhead, GameObject savedInfo)
	{
		//get current node
		PathNode current = GetCurrentPathNode(inPosition,savedInfo);

		//get position on current node
		float posOnCurrent = current.GetPositionOnPath(inPosition);

		//calc meters till next node
		float currentPathLength = current.GetDistanceToNext();
		float distTillNextNode = (1.0f - posOnCurrent) * currentPathLength;
		inUnitsAhead -= distTillNextNode;

		//loop over next nodes until we have none left or we have traveled our dist
		while (inUnitsAhead > 0 && current.mNextNode)
		{
			current = current.mNextNode.GetComponent<PathNode>();
			inUnitsAhead -= current.GetDistanceToNext();
		}

		//return direction
		return current.GetDirection();
	}

	static public PathNode GetFirstNodeInChunk(GameObject inChunk)
	{
		Transform container = inChunk.transform.Find("PathNodes");

		//get the first path node
		foreach (PathNode node in container.gameObject.GetComponentsInChildren<PathNode>())
		{
			if(node.mPrevNode == null)
			{
				return node;
			}
		}

		//Debug.LogError("Could not find first path node");
		return null;
	}

	static public PathNode GetLastNodeInChunk(GameObject inChunk)
	{
		Transform container = inChunk.transform.Find("PathNodes");

		//get the last path node
		foreach (PathNode node in container.gameObject.GetComponentsInChildren<PathNode>())
		{
			if (node.mNextNode == null)
			{
				return node;
			}
		}

		//Debug.LogError("Could not find last path node");
		return null;
	}

	static public void ConnectChunkPathNodes(GameObject back, GameObject front)
	{
		PathNode prevChunkEnd = PathUtils.GetLastNodeInChunk(back);
		PathNode newChunkStart = PathUtils.GetFirstNodeInChunk(front);

		if (prevChunkEnd == null || newChunkStart == null)
			return;

		prevChunkEnd.mNextNode = newChunkStart.transform;
		newChunkStart.mPrevNode = prevChunkEnd.transform;
	}
}

