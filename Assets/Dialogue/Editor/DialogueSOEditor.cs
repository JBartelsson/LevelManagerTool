using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(SceneTransitions))]
public class DialogueSOEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var dialogueContainer = (SceneTransitions)target;
        if (GUILayout.Button("Open Editor")){
            Debug.Log(target.name);
            DialogueGraph.OpenDialogueGraphWindow(AssetDatabase.GetAssetPath(target));
        }
    }

}
