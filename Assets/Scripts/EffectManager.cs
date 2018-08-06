using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour
{
	GameObject mExplosion;
	static public GameObject mMainObject;

	void Start () 
	{
		mExplosion = GameObject.Find("Explosion");
	}

	void Update () 
	{
		
	}

	static public EffectManager GetSingleton()
	{
		return GameObject.Find("_MAINGAMEOBJECT").GetComponent<EffectManager>();
	}
	
	static public void TriggerExplosion(Vector3 inPos, bool inLocal)
	{
		if (inLocal)
			EffectManager.GetSingleton().ExplosionRPC(inPos);
		else
			EffectManager.GetSingleton().networkView.RPC("ExplosionRPC", RPCMode.All, inPos);
	}

	[RPC]
	void ExplosionRPC(Vector3 inPos)
	{
		mExplosion.transform.position = inPos;
		mExplosion.particleEmitter.Emit(200);

		SoundManager.GetSingleton().Spawn3DSound(SFXType.Explosion, inPos,45,2500);
	}
}

