using UnityEngine;
using UnityEditor;
using System.Collections;

public class RoelsToels : MonoBehaviour 
{
	////////////////////
	//TEXTURES
	////////////////////

	[MenuItem("RoelsToels/Textures/Disable mipmaps on selected textures")]
	static void DisableMipmapOnSelected()
	{
		//get all textures selected
		Object[] alltextures = GetSelectedTextures();

		//show dialog box
		if (EditorUtility.DisplayDialog("RoelToels: Disable mipmaps", "Are you sure you want to disable mipmapping on these " + alltextures.Length + " textures?", "Yes", "No"))
		{
			//clear selection
			Selection.objects = new Object[0];

			Debug.Log("RoelsToels: Disabling mipmaps on selected " + alltextures.Length + " textures...");

			foreach (Object curtex in alltextures)
			{
				//get the importer
				string path = AssetDatabase.GetAssetPath(curtex);
				TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

				//disable mipmap
				textureImporter.mipmapEnabled = false;

				//save it
				AssetDatabase.ImportAsset(path);
			}
		}

		Debug.Log("RoelsToels: Done.");
	}

	[MenuItem("RoelsToels/Textures/Set selected textures as GUI textures")]
	static void SelectTexturesAsGUI()
	{
		//get all textures selected
		Object[] alltextures = GetSelectedTextures();

		//show dialog box
		if (EditorUtility.DisplayDialog("RoelToels: Change textures to GUI ones", "Are you sure you want to set these selected " + alltextures.Length + " textures as GUI textures?", "Yes", "No"))
		{
			//clear selection
			Selection.objects = new Object[0];

			Debug.Log("RoelsToels: Setting selected " + alltextures.Length + " textures as GUI textures...");

			foreach (Object curtex in alltextures)
			{
				//get the importer
				string path = AssetDatabase.GetAssetPath(curtex);
				TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

				//set it as gui
				textureImporter.textureType = TextureImporterType.GUI;
				textureImporter.npotScale = TextureImporterNPOTScale.None;
				TextureImporterSettings st = new TextureImporterSettings();
				textureImporter.ReadTextureSettings(st);
				st.wrapMode = TextureWrapMode.Clamp;
				textureImporter.SetTextureSettings(st);

				//save it
				AssetDatabase.ImportAsset(path);
			}
		}

		Debug.Log("RoelsToels: Done.");
	}

	static Object[] GetSelectedTextures()
	{
		return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
	}


	////////////////////
	//MODELS
	////////////////////

	[MenuItem("RoelsToels/Models/Set import scale to 1.0")]
	static void ResetImportScale()
	{
		//get all gameobjects selected
		Object[] allmodels = Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets);
		int nummodels = allmodels.Length;

		//leave only the models
		foreach (Object curmodel in allmodels)
		{
			if (!IsAModel(curmodel))
			{
				nummodels--;
			}
		}

		//show dialog box
		if (EditorUtility.DisplayDialog("RoelToels: Reset model scale", "Are you sure you want to correct the import scale of these " + nummodels + " models?", "Yes", "No"))
		{
			//clear selection
			Selection.objects = new Object[0];

			Debug.Log("RoelsToels: Setting model import scale to 1.0 of these " + nummodels + " models...");

			foreach (Object curmodel in allmodels)
			{
				print(curmodel.GetType());

				if (IsAModel(curmodel))
				{
					//get the importer
					string path = AssetDatabase.GetAssetPath(curmodel);
					ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;

					//correct the scale
					modelImporter.globalScale = 1.0f;

					//save it
					AssetDatabase.ImportAsset(path);
				}
			}
		}

		Debug.Log("RoelsToels: Done.");
	}


	static bool IsAModel(Object inObject)
	{
		string path = AssetDatabase.GetAssetPath(inObject);
		
		if(path.Contains(".fbx"))
			return true;
		else
			return false;
	}

	////////////////////
	//Transform
	////////////////////
	[MenuItem("RoelsToels/Transform/Reset Transform of selected object(s) &r")]
	static void ResetTransform()
	{
		//get all gameobjects selected
		foreach (Transform transform in Selection.transforms)
		{
			transform.localPosition = new Vector3(0, 0, 0);
		}
	}

	[MenuItem("RoelsToels/Transform/Drop selected objects to the floor &d")]
	static void DropToFloor()
	{
		//get all gameobjects selected
		foreach (GameObject curObject in Selection.gameObjects)
		{
			RaycastHit info;
			Vector3 orig = curObject.transform.position;
			Vector3 dir = Vector3.down;

			if (Physics.Raycast(orig, dir, out info))
			{
				curObject.transform.position = info.point;
			}
		}
	}

	////////////////////
	//Player resolutions
	////////////////////


	[MenuItem("RoelsToels/Set player resolution/Set to 640x960 &1")]
	static void SetPlayer640x960()
	{
		SetPlayerResolution(640,960);
	}

	[MenuItem("RoelsToels/Set player resolution/Set to 480x800 &2")]
	static void SetPlayer480x800()
	{
		SetPlayerResolution(480, 800);
	}

	[MenuItem("RoelsToels/Set player resolution/Set to 480x640 &3")]
	static void SetPlayer480x640()
	{
		SetPlayerResolution(480, 640);
	}

	[MenuItem("RoelsToels/Set player resolution/Set to 320x480 &4")]
	static void SetPlayer320x480()
	{
		SetPlayerResolution(320, 480);
	}

	[MenuItem("RoelsToels/Set player resolution/Set to 1024x768 &5")]
	static void SetPlayer1024x768()
	{
		SetPlayerResolution(1024, 768);
	}

	static void SetPlayerResolution(int width, int height)
	{
		PlayerSettings.defaultScreenWidth = width;
		PlayerSettings.defaultScreenHeight = height;
	}

	////////////////////
	//GameObjects
	////////////////////
	[MenuItem("RoelsToels/GameObjects/Set the tag of all selected objects to their name")]
	static void SetTags()
	{
		//get all gameobjects selected
		foreach (GameObject gameobject in Selection.gameObjects)
		{
			gameobject.tag = gameobject.name;
		}
	}

	[MenuItem("RoelsToels/GameObjects/Set the tag of all selected objects to...")]
	static void SetTagsByHand()
	{
		//get all gameobjects selected
		foreach (GameObject gameobject in Selection.gameObjects)
		{
			gameobject.tag = "LevelChunk";
		}
	}
}
