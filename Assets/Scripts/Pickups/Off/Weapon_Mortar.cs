using UnityEngine;
using System.Collections;

public class Weapon_Mortar : MonoBehaviour
{
	int currentPosInArray = 0;
	GameObject[] playersToHit;

	void Start () 
	{
		Network.Instantiate(Resources.Load("Mortar"), transform.position, Quaternion.identity, 3);

		playersToHit = new GameObject[Utils.GetNumPlayers()];

		float playerPosOnTrack = GetComponent<PlayerPlace>().GetPosOnTrack();

		//notify all players
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (player.GetComponent<PlayerPlace>().GetPosOnTrack() >= playerPosOnTrack)
			{
				playersToHit[currentPosInArray] = player;
				currentPosInArray++;
				Notifier.BigNotifyPlayer(player, "Incoming mortar!!");
			}
		}

		StartCoroutine(GetReadyToBomb());
	}

	IEnumerator GetReadyToBomb()
	{
		yield return new WaitForSeconds(2);

		//spawn a bomb for each player
		for(int i = 0; i < currentPosInArray; i++)
		{
			GameObject player = playersToHit[i];

			if (player != gameObject)
			{
				//Spawn a rocket!
				GameObject rocket = (GameObject)Network.Instantiate(Resources.Load("Rocket"), player.transform.position + Vector3.up * (50 + Random.Range(-20, 20)), Quaternion.Euler(-90, 0, 0), 3);

				rocket.GetComponent<NetworkView>().RPC("SetAirTarget", RPCMode.All, player.GetComponent<NetworkView>().viewID);

				rocket.GetComponent<Rocket>().SetOrigin(gameObject);
			}
		}

		Destroy(this);
	}

	void OnLevelRestarted()
	{
		Destroy(this);
	}

	void Update () 
	{
	
	}
}

