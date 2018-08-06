using UnityEngine;
using System.Collections;

public enum PickupType
{
	NONE,
	OFFENSIVE,
	DEFENSIVE,
	UTILITY,
	UPGRADE
}

public class PlayerPickupManager : MonoBehaviour 
{
	PickupType mCurrentPickup;
	int mUpgradelevel;

	//specific pickup info
	bool mHasArmor;

	//targetting
	float mTargetConeLength = 100;
	float mTargetConeRadius = 50;

	Transform mArmorObject;

	//random pickup stuff
	bool mBusyGivingRandomPickup = false;

	// Use this for initialization
	void Start () 
	{
		mArmorObject = transform.Find("Armor");

		RemoveArmor();
	}

	public void Reset()
	{
		mCurrentPickup = PickupType.NONE;
		mUpgradelevel = 0;

		RemoveArmor();
	}

	public void PickUp(PickupType inPickupType)
	{
		if (Utils.RANDOMPICKUPS())
		{
			if(!mBusyGivingRandomPickup)
				StartCoroutine(RandomPickupRoutine());
		}
		else
		{
			if (Utils.IsLocalPlayer(gameObject, false))
				SoundManager.GetSingleton().Spawn2DSound(SFXType.ItemPickup);

			if (inPickupType == PickupType.UPGRADE)
			{
				float newpitch = 0.75f;
				newpitch += 0.25f * mUpgradelevel;

				if (Utils.IsLocalPlayer(gameObject, false))
					SoundManager.GetSingleton().Spawn2DSound(SFXType.ItemPickupLevelUp).pitch = newpitch;

				mUpgradelevel++;
				if (mUpgradelevel > 3)
					mUpgradelevel = 3;
			}
			else
			{
				mCurrentPickup = inPickupType;
			}
		}
	}

	IEnumerator RandomPickupRoutine()
	{
		mBusyGivingRandomPickup = true;

		for (int i = 0; i < 15; i++)
		{
			GiveRandomPickup(false);
			yield return new WaitForSeconds(0.1f);
		}

		GiveRandomPickup(true);
		mBusyGivingRandomPickup = false;
	}

	void GiveRandomPickup(bool inBasedOnRank)
	{
		mUpgradelevel = Random.Range(0, 4);
		int pickupType = Random.Range(0, 3);

		if (inBasedOnRank)
		{
			float playerPercentage = GetComponent<PlayerPlace>().GetPlacePercentage();

			//we have 4 different states, in which players will get different pickups
			if (playerPercentage <= 0.25f) //totally in the back, players get the strongest pickups
			{
				//Calc the type of pickup. Players dont get defensive pickups
				pickupType = Random.Range(0, 2);

				//pickup level chances 1: 10% 2:20% 3:35% 4:35%
				if (Random.value < 0.7f)
				{
					if (Random.value < 0.5f) mUpgradelevel = 3;
					else mUpgradelevel = 2;
				}
				else
				{
					if (Random.value < 0.66f) mUpgradelevel = 1;
					else mUpgradelevel = 0;
				}
			}
			else if (playerPercentage <= 0.5f) //not entirely in the back, but still entitled to better pickups
			{
				//Calc the type of pickup. Players have a smaller chance to get a defensive pickup than an other type
				if (Random.value < 0.2f)
				{
					pickupType = 2;
				}
				else
				{
					if (Random.value < 0.5f) pickupType = 0;
					else pickupType = 1;
				}

				//pickup level chances 1: 15% 2:35% 3:35% 4:15%
				if (Random.value < 0.7f)
				{
					if (Random.value < 0.5f) mUpgradelevel = 1;
					else mUpgradelevel = 2;
				}
				else
				{
					if (Random.value < 0.5f) mUpgradelevel = 0;
					else mUpgradelevel = 3;
				}
			}
			else if (playerPercentage <= 0.75f) //starting to get to the front, so less stronger pickups
			{
				//Dont modify the type of pickup

				//very small chance of a level 3 pickup, and even chances for a level 1 or 2 pickup
				//pickup level chances 1: 40% 2:30% 3:20% 4:10%
				if (Random.value < 0.4f)
				{
					mUpgradelevel = 0;
				}
				else
				{
					if (Random.value < 0.5f)
					{
						mUpgradelevel = 1;
					}
					else
					{
						if (Random.value < 0.66f) mUpgradelevel = 2;
						else mUpgradelevel = 3;
					}
				}
			}
			else //in the front batch, so this group gets the worst pickups
			{
				//Dont modify the type of pickup

				//Most chance for a low value pickup, but slight chance for a level 2 pickup
				//pickup level chances 1: 70%% 2:30% 3:0% 4:0%
				if (Random.value < 0.70f) mUpgradelevel = 0;
				else mUpgradelevel = 1;
			}
		}

		if (pickupType == 0) mCurrentPickup = PickupType.OFFENSIVE;
		else if (pickupType == 1) mCurrentPickup = PickupType.UTILITY;
		else if (pickupType == 2) mCurrentPickup = PickupType.DEFENSIVE;
	}

	public void UsePickup()
	{
		if (mCurrentPickup == PickupType.NONE)
			return;

		if(mBusyGivingRandomPickup)
			return;

		//see if we need a target to use this pickup, and if we need one and dont have one, dont start the pickup
		if (CurrentPickupUsesTargetting() && !Utils.GetPlayerInConeInFront(gameObject, mTargetConeLength, mTargetConeRadius))
			return;

		//print("Giving player a pickup of type " + mCurrentPickup + " at level " + (mUpgradelevel+1));

		//give the player a pickup based on the current type and ugrade level (global means add on all clients)
		if (mCurrentPickup == PickupType.OFFENSIVE)
		{
			if (mUpgradelevel == 0)			AddPickup("Weapon_MachineGun", true);		//MACHINE GUN, global
			else if (mUpgradelevel == 1)	AddPickup("Weapon_Rockets", false);			//ROCKETS, local
			else if (mUpgradelevel == 2)	AddPickup("Weapon_MachineGunBig", true);	//BIG MACHINE GUN, global
			else							AddPickup("Weapon_Mortar", false);			//HEAT SEEKING ROCKETS, local		
		}
		else if (mCurrentPickup == PickupType.DEFENSIVE)
		{
			if (mUpgradelevel == 0)			AddPickup("Weapon_Mine", false);			//LAND MINE, local
			else if (mUpgradelevel == 1)	AddPickup("Weapon_Pillar", false);			//PILLAR, local
			else if (mUpgradelevel == 2)	AddPickup("Weapon_Armor", true);			//ARMOR, global
			else							AddPickup("Weapon_MagneticField", false);	//MAGNETIC FIELD, local			
		}
		else if (mCurrentPickup == PickupType.UTILITY)
		{
			if (mUpgradelevel == 0)			AddPickup("Weapon_GrapplingHook", false);	//GRAPPLING HOOK, local			
			else if (mUpgradelevel == 1)	AddPickup("Weapon_ReverseControls", false);	//REVERSE CONTROLS, local
			else if (mUpgradelevel == 2)	AddPickup("Weapon_SwapPosition", false);	//SWITCH POSITION, local
			else							AddPickup("Weapon_Booster", true);			//BOOST, global			
		}

		//reset pickup
		mCurrentPickup = PickupType.NONE;
		mUpgradelevel = 0;
	}

	public void AddPickup(string inPickupName, bool inGlobal)
	{
		if(!inGlobal)
			AddPickupComponent(inPickupName);
		else
			networkView.RPC("AddPickupComponent",RPCMode.All, inPickupName);
	}

	[RPC]
	void AddPickupComponent(string inPickupName)
	{
		gameObject.AddComponent(inPickupName);
	}
	
	void Update() //update targetting sphere
	{
		if (Utils.IsLocalPlayer(gameObject, false))
		{
			GameObject targetsphere = GameObject.Find("TargetPlane");

			if (CurrentPickupUsesTargetting())
			{
				GameObject target = Utils.GetPlayerInConeInFront(gameObject, mTargetConeLength, mTargetConeRadius);

				if (target)
				{
					targetsphere.transform.RotateAroundLocal(Vector3.forward, 0.075f);
					targetsphere.transform.position = target.transform.position-target.transform.forward*3+target.transform.up*2;
					targetsphere.renderer.enabled = true;
				}
				else
				{
					targetsphere.renderer.enabled = false;
				}
			}
			else
			{
				targetsphere.renderer.enabled = false;
			}
		}
	}

	/*void OnGUI()
	{
		if (Utils.IsLocalPlayer(gameObject, false))
		{
			GUI.Label(new Rect(5, 150, 500, 100), "Current Pickup: " + GetPickupString());
		}
	}*/

	public string GetPickupString()
	{
		string returner = "";
		
		if (mCurrentPickup == PickupType.OFFENSIVE)
		{
			if (mUpgradelevel == 0)			returner = "Offensive at lvl 1, Machine Gun";
			else if (mUpgradelevel == 1)	returner = "Offensive at lvl 2, Rockets";
			else if (mUpgradelevel == 2)	returner = "Offensive at lvl 3, Big Machine Gun";
			else							returner = "Offensive at lvl 4, Heat-Seeking Mortars";
		}
		else if (mCurrentPickup == PickupType.DEFENSIVE)
		{
			if (mUpgradelevel == 0)			returner = "Defensive at lvl 1, Land Mine";
			else if (mUpgradelevel == 1)	returner = "Defensive at lvl 2, Pillar";
			else if (mUpgradelevel == 2)	returner = "Defensive at lvl 3, Armor";
			else							returner = "Defensive at lvl 4, Magnetic Field";
		}
		else if (mCurrentPickup == PickupType.UTILITY)
		{
			if (mUpgradelevel == 0)			returner = "Utility at lvl 1, Grappling Hook";
			else if (mUpgradelevel == 1)	returner = "Utility at lvl 2, Reverse Controls";
			else if (mUpgradelevel == 2)	returner = "Utility at lvl 3, Switch Position";
			else							returner = "Utility at lvl 4, Boost";
		}

		if (returner != "")
		{
			if (Utils.RANDOMPICKUPS())
				returner = returner.Substring(returner.IndexOf(',')+2);

			return returner;
		}
		else
		{
			if (Utils.RANDOMPICKUPS())
				return "No pickup";

			return "No pick-up at level " + (mUpgradelevel + 1);
		}
	}

	public bool CurrentPickupUsesTargetting()
	{
		if (mCurrentPickup == PickupType.UTILITY && mUpgradelevel < 3) //util 1 - 2 - 3
			return true;
		return false; //rest doesnt use targetting
	}

	public PickupType GetCurrentPickup()
	{
		return mCurrentPickup;
	}

	public int GetCurrentPickupLevel()
	{
		return mUpgradelevel;
	}

	//pickup specific functions
	public void GiveArmor()
	{
		mHasArmor = true;
		mArmorObject.GetComponent<MeshRenderer>().enabled = true;
	}

	public bool GetHit()
	{
		bool gothit = !mHasArmor; //see if we will be hit

		RemoveArmor();
		networkView.RPC("RemoveArmor", RPCMode.Others);

		return gothit;
	}

	[RPC]
	void RemoveArmor()
	{
		mHasArmor = false; //also remove any armor
		mArmorObject.GetComponent<MeshRenderer>().enabled = false;
	}

	public void GetSlowed(float inSpeedMultiplier, bool inOnlyLocally)
	{
		if (inOnlyLocally)
			MultiplySpeed(inSpeedMultiplier);
		else
			networkView.RPC("MultiplySpeed", RPCMode.All, inSpeedMultiplier);
	}

	[RPC]
	void MultiplySpeed(float inSpeedMultiplier)
	{
		rigidbody.velocity *= inSpeedMultiplier;
	}

	[RPC]
	public void SwapWithPlayer(NetworkViewID targetID)
	{
		if (networkView.isMine) //we send to all clients and ask ownership locally, because networkview.owner cannot be trusted!
		{
			GameObject target = Utils.GetPlayerFromID(targetID);

			//shortly disable the collision of the target
			if(!target.networkView.isMine) //if the target is local we dont need to disable the collider
				target.GetComponent<PlayerPhysics>().DisableColliderFor(0.5f);

			//swap this object with the player object with targetID
			Vector3 ourpos = transform.position;
			Quaternion ourrot = transform.rotation;
			Vector3 ourvel = rigidbody.velocity;

			transform.position = target.transform.position;
			transform.rotation = target.transform.rotation;
			rigidbody.velocity = target.rigidbody.velocity;

			target.transform.position = ourpos;
			target.transform.rotation = ourrot;
			target.rigidbody.velocity = ourvel;

			if (Utils.IsLocalPlayer(gameObject, false))
				SoundManager.GetSingleton().Spawn2DSound(SFXType.PositionSwap);

			if (Utils.IsLocalPlayer(target, false))
				SoundManager.GetSingleton().Spawn2DSound(SFXType.PositionSwap);
		}
	}
}


