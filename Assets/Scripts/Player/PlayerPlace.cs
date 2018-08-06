using UnityEngine;
using System.Collections;

public class PlayerPlace : MonoBehaviour
{
	float mCurrentPosOnTrack;
	int mCurrentPlace;
	int mNumPlayers;
	float mPlacePercentage;
	float mAddedExtraPosOnTrack; //added to lock player place after finish

	static float curExtraPos = 100000;

	PathNode mNearestNode;

	void Start () 
	{
		mCurrentPlace = 0;
		mPlacePercentage = 0;
		mAddedExtraPosOnTrack = 0;
		mNearestNode = null;
	}

	void OnLevelRestarted()
	{
		mAddedExtraPosOnTrack = 0;
		curExtraPos = 100000;
	}

	void FixedUpdate()
	{
		mNearestNode = PathUtils.GetNearestPathNode(transform.position, null);
		mCurrentPosOnTrack = PathUtils.GetPositionOnTrack(transform.position,gameObject);
	}

	void Update() 
	{
		if (Utils.IsLocalPlayer(gameObject, false))
		{
			GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
			mNumPlayers = allPlayers.Length;

			for (int i = 0; i < mNumPlayers; i++) //do this as much times as their are players
			{
				float currentHighestPos = -1000;
				GameObject currentHighestPlayer = null;
				int currentHighestID = 0;

				for (int j = 0; j < mNumPlayers; j++) //loop over all players still left in the array
				{
					GameObject curPlayer = allPlayers[j];

					if (curPlayer != null && GetPosOnTrack(curPlayer) > currentHighestPos) //check if this player is the highest left in the array
					{
						currentHighestPos = GetPosOnTrack(curPlayer);
						currentHighestPlayer = curPlayer;
						currentHighestID = j;
					}
				}

				//now tell the current player their place, and remove this player from the array
				currentHighestPlayer.GetComponent<PlayerPlace>().SetPlace(i + 1);
				allPlayers[currentHighestID] = null;
			}

			//keep track of the first, last and average spots of the players
			float minPlaceOnTrack = 10000;
			float maxPlaceOnTrack = -10000;
			//float totalPlaceOnTrack = 0;

			allPlayers = GameObject.FindGameObjectsWithTag("Player");
			for (int i = 0; i < mNumPlayers; i++)
			{	
				float curSpot = GetPosOnTrack(allPlayers[i]);
				//totalPlaceOnTrack += curSpot;

				if (curSpot < minPlaceOnTrack)
					minPlaceOnTrack = curSpot;

				if (curSpot > maxPlaceOnTrack)
					maxPlaceOnTrack = curSpot;
			}

			//float averagePlaceOnTrack = totalPlaceOnTrack / mNumPlayers;
			//print("min: " + minPlaceOnTrack + " max: " + maxPlaceOnTrack);

			for (int i = 0; i < mNumPlayers; i++)
			{
				GameObject curPlayer = allPlayers[i];
				curPlayer.GetComponent<PlayerPlace>().RecalcPlacePercentage(minPlaceOnTrack, maxPlaceOnTrack);
			}
		}
	}


	void OnGUI()
	{
		/*
		if (Utils.IsLocalPlayer(gameObject, false))
		{
			GUI.Label(new Rect(5, 170, 500, 100), "Current pos: " + GetPlace() + "/" + GetNumPlayers() + "  percentage: " + GetPlacePercentage());
		}
		 */
	}

	public float GetPosOnTrack(GameObject inPlayer)
	{
		return inPlayer.GetComponent<PlayerPlace>().GetPosOnTrack();
	}

	public float GetPosOnTrack()
	{
		return mCurrentPosOnTrack+mAddedExtraPosOnTrack;
	}

	public int GetPlace()
	{
		return mCurrentPlace;
	}

	public int GetNumPlayers()
	{
		return mNumPlayers;
	}

	public void RecalcPlacePercentage(float min, float max)
	{
		if (min != max)
			mPlacePercentage = Mathf.InverseLerp(min, max, mCurrentPosOnTrack);
		else
			mPlacePercentage = 0.5f;
	}

	public float GetPlacePercentage()
	{
		//return Mathf.InverseLerp(mNumPlayers, 1.0f, mCurrentPlace);
		return mPlacePercentage;
	}

	public void SetPlace(int inPlace)
	{
		mCurrentPlace = inPlace;
	}

	public void LockPlayerPlace()
	{
		curExtraPos -= 1000;
		mAddedExtraPosOnTrack = curExtraPos;
		GetComponent<NetworkView>().RPC("AddExtraTrackPos", RPCMode.Others, mAddedExtraPosOnTrack);
	}

	public float GetTotalRacePercentage()
	{
		float percent = Mathf.InverseLerp(0.0f, (float)(PathUtils.mNumPathNodes-2), GetPosOnTrack()+1);
		percent = Mathf.Clamp(percent, 0.0f, 1.0f);
		return percent;
	}

	public PathNode GetSavedNearestNode()
	{
		return mNearestNode;
	}

	[RPC]
	void AddExtraTrackPos(float extra)
	{
		mAddedExtraPosOnTrack = extra;
	}
}

