using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]
[CreateAssetMenu]
public class SceneTransitions : ScriptableObject
{
    public static string LEFT_PORT_NAME = "Left Exit";
    public static string RIGHT_PORT_NAME = "Right Exit";
    public static string TOP_PORT_NAME = "Top Exit";
    public static string BOTTOM_PORT_NAME = "Bottom Exit";

    [HideInNormalInspector]
    [SerializeField] private List<SceneTransitionNodeData> sceneTransitionNodeData = new();
    [HideInNormalInspector]
    [SerializeField] private List<SceneConditionNodeData> sceneConditionNodeData = new();
    [HideInNormalInspector]
    [SerializeField] private List<NodeLinkData> nodeLinks = new();
    [HideInNormalInspector]
    [SerializeField] private List<ExposedProperty> exposedProperties = new();

    public List<SceneTransitionNodeData> SceneTransitionsNodeData { get => sceneTransitionNodeData; set => sceneTransitionNodeData = value; }
    public List<NodeLinkData> NodeLinks { get => nodeLinks; set => nodeLinks = value; }
    public List<ExposedProperty> ExposedProperties { get => exposedProperties; set => exposedProperties = value; }
    public List<SceneConditionNodeData> SceneConditionNodeData { get => sceneConditionNodeData; set => sceneConditionNodeData = value; }

    public void Renew(List<SceneTransitionNodeData> newSceneTransitionNodeData, List<SceneConditionNodeData> newSceneConditionNodeData, List<NodeLinkData> newNodeLinkData, List<ExposedProperty> newExposedProperties)
    {
        this.sceneTransitionNodeData = newSceneTransitionNodeData;
        this.sceneConditionNodeData = newSceneConditionNodeData;
        this.nodeLinks = newNodeLinkData;
        this.exposedProperties = newExposedProperties;
    }

    public enum ExitDirection
    {
        Left,
        Right,
        Top,
        Bottom
    }
    public bool ReceiveScenePath(string currentScenePath, ExitDirection exitDirection, out string newScenePath)
    {

        SceneTransitionNodeData currentNodeData = SceneTransitionsNodeData.First((x) => x.scenePath == currentScenePath);
        SceneTransitionNodeData nextDialogueNodeData;
        NodeLinkData nodeLink;
        newScenePath = "";
        switch (exitDirection)
        {
            case ExitDirection.Left:
                nodeLink = nodeLinks.FirstOrDefault((x) => x.baseNodeguid == currentNodeData.guid && x.basePortName == LEFT_PORT_NAME);
                if (nodeLink == null) return false;
                nextDialogueNodeData = SceneTransitionsNodeData.First((x) => x.guid == nodeLink.targetNodeguid);
                newScenePath = nextDialogueNodeData.scenePath;
                return true;
            case ExitDirection.Right:
                nodeLink = nodeLinks.FirstOrDefault((x) => x.baseNodeguid == currentNodeData.guid && x.basePortName == RIGHT_PORT_NAME);
                if (nodeLink == null) return false; 
                Debug.Log(nodeLink);
                nextDialogueNodeData = SceneTransitionsNodeData.FirstOrDefault((x) => x.guid == nodeLink.targetNodeguid);
                Debug.Log(SceneTransitionsNodeData);
                newScenePath = nextDialogueNodeData.scenePath;
                return true;

            case ExitDirection.Top:
                nodeLink = nodeLinks.FirstOrDefault((x) => x.baseNodeguid == currentNodeData.guid && x.basePortName == TOP_PORT_NAME);
                if (nodeLink == null) return false;
                nextDialogueNodeData = SceneTransitionsNodeData.First((x) => x.guid == nodeLink.targetNodeguid);
                newScenePath = nextDialogueNodeData.scenePath;
                return true;

            case ExitDirection.Bottom:
                nodeLink = nodeLinks.FirstOrDefault((x) => x.baseNodeguid == currentNodeData.guid && x.basePortName == BOTTOM_PORT_NAME);
                if (nodeLink == null) return false;
                nextDialogueNodeData = SceneTransitionsNodeData.First((x) => x.guid == nodeLink.targetNodeguid);
                newScenePath = nextDialogueNodeData.scenePath;
                return true;

            default:
                newScenePath = "";
                return false;
        }
    }
}