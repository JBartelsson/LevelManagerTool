using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(DialogueContainer))]
public class DialogueSOEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var dialogueContainer = (DialogueContainer)target;
        if (GUILayout.Button("Open Editor")){
            Debug.Log(target.name);
            DialogueGraph.OpenDialogueGraphWindow(AssetDatabase.GetAssetPath(target));
        }
    }

}
