using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	PlayerPhysics mPlayer;
	bool mInControl;
	bool mReversedControls;

	// Use this for initialization
	void Start () 
	{
		mPlayer = gameObject.GetComponent<PlayerPhysics>();
		OnLevelRestarted();
	}

	void OnLevelRestarted()
	{
		mInControl = false;
		mReversedControls = false;
		StopAllCoroutines();
		transform.Find("GRP_InvertedControls/InvertedControls").renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (mInControl)
		{
			if (Input.GetButtonDown("Switch"))
			{
				mPlayer.Switch();
			}

			if (Input.GetButton("FirePowerup") || Input.GetAxisRaw("FirePowerup") < -0.3f)
			{
				gameObject.GetComponent<PlayerPickupManager>().UsePickup();
			}
		}


		/*for (int i = 0; i < 20; i++ )
		{
			if (Input.GetKeyDown("joystick button "+i)) print("Button "+i);
		}*/
	}

	void FixedUpdate()
	{
		if (mInControl)
		{
			if (Input.GetButton("Accelerate"))
				mPlayer.Thrust(1.0f);

			if (Input.GetButton("Brake"))
				mPlayer.Brake();

			//rotation
			if (!mReversedControls)
				mPlayer.Turn(Input.GetAxisRaw("Horizontal"));
			else
				mPlayer.Turn(-Input.GetAxisRaw("Horizontal"));
		}

		//print("vel: "+mPlayer.rigidbody.velocity.magnitude);
	}

	public void AllowControl() { mInControl = true; }
	public void RemoveControl() { mInControl = false; }
	public bool HasControl() { return mInControl; }

	IEnumerator ReverseControls()
	{
		if (Utils.IsLocalPlayer(gameObject, false))
			SoundManager.GetSingleton().Spawn2DSound(SFXType.InvertControls);

		transform.Find("GRP_InvertedControls/InvertedControls").renderer.enabled = true;

		yield return new WaitForSeconds(0.2f);
		mReversedControls = true;
		yield return new WaitForSeconds(5);
		Notifier.BigNotifyPlayer(gameObject, "Controls normal again");
		yield return new WaitForSeconds(0.2f);
		mReversedControls = false;

		transform.Find("GRP_InvertedControls/InvertedControls").renderer.enabled = false;
	}

	[RPC]
	void TriggerReverseControls()
	{
		StartCoroutine(ReverseControls());
	}
}
