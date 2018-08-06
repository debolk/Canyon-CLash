using UnityEngine;
using System.Collections;

public class Mortar : MonoBehaviour
{
	float mVel;

	void Start () 
	{
		mVel = 40;
		StartCoroutine(WaitForRemove());
		SoundManager.GetSingleton().Spawn3DSound(SFXType.RocketLaunch, gameObject);
	}

	IEnumerator WaitForRemove()
	{
		yield return new WaitForSeconds(2);
		Destroy(gameObject);
	}

	void FixedUpdate() 
	{
		mVel += 4;
		transform.position += Vector3.up * mVel * Time.fixedDeltaTime;
	}

	void OnLevelRestarted()
	{
		Destroy(gameObject);
	}
}

