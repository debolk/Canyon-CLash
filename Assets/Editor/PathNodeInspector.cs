using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathNode))]
public class PathNodeInspector : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		PathNode node = (PathNode)target;
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Add Next Node"))
		{
			GameObject nextnode = (GameObject)GameObject.Instantiate(node.gameObject,node.transform.position,Quaternion.identity);
			nextnode.transform.localScale = Vector3.one;
			nextnode.transform.position += node.GetDirection()*25;
			nextnode.transform.parent = node.transform.parent;
			nextnode.name = node.name;
			nextnode.GetComponent<PathNode>().mPrevNode = node.transform;
			node.GetComponent<PathNode>().mNextNode = nextnode.transform;
			Selection.activeObject = nextnode;
		}
		GUILayout.EndHorizontal();
	}
}