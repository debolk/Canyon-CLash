using UnityEngine;
using System.Collections;

public class MagneticField : MonoBehaviour 
{
	Vector3 mOriginalScale;
	bool armed;

	// Use this for initialization
	void Start()
	{
		mOriginalScale = transform.localScale;
		transform.localScale = new Vector3(0, 0, 0);
		armed = false;

		SoundManager.GetSingleton().Spawn3DSound(SFXType.ItemDrop, gameObject);

		StartCoroutine(WaitForArm());
		StartCoroutine(WaitForRemove());
	}

	IEnumerator WaitForArm()
	{
		yield return new WaitForSeconds(0.75f);
		armed = true;
	}

	[RPC]
	IEnumerator WaitForRemove()
	{
		yield return new WaitForSeconds(15);
		Destroy(gameObject);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		transform.localScale -= (transform.localScale - mOriginalScale) * 0.1f;
		transform.Rotate(0.0f, 0.5f, 0.0f);
	}

	void OnTriggerStay(Collider other)
	{
		if (armed && Utils.IsLocalPlayer(other.gameObject,true)) 
		{
			other.attachedRigidbody.velocity *= 0.975f;
		}
	}

	void OnLevelRestarted()
	{
		Destroy(gameObject);
	}

	public bool IsArmed()
	{
		return armed;
	}
}
