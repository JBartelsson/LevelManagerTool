using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SceneSetupSO))]
public class SceneBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var sceneSetupSO = target as SceneSetupSO;

        //Generiert Button für das Laden der Szenen
        GUI.backgroundColor = Color.yellow;
        if (sceneSetupSO.scenes != null && sceneSetupSO.scenes.Length > 0 && GUILayout.Button("Load Scene Setup", GUILayout.Height(50)))
        {
            //Unity interne Funktionen um Szenen zu laden

            EditorSceneManager.RestoreSceneManagerSetup(sceneSetupSO.scenes);
        }

        //Button um die aktuellen Szenen zu speichern

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Save Scene Setup"))
        {
            sceneSetupSO.scenes = EditorSceneManager.GetSceneManagerSetup();

            sceneSetupSO.runtimeScenes = new SceneSetupSO.RuntimeSceneSetup[sceneSetupSO.scenes.Length];
            for (int i = 0; i < sceneSetupSO.scenes.Length; i++)
            {
                sceneSetupSO.runtimeScenes[i] = new SceneSetupSO.RuntimeSceneSetup();
                sceneSetupSO.runtimeScenes[i].path = sceneSetupSO.scenes[i].path;
                sceneSetupSO.runtimeScenes[i].isActiveScene = sceneSetupSO.scenes[i].isActive;
            }
            //Editor als Dirty markieren, damit Änderungen gespeichert werden

            EditorUtility.SetDirty(sceneSetupSO);
        }
    }
}

