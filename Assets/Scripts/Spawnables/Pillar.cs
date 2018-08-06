using UnityEngine;
using System.Collections;

public class Pillar : MonoBehaviour 
{
	float mHeight = 20.0f;
	Vector3 mOriginalPos;

	// Use this for initialization
	void Start () 
	{
		mOriginalPos = transform.position;
		transform.position = mOriginalPos + new Vector3(0, -mHeight, 0);

		SoundManager.GetSingleton().Spawn3DSound(SFXType.ItemDrop, gameObject);
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		transform.position -= (transform.position-mOriginalPos)*0.04f;
	}

	void OnLevelRestarted()
	{
		Destroy(gameObject);
	}
}
