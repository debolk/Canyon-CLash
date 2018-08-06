using UnityEngine;
using System.Collections;

public class Weapon_MagneticField : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
		Vector3 pos = gameObject.transform.position - gameObject.transform.forward + new Vector3(0, 1, 0);
		Network.Instantiate(Resources.Load("MagneticField"), pos, Quaternion.identity, 3);
		Destroy(this);
	}
}
