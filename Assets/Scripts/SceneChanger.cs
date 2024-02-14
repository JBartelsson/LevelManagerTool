using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] SceneTransitions sceneTransitions;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            sceneTransitions.ReceiveScenePath(SceneManager.GetActiveScene().path, SceneTransitions.ExitDirection.Right, out string newScenePath);
            return;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            sceneTransitions.ReceiveScenePath(SceneManager.GetActiveScene().path, SceneTransitions.ExitDirection.Top, out string newScenePath);
            return;

        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            sceneTransitions.ReceiveScenePath(SceneManager.GetActiveScene().path, SceneTransitions.ExitDirection.Left, out string newScenePath);
            return;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            sceneTransitions.ReceiveScenePath(SceneManager.GetActiveScene().path, SceneTransitions.ExitDirection.Bottom, out string newScenePath);
            return;
        }
    }
}
