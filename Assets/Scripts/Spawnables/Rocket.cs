using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour
{
	GameObject mOrigin;
	public GameObject mTarget;

	float mVelocity = 0.0f;
	float mAccel = 8.0f;
	bool mExploded = false;
	bool mAirStrike = false;

	void Start () 
	{
		//Notifier.LocalNotify("maxspeed: " + Utils.GetMaxSpeed(mAccel, 0.96f, true));
		SoundManager.GetSingleton().Spawn3DSound(SFXType.RocketLaunch, gameObject).volume = 0.75f;

		if (SoundManager.GetSingleton().toggleSFX)
			GetComponent<AudioSource>().Play();
	}

	void FixedUpdate() 
	{
		//accelleration
		mVelocity += mAccel;
		mVelocity *= 0.96f;

		//check if we will hit a wall (airstrikes ignore this)
		if (GetComponent<NetworkView>().isMine && !mAirStrike)
		{
			RaycastHit hit;

			int layermask = 1 << 8 | 1 << 2 | 1 << 9; //player layer and ignore raycast layer
			layermask = ~layermask; //we trace everything BUT the players

			if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layermask))
			{
				if (hit.distance < (mVelocity * Time.fixedDeltaTime))
				{
					//spawn an explosion
					EffectManager.TriggerExplosion(transform.position, false);

					Network.Destroy(gameObject);
					return;
				}
			}
		}

		if (!mAirStrike) //see if we are regular aim mode, or airstrike mode
		{
			//add velocity
			transform.position += transform.forward * mVelocity * Time.fixedDeltaTime;

			//get a target
			if (GetComponent<NetworkView>().isMine && !mTarget)
			{
				int numIgnorers = 1;

				//ignore all players that already have a rocket aimed at them
				GameObject[] allRockets = GameObject.FindGameObjectsWithTag("Rocket");

				foreach (GameObject rocket in allRockets)
				{
					if (rocket.GetComponent<Rocket>().mTarget)
					{
						numIgnorers++;
					}
				}

				GameObject[] ignorers = new GameObject[numIgnorers];
				ignorers[0] = mOrigin;
				numIgnorers = 1;

				foreach (GameObject rocket in allRockets)
				{
					Rocket curRocket = rocket.GetComponent<Rocket>();
					if (curRocket.mTarget)
					{
						ignorers[numIgnorers] = curRocket.mTarget;
						numIgnorers++;
					}
				}

				//find a target
				GameObject target = Utils.GetPlayerInConeInFront(gameObject, 125, 150, ignorers);

				if (target)
				{
					mTarget = target;
					GetComponent<NetworkView>().RPC("SetTarget", RPCMode.Others, target.GetComponent<NetworkView>().viewID);
				}
			}

			//look at target
			Vector3 lookinat = transform.forward;
			Vector3 wanttolookat = lookinat;
			wanttolookat.y = 0;

			if (mTarget)
				wanttolookat = (mTarget.transform.position + Vector3.up * 2) - transform.position;

			float rotspeed = 0.0005f * mVelocity;

			Vector3 lookatdir = Vector3.RotateTowards(lookinat, wanttolookat.normalized, rotspeed, 1000);
			transform.LookAt(transform.position + lookatdir);
		}
		else
		{
			//this means we must drop down from the sky
			Vector3 pos = transform.position;
			pos.x = mTarget.transform.position.x;
			pos.z = mTarget.transform.position.z;
			pos.y -= mVelocity * Time.fixedDeltaTime*0.75f;

			transform.position = pos;
		}

		//see if we hit our target
		if(mTarget && GetComponent<NetworkView>().isMine)
		{
			float dist = (mTarget.transform.position-transform.position).magnitude;
			if (dist < 6.0f && !mExploded )
			{
				mExploded = true;

				if (mTarget.GetComponent<PlayerPickupManager>().GetHit()) //see if the player is protected by armor
				{
					//tell this player to get hit
					if (mTarget.GetComponent<NetworkView>().isMine)
						Explosion(mTarget.GetComponent<NetworkView>().viewID);
					else
						GetComponent<NetworkView>().RPC("Explosion", RPCMode.Others, mTarget.GetComponent<NetworkView>().viewID);

					//spawn an explosion
					EffectManager.TriggerExplosion(transform.position, false);
				}
				else
				{
					Network.Destroy(gameObject);
					return;
				}
			}
		}
	}

	public void SetOrigin(GameObject origin)
	{
		mOrigin = origin;
	}

	[RPC]
	void SetTarget(NetworkViewID targetID)
	{
		mTarget = Utils.GetPlayerFromID(targetID);
	}

	[RPC]
	void SetAirTarget(NetworkViewID targetID)
	{
		mTarget = Utils.GetPlayerFromID(targetID);
		transform.LookAt(transform.position + Vector3.down);
		mAirStrike = true;
	}

	[RPC]
	public void AddStartVelocity(float added)
	{
		mVelocity += added;
	}


	[RPC]
	void Explosion(NetworkViewID targetID)
	{
		GameObject target = Utils.GetPlayerFromID(targetID);

		if (target.GetComponent<NetworkView>().isMine)
		{
			//reduce player velocity
			target.GetComponent<Rigidbody>().velocity *= 0.5f;

			//send the player upward
			target.GetComponent<Rigidbody>().velocity += new Vector3(0, 20, 0);

			//spin the player
			target.GetComponent<Rigidbody>().AddTorque(Random.onUnitSphere * 1000);

			Network.Destroy(gameObject);
		}
	}

	void OnLevelRestarted()
	{
		Destroy(gameObject);
	}
}

