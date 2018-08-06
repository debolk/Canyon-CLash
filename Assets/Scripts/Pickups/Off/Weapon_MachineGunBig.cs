using UnityEngine;
using System.Collections;

public class Weapon_MachineGunBig : Weapon_MachineGun
{
	void Awake()
	{
		mModelScale = 2.0f;
		mTimePerShot = 0.2f;
		mSlowAmount = 0.87f;
		mMaxRange = 150.0f;
		mIsBigMachineGun = true;
	}
}
