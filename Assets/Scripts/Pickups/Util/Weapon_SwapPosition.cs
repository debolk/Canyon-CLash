using UnityEngine;
using System.Collections;

public class Weapon_SwapPosition : MonoBehaviour
{
	bool mUsed = false;

	void Start()
	{

	}

	void Update()
	{
		if(!mUsed)
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
				Notifier.BigNotifyPlayer(gameObject, "Initiating position swap!");

				//notify target
				Notifier.BigNotifyPlayer(target, "Initiating position swap!");

				StartCoroutine(StartPositionSwap(target));
			}
		}
	}

	IEnumerator StartPositionSwap(GameObject target)
	{
		yield return new WaitForSeconds(0.7f);

		//swap current player to other player, and disable other player's collision really shortly
		GetComponent<PlayerPickupManager>().SwapWithPlayer(target.GetComponent<NetworkView>().viewID);
		
		//swap other player to me
		if (!target.GetComponent<NetworkView>().isMine)
			target.GetComponent<NetworkView>().RPC("SwapWithPlayer", RPCMode.Others, GetComponent<NetworkView>().viewID); //we send to all clients and ask ownership locally, because networkview.owner cannot be trusted!

		Destroy(this);
	}

	void OnLevelRestarted()
	{
		Destroy(this);
	}
}

