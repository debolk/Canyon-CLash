using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
	int mMaxPlayers = 256;
	public Transform playerPrefab;
	bool[] mSpotsTaken;

	void Start()
	{
		mSpotsTaken = new bool[mMaxPlayers];
	}

	void OnNetworkLoadedLevel ()
	{
		if (!Network.isServer)
		{
			networkView.RPC("RequestServerSpawnLocation", RPCMode.Server);
		}
		else
		{
			SpawnAt(GetNextSpawnSpot(), false);
		}
	}

	[RPC]
	void RequestServerSpawnLocation(NetworkMessageInfo info)
	{
		//reply to the sender with a valid spawn location
		networkView.RPC("SpawnAt", info.sender, GetNextSpawnSpot(), false);
	}

	[RPC]
	void SpawnAt(int inSpotNumber, bool inABot)
	{
		//spawn player instance at location
		Vector3 spawnPos = GetSpawnPos(inSpotNumber);
		Transform newplayer = (Transform)Network.Instantiate(playerPrefab, spawnPos, transform.rotation, 2);
		newplayer.gameObject.networkView.RPC("InstantiatePlayer", RPCMode.AllBuffered, inABot, inSpotNumber);
	}

	void SpawnBot()
	{
		//spawn player
		SpawnAt(GetNextSpawnSpot(), true);
	}

	int GetNextSpawnSpot()
	{
		int spawnSpot = -1;

		for (int i = 0; i < mMaxPlayers; i++)
		{
			if (mSpotsTaken[i] == false)
			{
				spawnSpot = i;
				break;
			}
		}

		if (spawnSpot == -1)
			Debug.LogError("Could not find spawn spot for new player");

		mSpotsTaken[spawnSpot] = true;
		return spawnSpot;
	}

	public void RestartGame(int inNumPlayers)
	{
		mSpotsTaken = new bool[mMaxPlayers];

		for (int i = 0; i < inNumPlayers; i++)
		{
			mSpotsTaken[i] = true;
		}
	}

	public Vector3 GetSpawnPos(int inSpotNumber)
	{
		//get starting location
		Vector3 pos = Vector3.zero;
		int numPlayersPerLine = 5;
		float distBetweenSpots = 15.0f;

		int currentXPlayer = inSpotNumber % numPlayersPerLine;

		if (currentXPlayer % 2 == 0) //if even
		{
			pos.x = currentXPlayer / 2 * distBetweenSpots;
		}
		else
		{
			pos.x = -((currentXPlayer + 1) / 2 * distBetweenSpots);
		}

		pos.y = 5;

		int currentZLine = inSpotNumber / numPlayersPerLine;

		pos.z = -100.0f + currentZLine * -20;

		return pos;
	}

	public void OpenUpSpot(int inSpawnSpot)
	{
		mSpotsTaken[inSpawnSpot] = false;
	}

	void OnGUI()
	{
		if (Network.isServer && GUI.Button(new Rect(205, 40, 85, 20), "Spawn Bot"))
		{
			Notifier.GlobalNotify("Bot Spawned.");
			SpawnBot();
		}
	}
}