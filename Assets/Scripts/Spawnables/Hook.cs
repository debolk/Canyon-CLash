using UnityEngine;
using System.Collections;

public class Hook : MonoBehaviour
{
	GameObject origin;
	GameObject target;

	float startTime;

	void Start () 
	{
		if(networkView.isMine)
			StartCoroutine(WaitForRemove());

		startTime = Time.realtimeSinceStartup;
	}

	void Update () 
	{
		if (origin && target)
		{
			//graphical
			Vector3 posdiff = target.transform.position - origin.transform.position;
			transform.position = origin.transform.position + posdiff * 0.5f;
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, posdiff.magnitude);
			transform.LookAt(target.transform.position);

			//physics
			Vector3 diff = target.transform.position - origin.transform.position;
			Vector3 force = diff.normalized * 10;

			if (Utils.IsLocalPlayer(origin,true))
			{
				origin.rigidbody.AddForce(force); //pull the player forward
			}

			if (Utils.IsLocalPlayer(target, true))
			{
				target.rigidbody.AddForce(-force*2); //pull the enemy back
			}

			float timeAlive = Time.realtimeSinceStartup-startTime;
			if (networkView.isMine && (diff.magnitude < 20 || diff.magnitude > 150) && timeAlive > 1.0f)
			{
				Network.Destroy(gameObject);
			}
		}
	}

	IEnumerator WaitForRemove()
	{
		yield return new WaitForSeconds(3.5f);
		Network.Destroy(gameObject);
	}

	[RPC]
	void SetTargets(NetworkViewID originID, NetworkViewID targetID)
	{
		//print("target!");
		origin = Utils.GetPlayerFromID(originID);
		target = Utils.GetPlayerFromID(targetID);

		if (Utils.IsLocalPlayer(origin, false))
			SoundManager.GetSingleton().Spawn2DSound(SFXType.GrapplingHit);

		if (Utils.IsLocalPlayer(target, false))
			SoundManager.GetSingleton().Spawn2DSound(SFXType.GrapplingHit);
	}

	void OnLevelRestarted()
	{
		Destroy(gameObject);
	}
}

