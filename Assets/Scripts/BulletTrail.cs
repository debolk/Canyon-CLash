using UnityEngine;
using System.Collections;

public class BulletTrail : MonoBehaviour
{
	Vector3 mVelocity;
	float mDistLeft;

	void Start () 
	{
		Invoke("Remove", 1.0f);
	}

	void Remove()
	{
		Destroy(gameObject);
	}

	void FixedUpdate () 
	{
		if (mDistLeft > 0)
		{
			transform.position += mVelocity;
			mDistLeft -= mVelocity.magnitude;
		}
	}

	public void SetVel(Vector3 inVel)
	{
		mVelocity = inVel;
	}

	public void SetMaxDist(float inMaxDist)
	{
		mDistLeft = inMaxDist;
	}
}

