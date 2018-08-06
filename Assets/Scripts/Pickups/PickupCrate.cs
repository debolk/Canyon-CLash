using UnityEngine;
using System.Collections;

public class PickupCrate : MonoBehaviour 
{
	public PickupType mPickupType = PickupType.UPGRADE;

	bool mEnabled;
	float mStartY;
	float mRespawnTime;

	GameObject mLastCollidedWith;

	// Use this for initialization
	void Start () 
	{
		mStartY = transform.position.y+4;
		mEnabled = true;
		mRespawnTime = 1.5f;

		if (Utils.RANDOMPICKUPS())
		{
			/*if (mPickupType == PickupType.UPGRADE)
			{
				Destroy(gameObject);
			}*/

			tag = "Pickup";

			renderer.material.color = Color.black;
			transform.localScale *= 1.25f;
		}

		transform.localScale *= 1.5f;
		transform.position = new Vector3(transform.position.x, mStartY, transform.position.z);
	}
	
	// Update is called once per frame
	void FixedUpdate() 
	{
		//if(transform.childCount != 0)
		//{
			//Transform rotator = transform.GetChild(0);

			//rotating
			//Profiler.BeginSample("PICKUP_ROTATE");
			//rotator.localRotation *= Quaternion.Euler(0, 1.5f, 0);
			//rotator.transform.Rotate(0, 1.5f, 0);
			//Profiler.EndSample();

			/*Profiler.BeginSample("PICKUP_WOBBLE");
			//wobbling
			Vector3 newpos = rotator.transform.position;
			newpos.y = mStartY + Mathf.Sin(Time.time * 1.5f) * 0.5f;
			rotator.transform.position = newpos;
			Profiler.EndSample();*/
		//}
	}

	void OnTriggerEnter(Collider other)
	{
		//check if it is a player
		if (mEnabled && other.tag == "Player")
		{
			//check if this player is controlled locally
			if (other.networkView.isMine)
			{
				//keep the player that collided in mind
				mLastCollidedWith = other.gameObject;

				//tell the server this player has picked up a pickup
				if (Network.isServer)
					PickedUp(new NetworkMessageInfo());
				else
					networkView.RPC("PickedUp", RPCMode.Server);

				//locally already disable the pickup if we are client
				if(Network.isClient)
					Disable();
			}
		}
	}

	IEnumerator WaitForRespawn()
	{
		yield return new WaitForSeconds(mRespawnTime);

		//enable this pickup everywhere
		networkView.RPC("Enable", RPCMode.All);
	}

	[RPC]
	void Disable()
	{
		mEnabled = false;

		foreach (MeshRenderer curRenderer in GetComponentsInChildren<MeshRenderer>())
		{
			curRenderer.enabled = false;
		}
		
		if(Network.isServer)
			StartCoroutine("WaitForRespawn");
	}

	[RPC]
	void Enable()
	{
		mEnabled = true;

		foreach (MeshRenderer curRenderer in GetComponentsInChildren<MeshRenderer>())
		{
			curRenderer.enabled = true;
		}
	}

	[RPC]
	void PickedUp(NetworkMessageInfo info)
	{
		if (Network.isServer && mEnabled)
		{
			//give the player the actual pickup
			NetworkPlayer sender = info.sender;

			if (info.timestamp != 0) //is the player that called this one of the clients
				networkView.RPC("GivePlayerPickup", sender);
			else //or is it the server itself
				GivePlayerPickup();

			//disable this pickup everywhere
			networkView.RPC("Disable", RPCMode.All);
		}
	}

	[RPC]
	void GivePlayerPickup()
	{
		//print("Gimme pickup! "+mPickupType);

		//call the pickup function on the last collided player
		mLastCollidedWith.GetComponent<PlayerPickupManager>().PickUp(mPickupType);
	}
}
