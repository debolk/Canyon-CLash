using UnityEngine;
using System.Collections;

public class Notifier : MonoBehaviour 
{
	int mNumTexts = 5;
	string[] mTexts;

	// Use this for initialization
	void Start () 
	{
		mTexts = new string[mNumTexts];

		for (int i = 0; i < mNumTexts; i++)
		{
			mTexts[i] = "";
		}
	}

	public static Notifier GetSingleton()
	{
		return GameObject.Find("_MAINGAMEOBJECT").GetComponent<Notifier>();
	}

	[RPC]
	void AddText(string inText)
	{
		//shove all texts back
		for (int i = 1; i < mNumTexts; i++)
		{
			mTexts[i-1] = mTexts[i];
		}	

		mTexts[mNumTexts-1] = inText;
	}

	void AddTextNetworked(string inText)
	{
		AddText(inText);
		networkView.RPC("AddText", RPCMode.Others, inText);
	}

	static public void LocalNotify(string inText) //notification only displayed on this client
	{
		GetSingleton().AddText(inText);
	}

	static public void GlobalNotify(string inText) //notification over the network to all players
	{
		GetSingleton().AddTextNetworked(inText);
	}

	void OnGUI()
	{
		int height = mNumTexts * 25;
		GUILayout.BeginArea(new Rect(10, Screen.height - (height + 10), 500, height));
		
		for (int i = 0; i < mNumTexts; i++)
		{
			GUILayout.Label(mTexts[i] as string);
		}

		GUILayout.EndArea();
	}




	//game specific
	static public void BigNotifyPlayer(GameObject inTarget, string inNotifyText)
	{
		if(!Utils.IsABot(inTarget)) //cannot notify a bot
		{
			if (inTarget.networkView.isMine)
			{
				GetSingleton().StartCoroutine(GetSingleton().BigNotify(inNotifyText)); //dont need to send it over internet
			}
			else
			{
				GetSingleton().networkView.RPC("TriggerBigNotify", RPCMode.Others, inTarget.networkView.viewID, inNotifyText);
			}
		}
	}

	static public void BigNotifyLocal(string inNotifyText)
	{
		GetSingleton().StartCoroutine(GetSingleton().BigNotify(inNotifyText));
	}

	[RPC]
	void TriggerBigNotify(NetworkViewID ID, string notifytext)
	{
		GameObject target = Utils.GetPlayerFromID(ID);
		if (target.networkView.isMine)
		{
			StartCoroutine(BigNotify(notifytext));
		}
	}

	IEnumerator BigNotify(string text)
	{
		GameObject.Find("BigNotifyText").GetComponent<GUIText>().text = text;
		yield return new WaitForSeconds(2);
		if(GameObject.Find("BigNotifyText").GetComponent<GUIText>().text == text) //to check if it hasn't been called again in the meantime
			GameObject.Find("BigNotifyText").GetComponent<GUIText>().text = "";
	}
}
