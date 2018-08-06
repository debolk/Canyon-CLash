using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour 
{
	PlayerPhysics mPlayer;
	PlayerPickupManager mPickupManager;
	bool mInControl;
	bool mReversedControls;
	float mThrust;

	Vector3 mPreferredDirection;
	GameObject mTargetPickup;

	int timeInHover = 0; //number of frames in hover board mode

	struct Personality
	{
		//all set from 0.0 to 1.0
		public float mEarlySteer; //sets in corners early or late
		public float mAwayFromEdges; //how much the player hates road edges
		public float mPickupUser; //how much the player likes to get pickups (not used at the moment)
		public float mPickupSaver; //how much the player likes to save up for strong pickups (not used at the moment)
	}

	Personality mPersonality;

	// Use this for initialization
	void Start () 
	{
		mPlayer = gameObject.GetComponent<PlayerPhysics>();
		mPickupManager = gameObject.GetComponent<PlayerPickupManager>();

		mInControl = false;
		mReversedControls = false;
		mThrust = 1.0f; //Random.Range(0.5f, 1.0f);
		mTargetPickup = null;

		GeneratePersonality(0.4f,0.9f);
		//mPersonality.mAwayFromEdges = 0.5f;
		//mPersonality.mEarlySteer = 0.65f;
	}

	void OnLevelRestarted()
	{
		StopAllCoroutines();
		mInControl = false;
		mReversedControls = false;
		mTargetPickup = null;
		transform.Find("GRP_InvertedControls/InvertedControls").GetComponent<Renderer>().enabled = false;
	}

	void FixedUpdate()
	{
		if (mInControl)
		{
			//accelerate
			mPlayer.Thrust(mThrust);

			UnityEngine.Profiling.Profiler.BeginSample("AI_UpdatePreferredDir");
			UpdatePreferredDir();
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("AI_UpdatePickupLogic");
			UpdatePickupLogic();
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("AI_UpdateSteering");
			UpdateSteering();
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("AI_UpdatePickupUsage");
			UpdatePickupUsage();
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("AI_UpdateGliderUsage");
			UpdateGliderUsage();
			UnityEngine.Profiling.Profiler.EndSample();

			//if (!goingTowardsCenter && angleAwayFromStraight > 10 && rigidbody.velocity > 50)
				//mPlayer.Brake();

			//mPlayer.Turn(Random.Range(-1.0f, 1.0f));
		}
	}

	bool RandomEveryXSeconds(float inSeconds)
	{
		float chance = Time.fixedDeltaTime / inSeconds;
		
		if (Random.value < chance)
			return true;

		return false;
	}

	void UpdateGliderUsage()
	{
		float curHeight = mPlayer.mDistanceToGround;

		if (mPlayer.IsInHoverBoardMode())
		{
			timeInHover = 0;

			//see if the path is going down fast, and we are in the air
			Vector3 dirAhead = PathUtils.CalcDirectionAhead(transform.position, GetComponent<Rigidbody>().velocity.magnitude * 0.25f,gameObject);

			if (dirAhead.y < -0.25f)
			{
				mPlayer.Switch();
			}
			else if(curHeight > 15)
			{
				if (RandomEveryXSeconds(5.0f))
					mPlayer.Switch();
			}
		}
		else
		{
			timeInHover++;

			//see if we are on ground again
			if (curHeight < 5 && timeInHover > 50)
			{
				if (RandomEveryXSeconds(0.25f))
				{
					mPlayer.Switch();
				}
			}
		}
	}

	void UpdatePickupUsage()
	{

		if (mPickupManager.CurrentPickupUsesTargetting())
			mPickupManager.UsePickup();

		if (Utils.RANDOMPICKUPS())
		{
			if (RandomEveryXSeconds(6))
				mPickupManager.UsePickup();
		}
		else
		{
			if (RandomEveryXSeconds(13))
				mPickupManager.UsePickup();
		}

		//if(mPickupManager.GetCurrentPickupLevel() == 3)
			//mPickupManager.UsePickup();
	}

	void UpdatePickupLogic()
	{
		//see if we have a target pickup or not
		if (mTargetPickup)
		{
			Debug.DrawLine(transform.position, mTargetPickup.transform.position, Color.yellow);

			//see if the target pickup is still reachable
			Vector3 diff = mTargetPickup.transform.position - transform.position;
			float angle = Vector3.Angle(mPreferredDirection, diff);

			if (angle > 45) //if the pickup is behind us, we cannot grab it anymore
			{
				mTargetPickup = null;
				GeneratePersonality(0.4f, 0.9f);
			}
		}
		else //we will look for a pickup
		{
			//see if we want to look for a new pickup
			float everyXSeconds = 2.5f; 
			float chance = Time.fixedDeltaTime/everyXSeconds;

			if (Random.value < chance) //we will look for a pickup every X seconds
			{
				//see if there is a pickup that we might want to grab
				GameObject newPickup = null;

				float coneLength = 150.0f;
				float coneWidth = 30.0f;

				int currentLevel = mPickupManager.GetCurrentPickupLevel();
				PickupType currentType = mPickupManager.GetCurrentPickup();

				if (Random.value < 0.5f || currentLevel == 3) //50% that we will go for an upgrade, 50% that we will go for a Type block.
				{
					//first we will look at where we are located in the race
					float posInRace = GetComponent<PlayerPlace>().GetPlacePercentage();

					if (Utils.RANDOMPICKUPS())
					{
						newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup", coneLength, coneWidth);
					}
					else
					{
						if (posInRace < 0.33f) //in the back of the race
						{
							if (currentType != PickupType.OFFENSIVE && currentType != PickupType.UTILITY) //in the back we take offensive and utility pickups
							{
								if (Random.value < 0.5f) newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_UTIL", coneLength, coneWidth);
								else newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_OFF", coneLength, coneWidth);
							}
						}
						else if (posInRace < 0.66f) //in the middle
						{
							if (currentType == PickupType.NONE) //in the middle we take all sorts of pickups
							{
								if (Random.value < 0.33f) newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_OFF", coneLength, coneWidth);
								else if (Random.value < 0.5f) newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_DEF", coneLength, coneWidth);
								else newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_UTIL", coneLength, coneWidth);
							}
						}
						else //in the front of the race
						{
							if (currentType != PickupType.OFFENSIVE && currentType != PickupType.DEFENSIVE) //in the front we take defensive and offensive pickups
							{
								if (Random.value < 0.5f) newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_OFF", coneLength, coneWidth);
								else newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_DEF", coneLength, coneWidth);
							}
						}
					}
				}
				else
				{
					newPickup = Utils.GetTaggedObjectInConeInFront(gameObject, "Pickup_UPGR", coneLength, coneWidth);
				}

				if (newPickup)
				{
					mTargetPickup = newPickup;
					mPersonality.mEarlySteer = 0.65f; //we need good steering to steer correctly towards the pickup
				}
			}
		}
	}

	void UpdatePreferredDir()
	{
		//get path info
		PathNode currentNode = PathUtils.GetCurrentPathNode(transform.position,gameObject);
		Vector3 diffToCenterOfPath = currentNode.GetVectorToPath(transform.position);

		//calculate the direction a while ahead of the player sort of looking in front
		float amountToLookAhead = Mathf.Lerp(GetComponent<Rigidbody>().velocity.magnitude*0.1f, GetComponent<Rigidbody>().velocity.magnitude, mPersonality.mEarlySteer);
		Vector3 directionAhead = PathUtils.CalcDirectionAhead(transform.position, amountToLookAhead,gameObject); //the faster we go, the further we have to look in front

		//calculate how much we are nearing an edge and want to go away from it
		float roadWidth = currentNode.GetWidthAt(transform.position);
		float thresHold = roadWidth * 0.5f;
		float awayFromCenterFactor = (diffToCenterOfPath.magnitude - thresHold) / (roadWidth - thresHold);

		//awayFromCenterFactor = Mathf.Lerp(0.0f, awayFromCenterFactor * 2, mPersonality.mAwayFromEdges); //apply personality
		awayFromCenterFactor = Mathf.Clamp(awayFromCenterFactor, 0.0f, 1.0f);
		
		Vector3 dirToCenter = diffToCenterOfPath.normalized;

		//calculate the direction we would want to look by taking all wanted directions, and multiply by how much we want to go that direction
		mPreferredDirection = directionAhead + dirToCenter*awayFromCenterFactor*0.5f;
		mPreferredDirection.Normalize();
	}

	void UpdateSteering()
	{
		Vector3 finalSteerDir = mPreferredDirection;

		if (mTargetPickup != null)
		{
			//code to steer correctly with corners and pickups
			Vector3 diffToPickup	= mTargetPickup.transform.position - transform.position;
			Vector3 dirToPickup		= diffToPickup.normalized;
			float   distToPickup	= diffToPickup.magnitude;
			Vector3 directionAhead	= PathUtils.CalcDirectionAhead(transform.position, GetComponent<Rigidbody>().velocity.magnitude*2,gameObject); //the faster we go, the further we have to look in front
			Debug.DrawLine(transform.position, transform.position + directionAhead * 5, Color.magenta);

			Vector3 mirroredDirection = -Vector3.Reflect(directionAhead, dirToPickup);

			//we start pre-steering as soon as the pickup is nearer than rigidbody.magnitude
			//the pre-steer ends at dist 0
			float fullPreSteerDist = GetComponent<Rigidbody>().velocity.magnitude*0.75f;

			float preSteerFactor = ((distToPickup - GetComponent<Rigidbody>().velocity.magnitude * 0.1f) - fullPreSteerDist) / fullPreSteerDist;

			if (preSteerFactor > 0.0f)
			{
				preSteerFactor = 1.0f - Mathf.Clamp(preSteerFactor, 0.0f, 1.0f);
				Vector3 moddedSteerDir = Vector3.Lerp(dirToPickup, mirroredDirection, preSteerFactor*0.25f);
				finalSteerDir = moddedSteerDir.normalized;
			}
			else
			{
				finalSteerDir = dirToPickup;
			}
		}

		//potentially invert controls
		if (mReversedControls)
			finalSteerDir = -Vector3.Reflect(finalSteerDir, transform.forward);

		finalSteerDir.Normalize();

		//debug lines
		Debug.DrawLine(transform.position, transform.position + transform.forward * 10, Color.red);
		Debug.DrawLine(transform.position, transform.position + mPreferredDirection * 10, Color.magenta);
		Debug.DrawLine(transform.position, transform.position + finalSteerDir * 10, Color.green);

		//actual turning call
		mPlayer.TurnToDir(finalSteerDir);
	}

	public void AllowControl() { mInControl = true; }
	public void RemoveControl() { mInControl = false; }
	public bool HasControl() { return mInControl; }

	IEnumerator ReverseControls()
	{
		transform.Find("GRP_InvertedControls/InvertedControls").GetComponent<Renderer>().enabled = true;

		float totaltime = 5.0f;
		float disorrienttime = 0.5f;

		//we mimic the effec of a real player that has 2 moments of disorientation on activate and on deactivate
		mReversedControls = true;
		yield return new WaitForSeconds(disorrienttime);
		mReversedControls = false;

		yield return new WaitForSeconds(totaltime-disorrienttime);

		mReversedControls = true;
		yield return new WaitForSeconds(disorrienttime);
		mReversedControls = false;

		transform.Find("GRP_InvertedControls/InvertedControls").GetComponent<Renderer>().enabled = false;
	}

	[RPC]
	void TriggerAIReverseControls()
	{
		StartCoroutine(ReverseControls());
	}

	void GeneratePersonality(float min, float max)
	{
		mPersonality.mAwayFromEdges = Random.Range(min, max);
		mPersonality.mEarlySteer = Random.Range(min, max);
		mPersonality.mPickupUser = Random.Range(min, max);
		mPersonality.mPickupSaver = Random.Range(min, max);
	}
}
