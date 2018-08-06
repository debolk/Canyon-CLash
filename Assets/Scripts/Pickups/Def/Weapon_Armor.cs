using UnityEngine;
using System.Collections;

public class Weapon_Armor : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
		gameObject.GetComponent<PlayerPickupManager>().GiveArmor();

		if (Utils.IsLocalPlayer(gameObject, false))
			SoundManager.GetSingleton().Spawn2DSound(SFXType.Armor);
	}
}
