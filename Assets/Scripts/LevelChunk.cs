using UnityEngine;
using System.Collections;

public class LevelChunk : MonoBehaviour 
{
	public Vector3 ChunkEndPoint = new Vector3(0,0,450);
	public float ChunkRotation = 0;
	public int mChunkNumber = -1;

	Bounds mBounds;

	public Quaternion prevRot = Quaternion.identity;

	// Use this for initialization
	void Awake() 
	{
		transform.parent = GameObject.Find("LevelContainer").transform;
	}

	void Start()
	{

	}

	public void DestroyMe()
	{
		Destroy(this);
	}

	public void OnDrawGizmos()
	{
		//Bounds myBounds = GetBounds();
		//Gizmos.DrawWireCube(myBounds.center, myBounds.size);
	}

	public Bounds GetBounds()
	{
		return GetBounds(false);
	}

	public Bounds GetBounds(bool updateFirst)
	{
		if (mBounds.size.magnitude == 0 || updateFirst)
			UpdateBounds();

		return mBounds;
	}

	void Update()
	{
		if (transform.rotation != prevRot)
		{
			UpdateBounds();
		}

		prevRot = transform.rotation;
	}

	public void UpdateBounds()
	{
		mBounds = new Bounds(transform.position, Vector3.zero);

		foreach (MeshFilter curMesh in GetComponentsInChildren<MeshFilter>())
		{
			foreach(Vector3 curVert in curMesh.mesh.vertices)
			{
				Vector3 worldPos = transform.TransformPoint(curVert);
				mBounds.Encapsulate(worldPos);
			}
		}
	}

	void Rebuild()
	{
		PathUtils.RebuildPaths();
	}

	[RPC]
	void SetChunkNumber(int inNumber)
	{
		mChunkNumber = inNumber;

		if (Network.isClient && mChunkNumber == 0)
		{
			Invoke("Rebuild", 2.0f);
		}
	}
}

