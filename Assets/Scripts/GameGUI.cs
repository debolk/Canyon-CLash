using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour
{
	public Texture mRaceBar_Start;
	public Texture mRaceBar_Finish;
	public Texture mRaceBar_Bar;
	public Texture mRaceBar_PlayerVehicle;
	public Texture mRaceBar_OtherVehicle;

	public Texture[] mPickupIcons;

	PickupType mPickupLastFrame;
	int mPickupLevelLastFrame = -1;

	void Start () 
	{

	}

	void OnGUI() 
	{
		GameObject player = Utils.GetLocalPlayer();

		if (player)
		{
			float scrH = Screen.height;
			float scrW = Screen.width;

			//draw race ranking bar
			float distFromSide = 275;
			float distFromBot = 75;

			GUI.DrawTexture(new Rect(distFromSide, scrH - distFromBot - mRaceBar_Bar.height * 0.5f, scrW - distFromSide * 2, mRaceBar_Bar.height), mRaceBar_Bar);
			DrawTexCenteredAt(mRaceBar_Start, new Vector2(distFromSide, scrH - distFromBot));
			DrawTexCenteredAt(mRaceBar_Finish, new Vector2(scrW - distFromSide, scrH - distFromBot));

			float totalWidth = scrW - distFromSide * 2;

			float ownpercentage = player.GetComponent<PlayerPlace>().GetTotalRacePercentage();
			DrawTexCenteredAt(mRaceBar_PlayerVehicle, new Vector2(distFromSide + totalWidth * ownpercentage, scrH - distFromBot - 3));

			foreach (GameObject curPlayer in GameObject.FindGameObjectsWithTag("Player"))
			{
				if (!Utils.IsLocalPlayer(curPlayer, false))
				{
					float percentage = curPlayer.GetComponent<PlayerPlace>().GetTotalRacePercentage();
					DrawTexCenteredAt(mRaceBar_OtherVehicle, new Vector2(distFromSide + totalWidth * percentage, scrH - distFromBot - 3));
				}
			}
		}
	}

	void Update()
	{	
		GameObject player = Utils.GetLocalPlayer();

		if (player)
		{
			//draw current pickup
			int curLevel = player.GetComponent<PlayerPickupManager>().GetCurrentPickupLevel();
			PickupType curType = player.GetComponent<PlayerPickupManager>().GetCurrentPickup();

			if (curType != mPickupLastFrame || curLevel != mPickupLevelLastFrame)
			{
				int pickupImg = curLevel + (int)curType * 4;
				Texture pickupTex = mPickupIcons[pickupImg]; //needs to be fixed as soon as more pickup images come

				GameObject.Find("CurrentPickup").guiTexture.enabled = true;
				GameObject.Find("CurrentPickup").guiTexture.texture = pickupTex;
			}

			mPickupLastFrame = curType;
			mPickupLevelLastFrame = curLevel;

			//draw current spot
			PlayerPlace playerPlace = player.GetComponent<PlayerPlace>();
			int spot = playerPlace.GetPlace();
			int spottenth = (int)(spot / 10);
			int spotfirst = spot - spottenth * 10;

			string addedText = "th";

			if (spottenth != 1)
			{
				if (spotfirst == 1)
					addedText = "st";
				else if (spotfirst == 2)
					addedText = "nd";
				else if (spotfirst == 3)
					addedText = "rd";
			}

			GameObject.Find("CurrentSpotText").guiText.text = spot + addedText + " / " + playerPlace.GetNumPlayers();
		}
		else
		{
			GameObject.Find("CurrentSpotText").guiText.text = "";
			GameObject.Find("CurrentPickup").guiTexture.enabled = false;
			mPickupLevelLastFrame = -1;
		}
	}

	void DrawTexCenteredAt(Texture inTex, Vector2 inPos)
	{
		float left = inPos.x - inTex.width*0.5f;
		float top = inPos.y - inTex.height*0.5f;
		Rect texRect = new Rect(left, top, inTex.width, inTex.height);
		GUI.DrawTexture(texRect, inTex);
	}
}

