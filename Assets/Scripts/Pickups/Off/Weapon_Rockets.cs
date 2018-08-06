using UnityEngine;
using System.Collections;

public class Weapon_Rockets : MonoBehaviour
{
	void Start()
	{
		int numrockets = 5;
		int halfrockets = numrockets / 2 + 1;

		//spawn rockets
		for (int i = 1; i <= numrockets; i++)
		{
			GameObject rocket = (GameObject)Network.Instantiate(Resources.Load("Rocket"),	transform.position + 
																							transform.forward * -5 + 
																							transform.up * 2.5f + 
																							transform.right * (-halfrockets + i),	transform.rotation * Quaternion.Euler(-10, (-halfrockets + i) * 3, 0), 3);
			float startvel = rigidbody.velocity.magnitude*Random.Range(0.9f,1.1f);

			rocket.GetComponent<Rocket>().AddStartVelocity(startvel);
			rocket.networkView.RPC("AddStartVelocity", RPCMode.Others, startvel);

			rocket.GetComponent<Rocket>().SetOrigin(gameObject);
		}

		Destroy(this);
	}
}
