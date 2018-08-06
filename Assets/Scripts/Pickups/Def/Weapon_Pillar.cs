using UnityEngine;
using System.Collections;

public class Weapon_Pillar : MonoBehaviour 
{
	// Use this for initialization
	void Start() 
	{
		SpawnPillar(Utils.GetGroundPos(gameObject)); //spawn pillar on the ground
		Destroy(this);
	}

	void SpawnPillar(Vector3 inPosition)
	{
		float currentYRot = transform.rotation.eulerAngles.y;
		Network.Instantiate(Resources.Load("Pillar"), inPosition, Quaternion.Euler(0,currentYRot,0), 3);
	}
}
