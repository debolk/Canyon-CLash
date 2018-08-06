using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PathNode : MonoBehaviour
{
	public Transform mPrevNode;
	public Transform mNextNode;
	int mPathNumber;
	float mPathWidth;
	public float mManualPathWidth = 0.0f;

	Color mColor;

	void Start () 
	{
		mColor = Color.red;
		mPathWidth = 60;
	}

	void Update () 
	{
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			if (!mPrevNode && !mNextNode)
				Debug.LogWarning("Dead path node: " + name);

			RecalcPathWidth();

			//draw path width
			Vector3 side = Vector3.Cross(GetDirection(), Vector3.up) * GetWidth();

			Debug.DrawLine(transform.position - side, transform.position + side, Color.black);

			if (mNextNode)
			{
				Vector3 nextside = Vector3.Cross(NextNode().GetDirection(), Vector3.up) * NextNode().GetWidth();

				Debug.DrawLine(transform.position - side, mNextNode.transform.position - nextside, Color.black);
				Debug.DrawLine(transform.position + side, mNextNode.transform.position + nextside, Color.black);
			}
		}
	}

	public float GetPositionOnPath(Vector3 inPosition)
	{
		if (!mNextNode)
			return (1 - PrevNode().GetPositionOnPath(inPosition));

		Vector3 diff = GetDiffWithNext();
		Vector3 localpos = inPosition - transform.position;

		float position = Vector3.Dot(diff.normalized, localpos);
		position = position / (diff.magnitude);

		if(position > 1) position = 1;

		//draw debug line
		if (position > 0)
		{
			Vector3 posonline = transform.position + diff * position;
			Debug.DrawLine(posonline, inPosition);
		}
		////

		return position;
	}

	public Vector3 GetVectorToPath(Vector3 inPosition)
	{
		float posonpath = GetPositionOnPath(inPosition);
		Vector3 actualposontrack = transform.position+posonpath*GetDiffWithNext();

		return actualposontrack - inPosition;
	}

	public Vector3 GetDiffWithNext()
	{
		if (!mNextNode)
		{
			if (mPrevNode)
				return PrevNode().GetDiffWithNext();
			else
				return Vector3.forward;
		}

		Vector3 diff = mNextNode.transform.position - transform.position;
		return diff;
	}

	public Vector3 GetDirection()
	{
		return GetDiffWithNext().normalized;
	}

	public float GetDistanceToNext()
	{
		return GetDiffWithNext().magnitude;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = mColor;
		Gizmos.DrawSphere(transform.position,1.0f);

		Gizmos.color = Color.black;
		if (mPrevNode)
		{
			Gizmos.DrawLine(transform.position, mPrevNode.transform.position);
		}
	}

	public void SetNumber(int inNumber)
	{
		mPathNumber = inNumber;
		name = "PathNode " + mPathNumber;
	}

	public int GetNumber()
	{
		return mPathNumber;
	}

	public float GetWidth()
	{
		if(mManualPathWidth <= 0.0f)
			return mPathWidth;
		return mManualPathWidth;
	}

	public float GetWidthAt(Vector3 inPosition)
	{
		if(!mNextNode)
			return mPathWidth;

		float t = GetPositionOnPath(inPosition);
		float nextwidth = NextNode().GetWidth();

		float width = Mathf.Lerp(GetWidth(), nextwidth, t);
		return width;
	}

	public PathNode NextNode()
	{
		return mNextNode.GetComponent<PathNode>();
	}


	public PathNode PrevNode()
	{
		return mPrevNode.GetComponent<PathNode>();
	}


	public void SetColor(Color inColor)
	{
#if UNITY_EDITOR
		StopAllCoroutines();
		StartCoroutine(SetTempColor(inColor));
#endif
	}

	IEnumerator SetTempColor(Color inColor)
	{
		mColor = inColor;
		yield return new WaitForSeconds(0.5f);
		mColor = Color.red;
	}

	public void RecalcPathWidth()
	{
		RaycastHit hit;
		Vector3 origin = transform.position;
		Vector3 direction = Vector3.Cross(GetDirection(),Vector3.up);	

		int layermask = 1 << 8 | 1 << 2 | 1<<9; //player layer and ignore raycast layer
		layermask = ~layermask; //we trace everything BUT the players

		if (Physics.Raycast(origin, direction, out hit, 150.0f, layermask))
		{
			mPathWidth = hit.distance;
		}
	}
}

