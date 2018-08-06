using UnityEngine;
using System.Collections;
using UnityEditor;

public class FBXScaleFix : AssetPostprocessor
{
	public void OnPreprocessModel()
	{
		ModelImporter modelImporter = (ModelImporter)assetImporter;
		if(modelImporter.globalScale == 0.01f)
			modelImporter.globalScale = 1;
	}
}

