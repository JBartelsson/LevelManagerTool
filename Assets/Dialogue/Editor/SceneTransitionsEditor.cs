using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(SceneTransitions))]
public class SceneTransitionsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var dialogueContainer = (SceneTransitions)target;
        if (GUILayout.Button("Open Editor")){
            Debug.Log(target.name);
            SceneTransitionsWindow.OpenDialogueGraphWindow(AssetDatabase.GetAssetPath(target));
        }
    }

}
