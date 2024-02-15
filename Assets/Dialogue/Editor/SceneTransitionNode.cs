using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

public class SceneTransitionNode : Node
{
    public string GUID;

    public string sceneName;
    public bool EntryPoint = false;
}
