using UnityEngine;
using System.Collections;

public class ConnectGui : MonoBehaviour
{
	//DontDestroyOnLoad(this);
	string remoteIP = "127.0.0.1";
	int remotePort = 25000;
	int listenPort = 25000;
	string remoteGUID = "";
	bool useNat = false;
	public string mEnteredName = "CanyonClasher";

	void Awake()
	{
		//if (FindObjectOfType(ConnectGuiMasterServer))
		//this.enabled = false;
	}

	void OnGUI()
	{
		//print fps counter
		GUI.Label(new Rect(Screen.width - 67, Screen.height - 25, 75, 20), "FPS: " + (1.0f/Time.deltaTime).ToString("N1"));

		if (Network.peerType == NetworkPeerType.Disconnected)
		{
			GameObject.Find("MainMenuImage").GetComponent<GUITexture>().enabled = true;

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.Label("Enter name:",GUILayout.MaxWidth(80));
			mEnteredName = GUILayout.TextField(mEnteredName);

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(10);

			//useNat = GUILayout.Toggle(useNat, "Use NAT punchthrough");
			//GUILayout.EndHorizontal();
			//GUILayout.BeginHorizontal();
			//GUILayout.Space(10);

			GUILayout.BeginVertical();
			if (GUILayout.Button("Connect"))
			{
				if (useNat)
				{
					if (remoteGUID != "")
						Debug.LogWarning("Invalid GUID given, must be a valid one as reported by Network.player.guid or returned in a HostData struture from the master server");
					else
						Network.Connect(remoteGUID);
				}
				else
				{
					Network.Connect(remoteIP, remotePort);
				}
			}
			if (GUILayout.Button("Start Server"))
			{
				Network.InitializeServer(32, listenPort, useNat);

				// Notify our objects that the level and the network is ready
				foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
				{
					go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
				}
			}

			if (GUILayout.Button("Quit Game"))
			{
				Application.Quit();
			}

			GUILayout.EndVertical();

			remoteIP = GUILayout.TextField(remoteIP, GUILayout.MinWidth(100));

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			//GUI Sound buttons 
			SoundManager manager = SoundManager.GetSingleton();

			float totalwidth = 585;
			float startx = Screen.width / 2 - totalwidth / 2;

			if (manager.toggleMusic && GUI.Button(new Rect(startx, Screen.height - 35, 90, 25), "Music is on"))
			{
				manager.MuteBGSounds();
			}

			if (!manager.toggleMusic && GUI.Button(new Rect(startx, Screen.height - 35, 90, 25), "Music is off"))
			{
				manager.UnMuteBGSounds();
			}

			if (manager.toggleSFX && GUI.Button(new Rect(startx+100, Screen.height - 35, 120, 25), "Sound FX are on"))
			{
				manager.MuteSFXSounds();
			}

			if (!manager.toggleSFX && GUI.Button(new Rect(startx+100, Screen.height - 35, 120, 25), "Sound FX are off"))
			{
				manager.UnMuteSFXSounds();
			}

			GUI.Box(new Rect(startx+230, Screen.height - 35, 355, 25), "Arrow keys = Steer, Control = Use pick-up, Space = Glider");

		}
		else
		{
			GameObject.Find("MainMenuImage").GetComponent<GUITexture>().enabled = false;

			if (Network.isServer)
			{
				GUI.Label(new Rect(10, 10, 300, 50), "Local IP: " + Network.player.ipAddress);

				if (GUI.Button(new Rect(205, 10, 85, 20), "Disconnect"))
					Network.Disconnect(1000);
			}
			else
			{
				if (GUI.Button(new Rect(10, 10, 85, 20),"Disconnect"))
					Network.Disconnect(1000);
			}
		}

	}

	void OnServerInitialized()
	{
		if (useNat)
			Debug.Log("==> GUID is " + Network.player.guid + ". Use this on clients to connect with NAT punchthrough.");
		Debug.Log("==> Local IP/port is " + Network.player.ipAddress + "/" + Network.player.port + ". Use this on clients to connect directly.");

		Notifier.LocalNotify("Server initialized.");
	}

	void OnPlayerConnected()
	{
		Notifier.LocalNotify("A player connected");
	}

	void OnConnectedToServer()
	{
		// Notify our objects that the level and the network is ready
		foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);	
		}

		Notifier.LocalNotify("Connected to server.");
	}

	void OnDisconnectedFromServer()
	{
		//if (this.enabled != false)
		//	Application.LoadLevel(Application.loadedLevel);
		//else
		//	FindObjectOfType(NetworkLevelLoad).OnDisconnectedFromServer();

		//Destroy players
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			Destroy(player);
		}

		//Destroy level
		GameObject.Find("_MAINGAMEOBJECT").GetComponent<LevelGenerator>().DestroyLevelChunks(true);

		//Destroy pickups
		foreach (GameObject go in FindObjectsOfType(typeof(GameObject)))
		{
			go.SendMessage("OnLevelRestarted", SendMessageOptions.DontRequireReceiver);
		}

		SoundManager.GetSingleton().FadeBGSoundTo(SongType.MAINMENU);

		Notifier.LocalNotify("Disconnected from server.");

		GameObject.Find("MainCamera").GetComponent<FollowCamera>().ResetToOrig();
		GameObject.Find("BackCamera").GetComponent<FollowCamera>().ResetToOrig();
	}


	void OnPlayerDisconnected(NetworkPlayer player)
	{
		GameObject playerObject = Utils.GetPlayerFromNetworkPlayer(player);

		//open up spawn spot
		GetComponent<PlayerSpawner>().OpenUpSpot(playerObject.GetComponent<PlayerNetworkInfo>().GetSpawnSpot()); 

		Notifier.LocalNotify("A player disconnected");
		Debug.Log("Server destroying player");
		Network.RemoveRPCs(player, 0);
		Network.RemoveRPCs(player, 1);
		Network.RemoveRPCs(player, 2);
		Network.DestroyPlayerObjects(player);
	}

}