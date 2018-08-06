using UnityEngine;
using System.Collections;

public enum SongType
{
	MAINMENU,
	INGAME
}

public enum SFXType
{
	ItemDrop,
	Explosion,
	RocketLaunch,
	MachineGunFire,
	BigMachineGunFire,
	MachineGunHit,
	ItemPickup,
	ItemPickupLevelUp,
	InvertControls,
	PositionSwap,
	Armor,
	ReadySet,
	Go,
	GrapplingHit
}

public class SoundManager : MonoBehaviour
{
	public bool toggleMusic;
	public bool toggleSFX;

	public AudioClip[] BGSounds;
	public AudioClip[] SFXSounds;

	AudioSource mCurrentBGSong;
	AudioSource mNextBGSong;
	SongType mCurrentSongType;

	Transform mSoundTemplate;

	void Start () 
	{
		//dummy audio source
		mCurrentSongType = SongType.INGAME;
		mCurrentBGSong = gameObject.AddComponent<AudioSource>();

		FadeBGSoundTo(SongType.MAINMENU);
	}

	void FixedUpdate () 
	{
		//update player hover sounds
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			float vel = player.rigidbody.velocity.magnitude;
			float lerpFactor = Mathf.InverseLerp(0, 150.0f, vel);
			float targetPitch = Mathf.Lerp(0.5f, 3.0f, lerpFactor);
			float targetVolume = Mathf.InverseLerp(-50, 50.0f, vel);

			AudioSource hoverSound = player.transform.Find("HoverSound").audio;
			hoverSound.pitch -= (hoverSound.pitch - targetPitch) * 0.05f;
			hoverSound.volume -= (hoverSound.volume - targetVolume) * 0.05f;
			hoverSound.mute = !toggleSFX;
		}
	}

	static public SoundManager GetSingleton()
	{
		return GameObject.Find("_MAINGAMEOBJECT").GetComponent<SoundManager>();
	}

	public void FadeBGSoundTo(SongType inSongType)
	{
		if (mCurrentSongType != inSongType)
		{
			mCurrentSongType = inSongType;

			AudioClip nextSong = BGSounds[0];
			if (inSongType == SongType.INGAME)
				nextSong = BGSounds[1];

			mNextBGSong = gameObject.AddComponent<AudioSource>();
			mNextBGSong.clip = nextSong;
			mNextBGSong.loop = true;
			mNextBGSong.volume = 0.0f;
			mNextBGSong.mute = !toggleMusic;
			mNextBGSong.Play();

			StartCoroutine(FadeAudioSources());
		}
	}

	IEnumerator FadeAudioSources()
	{
		//fade to new song
		while (mCurrentBGSong.volume > 0)
		{
			mCurrentBGSong.volume -= 0.1f;
			mNextBGSong.volume += 0.1f;
			yield return new WaitForSeconds(0.15f);
		}

		mNextBGSong.volume = 1.0f;

		//destroy old song
		Destroy(mCurrentBGSong);

		//set new song as current
		mCurrentBGSong = mNextBGSong;
	}

	public AudioSource Spawn3DSound(SFXType sound, Vector3 pos)
	{
		return Spawn3DSound(sound, pos, 15, 750);
	}

	public AudioSource Spawn3DSound(SFXType sound, Vector3 pos, float minDist, float maxDist)
	{
		GameObject dummyObject = new GameObject("sounddummy");
		dummyObject.transform.position = pos;
		AudioSource returner = Spawn3DSound(sound, dummyObject, minDist, maxDist);

		AudioClip clip = SFXSounds[(int)sound];
		StartCoroutine(DestroyAfter(dummyObject, clip.length*1.25f));

		return returner;
	}

	public AudioSource Spawn3DSound(SFXType sound, GameObject parent)
	{
		return Spawn3DSound(sound, parent, 15, 750);
	}

	public AudioSource Spawn3DSound(SFXType sound, GameObject parent, float minDist, float maxDist)
	{
		AudioClip clip = SFXSounds[(int)sound];

		AudioSource newAudio = parent.AddComponent<AudioSource>();
		
		newAudio.minDistance = minDist;
		newAudio.maxDistance = maxDist;

		if (toggleSFX)
		{
			newAudio.clip = clip;
			newAudio.Play();
		}

		//make sure it gets destroyed when done
		StartCoroutine(DestroyAfter(newAudio, clip.length));

		return newAudio;
	}

	public AudioSource Spawn2DSound(SFXType sound)
	{
		return Spawn2DSound(sound, 1.0f);
	}

	public AudioSource Spawn2DSound(SFXType sound, float volume)
	{
		AudioClip clip = SFXSounds[(int)sound];

		AudioSource newAudio = gameObject.AddComponent<AudioSource>();
		newAudio.volume = volume;
		if (toggleSFX)
		{
			newAudio.clip = clip;
			newAudio.Play();
		}

		//make sure it gets destroyed when done
		StartCoroutine(DestroyAfter(newAudio, clip.length));

		return newAudio;
	}

	IEnumerator DestroyAfter(Object objectToDestroy, float timeBeforeDestroy)
	{
		yield return new WaitForSeconds(timeBeforeDestroy);

		if(objectToDestroy)
			Destroy(objectToDestroy);
	}

	public void MuteBGSounds()
	{
		toggleMusic = false;
		mNextBGSong.mute = true;
		mCurrentBGSong.mute = true;
	}

	public void UnMuteBGSounds()
	{
		toggleMusic = true;
		mNextBGSong.mute = false;
		mCurrentBGSong.mute = false;
	}

	public void ToggleBGSounds()
	{
		if (!toggleMusic)
		{
			UnMuteBGSounds();
		}
		else
		{
			MuteBGSounds();
		}
	}

	public void MuteSFXSounds()
	{
		toggleSFX = false;
	}

	public void UnMuteSFXSounds()
	{
		toggleSFX = true;
	}
}

