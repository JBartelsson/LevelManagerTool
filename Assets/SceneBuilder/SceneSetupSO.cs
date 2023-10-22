using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu(fileName = "SceneSetup", menuName = "Scriptable Objects/Scene Setup")]
    public class SceneSetupSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField]
        public UnityEditor.SceneManagement.SceneSetup[] scenes;
#endif

        [SerializeField]
        [HideInInspector]
        public RuntimeSceneSetup[] runtimeScenes;
        [System.Serializable]
        public class RuntimeSceneSetup
        {
            public string path;
            public bool isActiveScene;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
        Debug.Log(runtimeScenes);
            if (runtimeScenes.Length == scenes.Length)
            {
                for (int i = 0; i < scenes.Length; i++)
                {
                    runtimeScenes[i].isActiveScene = scenes[i].isActive;
                }
            }
            else
            {
                runtimeScenes = new RuntimeSceneSetup[scenes.Length];

                for (int i = 0; i < scenes.Length; i++)
                {
                    runtimeScenes[i] = new RuntimeSceneSetup();
                    runtimeScenes[i].path = scenes[i].path;
                    runtimeScenes[i].isActiveScene = scenes[i].isActive;
                }
            }
        }
#endif
    }

