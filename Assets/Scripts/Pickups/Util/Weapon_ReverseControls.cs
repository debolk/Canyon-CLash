using UnityEngine;
using System.Collections;

public class Weapon_ReverseControls : MonoBehaviour
{
	void Start () 
	{

	}

	void Update () 
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

			if(!Utils.IsABot(target))
			{
				target.GetComponent<NetworkView>().RPC("TriggerReverseControls", RPCMode.All);
			}
			else
			{
				target.GetComponent<NetworkView>().RPC("TriggerAIReverseControls", RPCMode.All);
			}

			//notify player
			Notifier.BigNotifyPlayer(gameObject, "Target controls reversed!");

			//notify target
			Notifier.BigNotifyPlayer(target, "Controls reversed!");

			if (Utils.IsLocalPlayer(gameObject, false))
				SoundManager.GetSingleton().Spawn2DSound(SFXType.InvertControls);

			Destroy(this); //remove script
		}
	}
}

