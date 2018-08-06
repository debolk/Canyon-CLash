using UnityEngine;
using System.Collections;

public class Weapon_MachineGun : MonoBehaviour
{
	public GameObject mGunModel;
	public Object mBulletTrail;

	public float mRandomness = 0.01f;
	public float mTimePerShot = 0.15f;
	public float mModelScale = 1.0f;
	public float mSlowAmount = 0.97f;
	public float mMaxRange = 100.0f;
	public bool mIsBigMachineGun = false;

	float mTimeSinceLastShot = 0.0f;
	float mMaxDist = 200.0f;
	bool mDoubleSound = false;

	Vector3 mDirection;

	// Use this for initialization
	void Start()
	{
		mGunModel = (GameObject)Instantiate(Resources.Load("MachineGun"), Vector3.zero, Quaternion.identity);

		mGunModel.transform.parent = transform;
		mGunModel.transform.localPosition = new Vector3(0,-2.3f*(mModelScale-1),0);
		mGunModel.transform.localRotation = Quaternion.identity;
		mGunModel.transform.localScale = Vector3.one * mModelScale;

		mBulletTrail = Resources.Load("BulletTrail");

		StartCoroutine(WaitForRemove());
	}

	IEnumerator WaitForRemove()
	{
		yield return new WaitForSeconds(5);

		Destroy(mGunModel);
		Destroy(this);
	}

	void OnLevelRestarted()
	{
		Destroy(mGunModel);
		Destroy(this);
	}

	void FixedUpdate()
	{
		//if(Random.value > 0.1f)
			//mGunModel.transform.FindChild("flareholder").FindChild("flare").renderer.enabled = false;

		mTimeSinceLastShot += Time.fixedDeltaTime;
		mMaxDist = 200.0f;

		Transform actualGun = mGunModel.transform.Find("miniGun");
		Vector3 curForward = actualGun.forward;
		curForward -= (curForward - mDirection) * 0.1f;
		actualGun.LookAt(actualGun.position + curForward);

		if(mTimeSinceLastShot > mTimePerShot*0.5f && !mDoubleSound)
		{
			PlaySound();
			mDoubleSound = true;
		}

		//see if we can fire again
		if (mTimeSinceLastShot > mTimePerShot)
		{
			mTimeSinceLastShot -= mTimePerShot;

			//mGunModel.transform.FindChild("flareholder").FindChild("flare").renderer.enabled = true;


			//Shoot stuff!
			RaycastHit hit;
			Vector3 origin = transform.position + transform.up * 2.5f + transform.forward * 5.0f;
			mDirection = transform.forward;

			//see if there is a player in front of us
			GameObject target = Utils.GetPlayerInConeInFront(gameObject, mMaxRange, mMaxRange / 4);
			if (target)
			{
				mDirection = (target.transform.position + Vector3.down * 2) - transform.position;
				mMaxDist = mDirection.magnitude;
				mDirection.Normalize();
			}

			//add randomness
			mDirection += (transform.up * Random.Range(-1.0f, 1.0f) + transform.right * Random.Range(-1.0f, 1.0f)) * mRandomness;

			//cast a ray
			if (Utils.IsLocalPlayer(gameObject, true) && Physics.Raycast(origin, mDirection, out hit))
			{
				GameObject otherObject = hit.transform.gameObject;

				//mGunModel.transform.LookAt(hit.point);

				//see if we hit a player
				if (Utils.IsAPlayer(otherObject))
				{
					//if local player, tell it to slow, if the player is on another client, tell all others to slow this player down
					otherObject.GetComponent<PlayerPickupManager>().GetSlowed(mSlowAmount, Utils.IsLocalPlayer(otherObject, true));

					SoundManager.GetSingleton().Spawn3DSound(SFXType.MachineGunHit, otherObject).volume = 0.6f;

					//Debug.DrawLine(origin, hit.point, Color.green, 1.0f);
				}
				else
				{
					//Debug.DrawLine(origin, hit.point, Color.red, 1.0f);
				}
			}

			//fire a trail
			Vector3 startPos = mGunModel.transform.FindChild("flareholder").position;
			GameObject newTrail = (GameObject)Instantiate(mBulletTrail, startPos + mDirection * 5, Quaternion.identity);
			newTrail.GetComponent<BulletTrail>().SetVel(mDirection.normalized * 10);
			newTrail.GetComponent<BulletTrail>().SetMaxDist(mMaxDist);

			PlaySound();
			mDoubleSound = false;
		}
	}

	void PlaySound()
	{
		//play sound
		SFXType sfx = SFXType.MachineGunFire;
		if (mIsBigMachineGun)
			sfx = SFXType.BigMachineGunFire;
		SoundManager.GetSingleton().Spawn3DSound(sfx, gameObject).volume = 0.6f;
	}
}
