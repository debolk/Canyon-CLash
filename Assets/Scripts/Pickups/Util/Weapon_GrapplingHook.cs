using UnityEngine;
using System.Collections;

public class Weapon_GrapplingHook: MonoBehaviour
{
	bool mUsed = false;

	void Start()
	{

	}

	void FixedUpdate()
	{
		if (!mUsed)
		{
			//see if there is a player in front of us
			GameObject target = Utils.GetPlayerInConeInFront(gameObject, 100, 50);

			if (target)
			{
				if (!target.GetComponent<PlayerPickupManager>().GetHit()) //see if we miss because of armor
				{
					Notifier.BigNotifyPlayer(gameObject, "Blocked by armor!");
					Destroy(this);
					return;
				}

				mUsed = true;

				//notify player
				Notifier.BigNotifyPlayer(gameObject, "Player hooked!");

				//notify target
				Notifier.BigNotifyPlayer(target, "You got hooked!");

				//graphical representation
				GameObject hook = (GameObject)Network.Instantiate(Resources.Load("Hook"), Vector3.zero, Quaternion.identity, 3);
				hook.GetComponent<NetworkView>().RPC("SetTargets", RPCMode.All, GetComponent<NetworkView>().viewID, target.GetComponent<NetworkView>().viewID);

				Destroy(this);
			}
		}
	}
}

