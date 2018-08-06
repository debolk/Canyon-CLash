using UnityEngine;
using System.Collections;

public class PlayerPhysics : MonoBehaviour 
{
	float mSuspensionHeight = 2.0f;
	float mSuspensionStiffness = 75;
	float mSuspensionDamping = 7;

	float mOnGroundPitchSmooth = 0.1f;
	float mInAirPitchSmooth = 0.02f;

	float mThrustForce = 20.0f;
	float mThrustMaxTether = 1.0f; //maximum amount the thrust force can go up and down as a result of tether
	float mBoostForce = 12.0f; //boost is added to thrust

	float mAirFrictionMultiplier = 0.9963f;

	float mTurnSpeed = 0.0f;
	float mTurnAccel = 0.15f;
	float mMaxTurnSpeedAtStandStill = 2.0f;
	float mMaxTurnSpeedAtFullSpeed = 1.5f;
	float mMaxSpeedForTurning = 50.0f;

	float mLiftForce = 0.35f;

	public float mDistanceToGround;
	Vector3 mGroundNormal;
	bool mOnGround;

	bool mInHoverBoardMode = true;
	bool mSkidding = false;

	float mTurnRoll = 0;
	float mOrigThrust;

    void Start()
    {
		mOrigThrust = mThrustForce;
    }

	public void Reset()
	{
		SwitchToBoard();
		mTurnRoll = 0;
		mTurnSpeed = 0.0f;
	}

	void FixedUpdate()
	{
		UpdateGroundInfo();

		//we will always apply hoverboard code, we will only add the glider mechanics if those are enabled
		ApplyHoverBoard();
		if (!mInHoverBoardMode)
		{
			ApplyGlider();
		}

		UpdateTether();

		if (mTurnSpeed > 0)
		{
			mTurnSpeed -= mTurnAccel * 0.5f;
			if (mTurnSpeed < 0) mTurnSpeed = 0;
		}
		else
		{
			mTurnSpeed += mTurnAccel * 0.5f;
			if (mTurnSpeed > 0) mTurnSpeed = 0;
		}
	}

	public void Thrust(float inThrustMult)
	{
		GetComponent<Rigidbody>().AddForce(transform.forward * mThrustForce * inThrustMult);
	}

	public void Brake()
	{
		Vector3 localvelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);

		localvelocity.z *= 0.97f; //friction in forward direction
		localvelocity.x *= 0.97f; //friction in forward direction

		Vector3 globalvelocity = transform.TransformDirection(localvelocity);
		GetComponent<Rigidbody>().velocity = globalvelocity;

		//braking means insta-skidding
		//mSkidding = true;
	}

	public void Boost()
	{
		GetComponent<Rigidbody>().AddForce(transform.forward * mBoostForce);
	}

	public void Turn(float inAmount) //amount goes from -1 (full left) to 1 (full right)
	{
		mTurnSpeed += inAmount * mTurnAccel;

		float turnspeedmult = GetComponent<Rigidbody>().velocity.magnitude/mMaxSpeedForTurning;
		if(turnspeedmult > 1) turnspeedmult = 1;

		float maxturnspeed = Mathf.Lerp(mMaxTurnSpeedAtStandStill, mMaxTurnSpeedAtFullSpeed, turnspeedmult);

		if (GetComponent<Rigidbody>().velocity.magnitude < 10)
		{
			//maxturnspeed *= rigidbody.velocity.magnitude * (1.0f/10.0f);
		}

		if (mTurnSpeed > maxturnspeed) mTurnSpeed = maxturnspeed;
		if (mTurnSpeed < -maxturnspeed) mTurnSpeed = -maxturnspeed;
	}

	public void Jump()
	{
		GetComponent<Rigidbody>().AddForce(Vector3.up * 250);
	}

	void ApplyHoverBoard() //apply hoverboard physics
	{
		if (!mInHoverBoardMode)
			mGroundNormal = Vector3.up;

		transform.Rotate(0, 0, -mTurnRoll);

		Vector3 axisToRotateAround = Vector3.Cross(transform.up, mGroundNormal);
		float angleDiff = Vector3.Angle(transform.up, mGroundNormal)*Mathf.Deg2Rad;

		//in air smoothing to ground angle
		if (mInHoverBoardMode)
		{
			if (GetComponent<Rigidbody>().velocity.y < 5)
				transform.RotateAround(axisToRotateAround, angleDiff * mInAirPitchSmooth * (-GetComponent<Rigidbody>().velocity.y * 0.005f + 1.0f));
		}
		else
		{
			transform.RotateAround(axisToRotateAround, angleDiff * mInAirPitchSmooth);
		}

		//suspension code
		if (mOnGround)
		{
			//on ground smoothing to ground angle
			transform.RotateAround(axisToRotateAround, angleDiff * mOnGroundPitchSmooth);

			//suspension calculation
			float suspensionpress = mSuspensionHeight - mDistanceToGround;
			float suspensionforce = suspensionpress * mSuspensionStiffness;
			float suspensiondampingforce = mSuspensionDamping * Vector3.Dot(-GetComponent<Rigidbody>().velocity, mGroundNormal);

			//add suspension force
			float totalforce = suspensionforce + suspensiondampingforce;
			GetComponent<Rigidbody>().AddForce(mGroundNormal * totalforce);
		}

		//sideways friction code
		if (mOnGround || !mInHoverBoardMode)
		{
			//get the velocity in local space
			Vector3 localvelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);

			//Debug.DrawLine(transform.position, transform.position + rigidbody.velocity * 2, Color.red);
			//Debug.DrawLine(transform.position, transform.position + transform.forward*localvelocity.z * 2, Color.green);
			//Debug.DrawLine(transform.position, transform.position + transform.right*localvelocity.x * 2, Color.green);

			//get the velocity we had before friction
			float prevmagnitude = localvelocity.magnitude;

			//Notifier.LocalNotify(""+localvelocity.x);

			//apply sideways friction based on whether we are skidding or not
			if(!mSkidding || !mInHoverBoardMode) //we cant skid in glider mode
			{
				localvelocity.x *= 0.9f;

				//if (Mathf.Abs(localvelocity.x) > 7.5f)
					//mSkidding = true;
			}
			else
			{
				localvelocity.x *= 0.97f;

				if (Mathf.Abs(localvelocity.x) < 7.5f)
					mSkidding = false;
			}

			//we now have all the velocity that we have lost
			float newmagnitude = localvelocity.magnitude; 

			//add the lost velocity to the forward velocity
			localvelocity *= prevmagnitude / newmagnitude;

			//convert it back to global velocity
			Vector3 globalvelocity = transform.TransformDirection(localvelocity);
			GetComponent<Rigidbody>().velocity = globalvelocity;
		}

		GetComponent<Rigidbody>().velocity *= mAirFrictionMultiplier;

		transform.RotateAroundLocal(Vector3.up, Mathf.Deg2Rad*mTurnSpeed);

		//print(rigidbody.velocity.magnitude);

		//add random rotation based on speed
		//if (Random.value > 0.05f)
		//{
			//rigidbody.AddTorque(transform.forward * Random.Range(-1.0f,1.0f) * rigidbody.velocity.magnitude * 0.5f);
		//}

		//rotate roll
		mTurnRoll -= mTurnSpeed*1.25f;

		//un-roll
		mTurnRoll -= mTurnRoll * 0.05f;

		transform.Rotate(0, 0, mTurnRoll);
	}

	void ApplyGlider() //apply glider physics
	{
		//lift
		//if(rigidbody.velocity.y <= 0)

		float speedLiftForce = GetComponent<Rigidbody>().velocity.magnitude * mLiftForce;
		float constantLiftForce = 100.0f * mLiftForce;

		float liftForce = Mathf.Lerp(speedLiftForce, constantLiftForce, 0.5f);

		GetComponent<Rigidbody>().AddForce(Vector3.up * liftForce); 
	}

	void UpdateGroundInfo()
	{
		float extraheight = 2.0f;
		float carlength = 3.5f;

		Vector3 forward = transform.forward;
		forward.y = 0; 
		forward.Normalize();
		forward *= carlength;

		RaycastHit hit1,hit2;
		Vector3 origin1 = transform.position + new Vector3(0, extraheight, 0) - forward;
		Vector3 origin2 = transform.position + new Vector3(0, extraheight, 0) + forward;
		Vector3 direction = Vector3.down;

		int layermask = 1 << 8 | 1 << 2 | 1 << 9;//player layer and ignore raycast layer
		layermask = ~layermask; //we trace everything BUT the players

		if (Physics.Raycast(origin1, direction, out hit1, Mathf.Infinity, layermask))
		{
			if (Physics.Raycast(origin2, direction, out hit2, Mathf.Infinity, layermask))
			{
				float grounddist1 = hit1.distance - extraheight;
				float grounddist2 = hit2.distance - extraheight;
				mDistanceToGround = (grounddist1 + grounddist2) * 0.5f;

				mGroundNormal = (hit1.normal+hit2.normal)*0.5f;
				mGroundNormal.Normalize();
			}
		}
		else
		{
			mDistanceToGround = 10000;
			mGroundNormal = Vector3.up;
		}

		if (mDistanceToGround < mSuspensionHeight)
		{
			mOnGround = true;
		}
		else
		{
			mOnGround = false;
		}
	}

	void UpdateTether()
	{
		float percentage = GetComponent<PlayerPlace>().GetPlacePercentage();

		percentage = (percentage - 0.5f) * 2; //now it goes from -1 to 1

		mThrustForce = mOrigThrust - mThrustMaxTether * percentage;
	}

	public void TurnToDir(Vector3 inDirection)
	{
		Vector3 curDir = transform.forward;

		bool turnLeft = Vector3.Cross(curDir, inDirection).y < 0;

		float angle = Vector3.Angle(curDir, inDirection);

		if (angle > 5.0f) //movement threshold
		{
			if (turnLeft)
			{
				float turnSpeed = -mTurnSpeed;
				float timeToStop = turnSpeed / mTurnAccel * 0.5f;

				if (timeToStop * turnSpeed > angle)
					Turn(1.0f);
				else
					Turn(-1.0f);
			}
			else
			{
				float turnSpeed = mTurnSpeed;
				float timeToStop = turnSpeed / mTurnAccel * 0.5f;

				if (timeToStop * turnSpeed > angle)
					Turn(-1.0f);
				else
					Turn(1.0f);
			}
		}
	}

	public void Switch()
	{
		if (mInHoverBoardMode)
			SwitchToGlider();
		else
			SwitchToBoard();
	}

	[RPC]
	public void SwitchToGlider()
	{
		if (mInHoverBoardMode)
		{
			mInHoverBoardMode = false;
			transform.Find("Glider").GetComponent<Renderer>().enabled = true;

			if (GetComponent<NetworkView>().isMine)
				GetComponent<NetworkView>().RPC("SwitchToGlider", RPCMode.Others);
		}
	}

	[RPC]
	public void SwitchToBoard()
	{
		if (!mInHoverBoardMode)
		{
			mInHoverBoardMode = true;
			transform.Find("Glider").GetComponent<Renderer>().enabled = false;

			if (GetComponent<NetworkView>().isMine)
				GetComponent<NetworkView>().RPC("SwitchToBoard", RPCMode.Others);
		}
	}

	public bool IsInHoverBoardMode() { return mInHoverBoardMode; }

	public void DisableColliderFor(float inSeconds)
	{
		StartCoroutine(TemporaryDisableCollider(inSeconds));
	}

	IEnumerator TemporaryDisableCollider(float inSeconds)
	{
		GetComponent<Collider>().isTrigger = true;

		yield return new WaitForSeconds(inSeconds);

		GetComponent<Collider>().isTrigger = false;
	}

	void OnGUI()
	{
		if (Utils.IsLocalPlayer(gameObject, false))
		{
			GUI.Label(new Rect(250, Screen.height - 30, 500, 40), "Velocity: " + GetComponent<Rigidbody>().velocity.magnitude.ToString("N2"));
		}
	}

	public float GetTurnSpeed() { return mTurnSpeed; }
}