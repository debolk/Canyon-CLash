using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour
{
	public Transform target;
	public float distance = 6.0f;
	public float yOffset = 4.0f;
	public float xAngleOffect = 20.0f;
	public bool backCamera = false;

	Vector3 mRandomAdd = Vector3.zero;
	Vector3 mTargetRandomAdd = Vector3.zero;
	int newrandomframecount = 0;

	Quaternion origRot;
	Vector3 origPos;

	// Use this for initialization
	void Start()
	{
		origRot = transform.rotation;
		origPos = transform.position;
	}

	public void ResetToOrig()
	{
		target = null;
		transform.rotation = origRot;
		transform.position = origPos;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		newrandomframecount++;
		if (newrandomframecount > 2 && target)
		{
			mTargetRandomAdd = Random.onUnitSphere * target.GetComponent<Rigidbody>().velocity.magnitude * 0.001f; ;
			newrandomframecount = 0;
		}

		mRandomAdd -= (mRandomAdd - mTargetRandomAdd) * 0.5f;

		if (!backCamera && target)
		{
			float vel = target.GetComponent<Rigidbody>().velocity.magnitude;
			float fovmult = Mathf.InverseLerp(70, 160, vel);
			float preferredFov = Mathf.Lerp(80, 130, fovmult);
			GetComponent<Camera>().fov -= (GetComponent<Camera>().fov - preferredFov) * 0.1f;
		}
	}

	void LateUpdate()
	{
		if (target)
		{
			Vector3 forward = target.transform.forward;
			forward.y = 0;
			forward.Normalize();

			if (backCamera)
				forward = -forward;

			Quaternion rotation = Quaternion.LookRotation(forward);

			Vector3 position = (rotation * new Vector3(0.0f, 0.0f, -distance)) + target.position + new Vector3(0, yOffset, 0);

			//position += mRandomAdd; //temporarily removed

			transform.rotation = rotation;
			transform.Rotate(xAngleOffect, 0, 0);

			if (target.GetComponent<Rigidbody>() && target.GetComponent<Rigidbody>().velocity.magnitude > 100 && !backCamera)
				transform.Rotate(Random.onUnitSphere * (target.GetComponent<Rigidbody>().velocity.magnitude - 100) * 0.01f);

			transform.position = position;
		}
		else if(backCamera)
		{
			GetComponent<Camera>().enabled = false;
			GameObject.Find("CamBorder").GetComponent<GUITexture>().enabled = false;
		}

		bool inField = false;
		foreach (GameObject curField in GameObject.FindGameObjectsWithTag("MagneticField"))
		{
			if ((transform.position - curField.transform.position).magnitude < 30 && curField.GetComponent<MagneticField>().IsArmed())
			{
				inField = true;
			}
		}

		if (inField)
		{
			GetComponent<GlowEffect>().enabled = true;
			GetComponent<BlurEffect>().enabled = true;
		}
		else
		{
			GetComponent<GlowEffect>().enabled = false;
			GetComponent<BlurEffect>().enabled = false;
		}
	}

	public void SetTarget(Transform newtransform)
	{
		target = newtransform;
		GetComponent<Camera>().enabled = true;
		GameObject.Find("CamBorder").GetComponent<GUITexture>().enabled = true;
	}
}

