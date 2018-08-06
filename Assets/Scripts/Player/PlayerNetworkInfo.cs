using UnityEngine;
using System.Collections;

//NETWORK GROUPS:
//0 - misc
//1 - level chunks
//2 - players
//3 - pickups

public class PlayerNetworkInfo : MonoBehaviour 
{
	//overall game related
	string mName;
	bool mReady;
	bool mGameStarted;
	bool mIsWinner;
	bool mGameOver;

	bool mHasFinished;

	float mRaceTime;

	//actual player related
	int mSpawnSpot = 0;

	// Use this for initialization
	void Start () 
	{
		mReady = false;
		mGameStarted = false;
		mIsWinner = false;
		mGameOver = false;
		mRaceTime = 0;
	}

	bool IsReady() { return mReady; }

	[RPC]
	void InstantiatePlayer(bool isABot, int inSpawnSpot)
	{
		//see what kind of controller we need to add
		if (isABot)
		{
			gameObject.AddComponent<AIController>();
			GetComponent<AIController>().enabled = false;
		}
		else
		{
			gameObject.AddComponent<PlayerController>();
			GetComponent<PlayerController>().enabled = false;
		}

		// This is our own player
		if (Utils.IsLocalPlayer(gameObject,true))
		{
			GetComponent<NetworkRigidbody>().enabled = false;
			GetComponent<PlayerPhysics>().enabled = true;
			rigidbody.useGravity = true;

			string myName = "";

			if (!Utils.IsABot(gameObject))
			{
				GameObject.Find("MainCamera").GetComponent<FollowCamera>().SetTarget(transform);
				GameObject.Find("BackCamera").GetComponent<FollowCamera>().SetTarget(transform);

				GetComponent<PlayerController>().enabled = true;

				//get the name from the connectGUI
				myName = GameObject.Find("_MAINGAMEOBJECT").GetComponent<ConnectGui>().mEnteredName;
			}
			else
			{
				GetComponent<AIController>().enabled = true;
				myName = "[BOT] " + Utils.GetRandomName();
				name += " (BOT)";
			}
			
			//notify others of the name
			SetName(myName);
			networkView.RPC("SetName", RPCMode.OthersBuffered, myName);

			//assign a random texture
			int texture = Random.Range(1, 7);
			networkView.RPC("SetTexture", RPCMode.AllBuffered, texture);
		}
		// This is just some remote controlled player, don't execute direct
		// user input on this
		else
		{
			name += " (Remote)";
			GetComponent<NetworkRigidbody>().enabled = true;
			GetComponent<PlayerPhysics>().enabled = false;
			rigidbody.useGravity = false;

			//we will get the name from the client that owns this player
		}

		//keep track of the respawn location
		mSpawnSpot = inSpawnSpot;
	}

	// Update is called once per frame
	void Update () 
	{
		if (networkView.isMine) //only execute on the player object that belongs to this network player
		{
			if ((Input.GetButton("GetReady") || Utils.IsABot(gameObject))&& !mReady)
			{
				PlayerReady();
				networkView.RPC("PlayerReady", RPCMode.OthersBuffered);
			}

			//code only the server executes
			if (Network.isServer && !mGameStarted && !Utils.IsABot(gameObject))
			{
				//check if all players are ready
				bool allready = true;
				foreach (PlayerNetworkInfo networkinfo in FindObjectsOfType(typeof(PlayerNetworkInfo)))
				{
					if (!networkinfo.IsReady())
						allready = false;
				}

				//if all of the players are ready, we can start the game
				if (allready)
				{
					Notifier.GlobalNotify("Game Started!");

					//tell all players to start
					StartGame();
					networkView.RPC("StartGame", RPCMode.Others);
				}
			}
		}

		bool addTime = false;
		if (Utils.IsABot(gameObject))
			addTime = GetComponent<AIController>().HasControl();
		else
			addTime = GetComponent<PlayerController>().HasControl();

		if (addTime)
			mRaceTime += Time.deltaTime;
	}

	void OnGUI()
	{
		if (!Utils.IsABot(gameObject))
		{
			if (Network.isServer && (GUI.Button(new Rect(5, 40, 100, 20), "Restart Game")))
			{
				Notifier.GlobalNotify("Game Restarted.");
				RestartGame();
			}

			if (networkView.isMine)
			{
				float addedY = 0;

				if (Network.isServer)
					addedY = 30;

				if (!mReady)
					GUI.Label(new Rect(10, 40 + addedY, 500, 100), "You are not ready, Press 'R' to get ready");
				else if (!GetComponent<PlayerController>().HasControl())
					GUI.Label(new Rect(10, 40 + addedY, 500, 100), "You are ready, waiting for other players");

				if (mGameOver)
				{
					if (mIsWinner)
						GUI.Label(new Rect(150, 200, 500, 100), "You win!");
					else
						GUI.Label(new Rect(150, 200, 500, 100), "You lose!");
				}

				if (mHasFinished)
				{
					int numFinished = 0;
					foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
					{
						if (player.GetComponent<PlayerNetworkInfo>().HasFinished())
						{
							numFinished++;
						}
					}

					GUI.Box(new Rect(Screen.width / 2 - 155, 125, 305, numFinished * 20 + 10), "");

					foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
					{
						if (player.GetComponent<PlayerNetworkInfo>().HasFinished())
						{
							DrawRanking(player);
						}
					}
				}
			}
		}
	}

	void DrawRanking(GameObject player)
	{
		int rank = player.GetComponent<PlayerPlace>().GetPlace();
		string myName = player.GetComponent<PlayerNetworkInfo>().GetMyName();
		float raceTime = player.GetComponent<PlayerNetworkInfo>().GetRaceTime();

		int seconds = (int)raceTime;
		float rest = raceTime - (float)seconds;
		int hundreds = (int)(rest * 100.0f);
		int minutes = seconds / 60;
		seconds -= minutes * 60;

		string raceTimeString = minutes + ":" + seconds.ToString("00") + ":" + hundreds.ToString("00");

		GUI.Label(new Rect(Screen.width / 2 - 150, 110 + rank * 20, 500, 100), rank.ToString());
		GUI.Label(new Rect(Screen.width / 2 - 125, 110 + rank * 20, 500, 100), myName.ToString());
		GUI.Label(new Rect(Screen.width / 2 + 100, 110 + rank * 20, 500, 100), raceTimeString);
	}

	void RestartPlayer(int inSpawnSpot)
	{
		mReady = false;
		mIsWinner = false;
		mGameOver = false;
		mGameStarted = false;
		mHasFinished = false;
		mSpawnSpot = inSpawnSpot;
		mRaceTime = 0;

		GetComponent<PlayerPhysics>().Reset();
		GetComponent<PlayerPickupManager>().Reset();

		transform.position = GameObject.Find("_MAINGAMEOBJECT").GetComponent<PlayerSpawner>().GetSpawnPos(mSpawnSpot);
		transform.rotation = Quaternion.identity;
		rigidbody.velocity = Vector3.zero;
		rigidbody.angularVelocity = Vector3.zero;
	}

	void OnTriggerEnter(Collider other)
	{
		if (Network.isServer && !mHasFinished && other.name == "finishline")
		{
			mHasFinished = true;

			GetComponent<PlayerPlace>().LockPlayerPlace();

			networkView.RPC("CrossedFinish", RPCMode.All, mRaceTime);
		}
	}

	public int GetSpawnSpot() { return mSpawnSpot; }

	[RPC]
	void SetName(string inName)
	{
		mName = inName;

		//change name on 3d text
		gameObject.GetComponentInChildren<TextMesh>().text = mName;

		Notifier.LocalNotify("Player " + inName + " arrived");
	}

	[RPC]
	void PlayerReady()
	{
		mReady = true;
	}

	[RPC]
	void StartGame()
	{
		mGameStarted = true;
		StartCoroutine(StartGameRoutine());

		SoundManager.GetSingleton().FadeBGSoundTo(SongType.INGAME);
	}

	IEnumerator StartGameRoutine()
	{
		Notifier.BigNotifyLocal("Get ready!");
		yield return new WaitForSeconds(0.75f);
		Notifier.BigNotifyLocal("3..");
		SoundManager.GetSingleton().Spawn2DSound(SFXType.ReadySet,0.5f);
		yield return new WaitForSeconds(0.75f);
		Notifier.BigNotifyLocal("2..");
		SoundManager.GetSingleton().Spawn2DSound(SFXType.ReadySet, 0.5f);
		yield return new WaitForSeconds(0.75f);
		Notifier.BigNotifyLocal("1..");
		SoundManager.GetSingleton().Spawn2DSound(SFXType.ReadySet, 0.5f);
		yield return new WaitForSeconds(0.75f);
		Notifier.BigNotifyLocal("GO!!");
		SoundManager.GetSingleton().Spawn2DSound(SFXType.Go);

		foreach (PlayerController controller in FindObjectsOfType(typeof(PlayerController)))
		{
			controller.AllowControl();
		}

		foreach (AIController controller in FindObjectsOfType(typeof(AIController)))
		{
			controller.AllowControl();
		}
	}

	void RestartGame()
	{
		int numPlayers = GetComponent<PlayerPlace>().GetNumPlayers();

		GameObject.Find("_MAINGAMEOBJECT").GetComponent<PlayerSpawner>().RestartGame(numPlayers);

		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			int playerSpot = player.GetComponent<PlayerPlace>().GetPlace();
			int spawnSpot = numPlayers - playerSpot;

			player.networkView.RPC("RestartGameRPC", RPCMode.All, spawnSpot);
		}
	}

	[RPC]
	void RestartGameRPC(int inSpawnSpot)
	{
		PathUtils.RebuildPaths();
		RestartPlayer(inSpawnSpot);

		if (Utils.IsLocalPlayer(gameObject,false))
		{
			//Send ALL objects the message that the game has restarted
			foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
			{
				go.SendMessage("OnLevelRestarted", SendMessageOptions.DontRequireReceiver);
			}
		}

		//Remove all spawned objects
		if(Network.isServer)
			Network.RemoveRPCsInGroup(3);
	}

	[RPC]
	void Winner()
	{
		mIsWinner = true;
		foreach (PlayerNetworkInfo info in FindObjectsOfType(typeof(PlayerNetworkInfo)))
		{
			info.mGameOver = true;
		}
	}

	[RPC]
	void CrossedFinish(float raceTime)
	{
		mHasFinished = true;

		if(Utils.IsABot(gameObject))
			GetComponent<AIController>().RemoveControl();
		else
			GetComponent<PlayerController>().RemoveControl();

		mRaceTime = raceTime;

		if (Utils.IsLocalPlayer(gameObject, false))
		{
			Notifier.BigNotifyPlayer(gameObject,"Finished!");
		}
	}

	[RPC]
	void SetTexture(int inTexture)
	{
		string textureString = "VehicleTextures/playerVehicle_0" + inTexture;
		transform.Find("playerVehicle").renderer.material.mainTexture = (Texture)Resources.Load(textureString);
	}

	public string GetMyName() { return mName; }
	public float GetRaceTime() { return mRaceTime; }
	public bool HasFinished() { return mHasFinished; }
}
