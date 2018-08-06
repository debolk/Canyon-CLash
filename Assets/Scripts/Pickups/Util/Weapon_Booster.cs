using UnityEngine;
using System.Collections;

public class Weapon_Booster : MonoBehaviour
{
	public GameObject mBoostModel;

	// Use this for initialization
	void Start()
	{
		//mBoostModel = (GameObject)Instantiate(Resources.Load("Booster"), Vector3.zero, Quaternion.identity);

		//mBoostModel.transform.parent = transform;
		//mBoostModel.transform.localPosition = new Vector3(0,0,-1);
		//mBoostModel.transform.localRotation = Quaternion.identity;

		EnableBoost();

		StartCoroutine(WaitForRemove());
	}

	IEnumerator WaitForRemove()
	{
		yield return new WaitForSeconds(3);

		DisableBoost();

		//Destroy(mBoostModel);
		Destroy(this);
	}

	void FixedUpdate()
	{
		if (Utils.IsLocalPlayer(gameObject, true))
		{
			GetComponent<PlayerPhysics>().Boost();
		}
	}

	void OnLevelRestarted()
	{
		//Destroy(mBoostModel);
		Destroy(this);
	}

	void EnableBoost()
	{
		Transform fire1 = transform.Find("Flame1");
		Transform fire2 = transform.Find("Flame2");

		//enable lights
		foreach (Light curLight in fire1.GetComponentsInChildren<Light>())
		{
			curLight.enabled = true;
		}

		foreach (Light curLight in fire2.GetComponentsInChildren<Light>())
		{
			curLight.enabled = true;
		}

		//enable particles
		foreach (ParticleEmitter curEmitter in fire1.GetComponentsInChildren<ParticleEmitter>())
		{
			curEmitter.emit = true;
		}

		foreach (ParticleEmitter curEmitter in fire2.GetComponentsInChildren<ParticleEmitter>())
		{
			curEmitter.emit = true;
		}

		transform.Find("BoostSound").audio.mute = false;
	}

	void DisableBoost()
	{
		Transform fire1 = transform.Find("Flame1");
		Transform fire2 = transform.Find("Flame2");

		//disable lights
		foreach (Light curLight in fire1.GetComponentsInChildren<Light>())
		{
			curLight.enabled = false;
		}

		foreach (Light curLight in fire2.GetComponentsInChildren<Light>())
		{
			curLight.enabled = false;
		}

		//disable particles
		foreach (ParticleEmitter curEmitter in fire1.GetComponentsInChildren<ParticleEmitter>())
		{
			curEmitter.emit = false;
		}

		foreach (ParticleEmitter curEmitter in fire2.GetComponentsInChildren<ParticleEmitter>())
		{
			curEmitter.emit = false;
		}

		transform.Find("BoostSound").audio.mute = true;
	}
}
