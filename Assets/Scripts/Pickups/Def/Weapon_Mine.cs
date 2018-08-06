using UnityEngine;
using System.Collections;

public class Weapon_Mine : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
		StartCoroutine(MineSpawner());
	}

	void SpawnMine()
	{
		Vector3 pos = gameObject.transform.position - gameObject.transform.forward + new Vector3(0, 1, 0);
		Network.Instantiate(Resources.Load("Mine"), pos, Quaternion.identity, 3);
	}

	IEnumerator MineSpawner()
	{
		SpawnMine();
		yield return new WaitForSeconds(0.25f);
		SpawnMine();
		yield return new WaitForSeconds(0.25f);
		SpawnMine();
		yield return new WaitForSeconds(0.25f);
		SpawnMine();

		Destroy(this);
	}

	void OnLevelRestarted()
	{
		Destroy(this);
	}
}
