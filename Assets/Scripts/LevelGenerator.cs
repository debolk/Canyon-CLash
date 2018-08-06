using UnityEngine;
using System.Collections;

public class LevelGenerator : MonoBehaviour 
{
	public Transform[]	mLevelChunks;
	public Transform	mFinishChunk;

	int			mNumChunks = 25;
	float		mCurrentYAngle = 0;
	Vector3		mCurrentPos = Vector3.zero;
	GameObject	mPrevPlacedChunk;

	// Use this for initialization
	void Start () 
	{

	}

	void OnNetworkLoadedLevel()
	{
		if (Network.isServer)
		{
			GenerateLevel();
		}
	}

	void OnGUI()
	{
		if (Network.isServer && GUI.Button(new Rect(115, 40, 80, 20), "New Level"))
		{
			Notifier.GlobalNotify("Generating new level...");

			foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
			{
				Notifier.BigNotifyPlayer(player, "Generating new level...");
			}

			DestroyLevelChunks(false);
			//networkView.RPC("DestroyLevelChunks", RPCMode.Others);
			GenerateLevel();
		}
	}

	IEnumerator GenerateLevel()
	{
		StartCoroutine(GenerateLevel(Random.seed));
		return null;
	}

	IEnumerator GenerateLevel(int seed)
	{
		Random.seed = seed;

		//first we will calculate which nodes we will be placing
		mCurrentPos = Vector3.zero;
		mCurrentYAngle = 0;
		GameObject[] allLevelChunks = new GameObject[mNumChunks];
		int[] chunkNumbers = new int[mNumChunks];
		int currentChunk = 0;
		int tries = 0;

		//we will loop until we have all chunks filled in
		while (allLevelChunks[mNumChunks - 1] == null)
		{
			//we save these for later use
			Vector3 prevPos = mCurrentPos;
			float prevRot = mCurrentYAngle;

			//get a random chunk, but make sure it's not the same as the previous
			int chunkNumber;

			int prevChunkNumber = -1;
			if (currentChunk != 0) prevChunkNumber = chunkNumbers[currentChunk - 1];

			do { chunkNumber = Random.Range(0, mLevelChunks.Length); }
			while (chunkNumber == prevChunkNumber);

			//print("trying a chunk of type " + chunkNumber);

			//place this chunk (so we can calculate its bounds)
			Transform chunkTransform = mLevelChunks[chunkNumber]; 
			if(currentChunk == mNumChunks-1) //finishline as last chunk
				chunkTransform = mFinishChunk;

			GameObject newChunk = AddChunk(chunkTransform, true, 0);
			newChunk.transform.parent = GameObject.Find("LevelContainer").transform;
			
			//before collision checks, we make rotate the entire level to 0 degrees, to ensure a better matching bounding box (DISABLED FOR NOW)
			//GameObject.Find("LevelContainer").transform.Rotate(0, -prevRot, 0);

			Bounds chunkBounds = newChunk.GetComponent<LevelChunk>().GetBounds(true);

			//see if this not collides with any previous chunks (we ignore the one right before this, since our start usually collides with that end)
			bool collides = false;
			for (int i = currentChunk - 2; i >= 0; i--)
			{
				Bounds otherBounds = allLevelChunks[i].GetComponent<LevelChunk>().GetBounds();

				if (chunkBounds.Intersects(otherBounds))
				{
					//print("chunk has COLLISION with chunk number: " + (i+1));
					collides = true;
				}			
			}

			//get rid of the temporary rotation (DISABLED FOR NOW)
			//GameObject.Find("LevelContainer").transform.rotation = Quaternion.identity;

			//if we have no collision, add the chunk to the array
			if (!collides)
			{
				//print("chunk has no collission, PLACING CHUNK NUMBER "+ (currentChunk+1));
				chunkNumbers[currentChunk] = chunkNumber;
				allLevelChunks[currentChunk] = newChunk;
				currentChunk++;
				tries = 0;

				Notifier.LocalNotify("Generating level chunks... ["+currentChunk+"/"+mNumChunks+"]");
				Notifier.BigNotifyLocal("Generating level... " + (int)(((float)currentChunk/(float)mNumChunks)*100.0f) + "%");
			}
			else //if we DO have collision, remove this chunk and try again
			{
				Destroy(newChunk);

				mCurrentPos = prevPos;
				mCurrentYAngle = prevRot;
			}

			//safety built in to prevent infinite loop
			tries++;
			if (tries >= 5)
			{
				Notifier.LocalNotify("Path intersecting, restarting");
				Notifier.BigNotifyLocal("Path intersecting, restarting");

				print("Path will cross inevitabely, starting over!");

				DestroyLevelChunks(true);

				//reset vars
				mCurrentPos = Vector3.zero;
				mCurrentYAngle = 0;
				allLevelChunks = new GameObject[mNumChunks];
				chunkNumbers = new int[mNumChunks];
				currentChunk = 0;
				tries = 0;

				//wait a bit, so the user can see the notification
				yield return new WaitForSeconds(1.0f);
			}

			yield return null;
		}

		Notifier.LocalNotify("Finalizing level generation...");
		Notifier.BigNotifyLocal("Finalizing level...");
		yield return null;

		//first, remove all nodes we created in the process
		DestroyLevelChunks(true);

		//now we will place the actual nodes
		mCurrentPos = Vector3.zero;
		mCurrentYAngle = 0;
		mPrevPlacedChunk = null;
		
		for (int i = 0; i < mNumChunks; i++)
		{
			//place new chunk
			Transform chunkTransform = mLevelChunks[chunkNumbers[i]];
			if (i == mNumChunks - 1) //make sure we place the finish at the end
				chunkTransform = mFinishChunk;

			GameObject newChunk = AddChunk(chunkTransform, false, i);

			//connect path nodes
			if (mPrevPlacedChunk)
			{
				PathUtils.ConnectChunkPathNodes(mPrevPlacedChunk, newChunk);
			}

			mPrevPlacedChunk = newChunk;
		}

		PathUtils.RebuildPaths();

		Notifier.LocalNotify("Level generation done!");
		Notifier.BigNotifyLocal("Level generation done!");
	}

	GameObject AddChunk(Transform chunk, bool local, int chunkNumber)
	{
		Quaternion rot = Quaternion.Euler(0, mCurrentYAngle, 0);

		Transform newchunk;

		if (local)
			newchunk = (Transform)Instantiate(chunk, mCurrentPos, rot);
		else
		{
			newchunk = (Transform)Network.Instantiate(chunk, mCurrentPos, rot, 1);
			newchunk.networkView.RPC("SetChunkNumber", RPCMode.OthersBuffered ,chunkNumber);
		}

		mCurrentPos += rot * chunk.GetComponent<LevelChunk>().ChunkEndPoint;
		mCurrentYAngle += chunk.GetComponent<LevelChunk>().ChunkRotation;

		return newchunk.gameObject;
	}

	[RPC]
	public void DestroyLevelChunks(bool local)
	{
		foreach (LevelChunk go in GameObject.Find("LevelContainer").GetComponentsInChildren<LevelChunk>())
		{
			if (local)
			{
				Destroy(go.gameObject);
			}
			else
			{
				Network.RemoveRPCs(go.networkView.viewID);
				Network.Destroy(go.gameObject);
			}
		}

		//if(Network.isServer)
			//Network.RemoveRPCsInGroup(1);
	}
}
