using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour 
{
	static GameObject mLocalPlayer = null;
	static string[] mRandomNames = new string[] {	"Beer",
													"Marvin",
													"Gabrian",
													"Lex",

													"Mata", 
													"Stefano",
													"Jacco",
													"Nils",
													"Brian",
													"Dino",
													"Oliver",
													"Robbie",
													"Jeremiah",
													"Martin",
													"Colin",
													"Peter",

													"Roeny",
													"Richel",
													"Karim",
													"Trevor",
													"Carlo",
													"Freek",
													"Rick",
													"Pim",

													"Daantje",
													"Paulke",
													"Bart",

													"Laurens",
													"Jasper",
													"Jan",
													"Luke",

													"Dries",
													"Diego",
													"Mart",
													"Marius"};

	static public string GetRandomName()
	{
		int spot = Random.Range(0,mRandomNames.Length);
		return mRandomNames[spot];
	}

	static public bool RANDOMPICKUPS()
	{
		return false;
	}

	static public int GetNumPlayers()
	{
		return GameObject.FindGameObjectsWithTag("Player").Length;
	}

	static public Vector3 GetGroundPos(GameObject player)
	{
		Vector3 pos = player.transform.position;
		pos.y -= player.GetComponent<PlayerPhysics>().mDistanceToGround;
		return pos;
	}

	static public GameObject GetPlayerFromID(NetworkViewID playerID) 
	{
		//find the player object that belongs to this networkviewid
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (player.GetComponent<NetworkView>().viewID == playerID)
			{
				return player;
			}
		}

		Debug.Log("Could not find player belonging to networkviewID: "+playerID);
		return null;
	}

	static public GameObject GetPlayerFromNetworkPlayer(NetworkPlayer inNetworkPlayer)
	{
		//find the player object that belongs to this networkviewid
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (player.GetComponent<NetworkView>().owner == inNetworkPlayer)
			{
				return player;
			}
		}

		Debug.Log("Could not find player belonging to Networkplayer: " + inNetworkPlayer);
		return null;
	}


	static public GameObject GetLocalPlayer() //returns the player controlled by this client, will not return bots
	{
		if (mLocalPlayer != null)
			return mLocalPlayer;

		//find the player object that this client controls
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (player.GetComponent<NetworkView>().isMine && player.GetComponent<AIController>() == null)
			{
				mLocalPlayer = player;
				return player;
			}
		}

		//Debug.LogError("Cannot find a local player");
		return null;
	}

	static public bool IsLocalPlayer(GameObject inPlayer, bool inAllowBots) //returns wether the gameObject is a local player, can be filtered to allow bots or not
	{
		if (inAllowBots)
		{
			return inPlayer.GetComponent<NetworkView>().isMine;
		}
		else
		{
			if (inPlayer.GetComponent<NetworkView>().isMine && inPlayer.GetComponent<AIController>() == null)
				return true;
			return false;
		}
	}

	static public bool IsABot(GameObject inPlayer)
	{
		return !(inPlayer.GetComponent<AIController>() == null);
	}

	static public bool IsAPlayer(GameObject inPlayer)
	{
		if (inPlayer != null && inPlayer.tag == "Player")
			return true;
		return false;
	}

	static public GameObject GetPlayerInConeInFront(GameObject inPlayer, float inMaxDist, float inConeRadiusAtMaxDist)
	{
		return GetTaggedObjectInConeInFront(inPlayer, "Player", inMaxDist, inConeRadiusAtMaxDist, new GameObject[0]);
	}

	static public GameObject GetPlayerInConeInFront(GameObject inPlayer, float inMaxDist, float inConeRadiusAtMaxDist, GameObject inIgnoreThis)
	{
		return GetTaggedObjectInConeInFront(inPlayer, "Player", inMaxDist, inConeRadiusAtMaxDist, new GameObject[1] { inIgnoreThis });
	}

	static public GameObject GetPlayerInConeInFront(GameObject inPlayer, float inMaxDist, float inConeRadiusAtMaxDist, GameObject[] inIgnoreList)
	{
		return GetTaggedObjectInConeInFront(inPlayer, "Player", inMaxDist, inConeRadiusAtMaxDist, inIgnoreList);
	}

	static public GameObject GetTaggedObjectInConeInFront(GameObject inPlayer, string inObjectTag, float inMaxDist, float inConeRadiusAtMaxDist)
	{
		return GetTaggedObjectInConeInFront(inPlayer, inObjectTag, inMaxDist, inConeRadiusAtMaxDist, new GameObject[0]);
	}

	static public GameObject GetTaggedObjectInConeInFront(GameObject inPlayer, string inObjectTag, float inMaxDist, float inConeRadiusAtMaxDist, GameObject[] inIgnoreList)
	{
		GameObject returner = null;

		float nearest = 1000000;

		/*//DRAW DEBUG/////////////
		Vector3 pos = inPlayer.transform.position;
		Vector3 midend = pos + inPlayer.transform.forward * inMaxDist;
		Vector3 leftend = midend + inPlayer.transform.right * -inConeRadiusAtMaxDist;
		Vector3 rightend = midend + inPlayer.transform.right * inConeRadiusAtMaxDist;
		Vector3 topend = midend + inPlayer.transform.up * inConeRadiusAtMaxDist;
		Vector3 botend = midend + inPlayer.transform.up * -inConeRadiusAtMaxDist;
		Debug.DrawLine(pos, leftend, Color.white);
		Debug.DrawLine(pos, rightend, Color.white);
		Debug.DrawLine(pos, topend, Color.white);
		Debug.DrawLine(pos, botend, Color.white);
		Debug.DrawLine(leftend, topend, Color.white);
		Debug.DrawLine(topend, rightend, Color.white);
		Debug.DrawLine(rightend, botend, Color.white);
		Debug.DrawLine(botend, leftend, Color.white);
		/////////////////////////*/

		foreach (GameObject other in GameObject.FindGameObjectsWithTag
			(inObjectTag))
		{
			bool ignore = false;

			foreach (GameObject ignorer in inIgnoreList)
			{
				if (ignorer == other)
					ignore = true;
			}

			if (other != inPlayer && !ignore)
			{
				//get position difference
				Vector3 posdiff = other.transform.position - inPlayer.transform.position;

				//project on player forward
				float distance = Vector3.Dot(posdiff, inPlayer.transform.forward);

				//check if other is in front of us and distance is closer than max
				if (distance > 0 && distance < inMaxDist)
				{
					//check if object is in cone
					float sidedist = ((distance * inPlayer.transform.forward) - posdiff).magnitude;

					float pointincone = Mathf.InverseLerp(0, inMaxDist, distance);
					float maxsidedist = Mathf.Lerp(0, inConeRadiusAtMaxDist, pointincone);

					if (sidedist < maxsidedist)
					{
						//check if sideways distance is closer than nearest
						if (sidedist < nearest)
						{
							//this is now our closest target
							nearest = sidedist;
							returner = other;
						}
					}
				}
			}
		}

		return returner;
	}

	static public float GetMaxSpeed(float inAccel, float inFrictionMult, bool inFrictionAfterAccel)
	{
		float percent = (1.0f - inFrictionMult);
		float multiplier = 1.0f / percent;
		float maxspeed = multiplier * inAccel;

		if (inFrictionAfterAccel)
			maxspeed -= inAccel;

		return maxspeed;
	}
}
