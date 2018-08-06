using UnityEngine;
using System.Collections;

public class Mine : MonoBehaviour 
{
	Vector3 mOriginalScale;
	bool armed;
	float mStartY;

	// Use this for initialization
	void Start () 
	{
		mStartY = transform.position.y + 1.0f;

		mOriginalScale = transform.localScale;
		transform.localScale = new Vector3(0, 0, 0);
		armed = false;

		SoundManager.GetSingleton().Spawn3DSound(SFXType.ItemDrop, gameObject);

		StartCoroutine(WaitForArm());
	}

	IEnumerator WaitForArm()
	{
		yield return new WaitForSeconds(0.1f);
		armed = true;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		transform.localScale -= (transform.localScale - mOriginalScale) * 0.1f;

		//rotating
		transform.Rotate(0, 0.75f, 0);

		//wobbling
		Vector3 newpos = transform.position;
		newpos.y = mStartY + Mathf.Sin(Time.time * 3.0f) * 0.5f;
		transform.position = newpos;
	}

	void OnTriggerEnter(Collider other)
	{
		if (armed && Utils.IsLocalPlayer(other.gameObject, true)) //we only let the local player collide, this way no client can decide for another client that they hit the mine
		{
			if (other.gameObject.GetComponent<PlayerPickupManager>().GetHit()) //see if the player is protected by armor
			{
				//reduce player velocity
				other.attachedRigidbody.velocity *= 0.7f;

				//send the player upward
				other.attachedRigidbody.velocity += new Vector3(0, 15, 0);

				//send the player flying back
				Vector3 posdiff = other.transform.position - transform.position;
				posdiff.y = 0;
				posdiff.Normalize();
				other.attachedRigidbody.velocity += posdiff * 20;
			}

			if (Network.isServer)
				GotHit();
			else
				networkView.RPC("GotHit", RPCMode.Server);
		}
	}

	[RPC]
	void GotHit()
	{
		EffectManager.TriggerExplosion(transform.position+Vector3.down, false);

		//destroy the mine
		if (gameObject)
			Network.Destroy(gameObject);
	}

	void OnLevelRestarted()
	{
		Destroy(gameObject);
	}
}