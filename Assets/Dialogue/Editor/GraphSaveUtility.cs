using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility : MonoBehaviour
{
    private SceneTransitionsGraphView _targetGraphView;
    private SceneTransitions _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<SceneTransitionNode> sceneTransitionNodes = new();
    private List<SceneConditionNode> sceneConditionNodes = new();

    private List<BasicNode> Nodes => _targetGraphView.nodes.ToList().Cast<BasicNode>().ToList();
    public static GraphSaveUtility GetInstance(SceneTransitionsGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    private void GetNodes()
    {
        foreach (Node node in _targetGraphView.nodes)
        {
            if (node is SceneTransitionNode)
            {
                sceneTransitionNodes.Add((SceneTransitionNode) node);
                continue;
            }
            if (node is SceneConditionNode)
            {
                Debug.Log("Scene Condition");
                sceneConditionNodes.Add((SceneConditionNode)node);
                continue;
            }
        }
    }

    public void SaveGraph(string fileName)
    {
        GetNodes();
        var dialogueContainer = ScriptableObject.CreateInstance<SceneTransitions>();
        if(!SaveNodes(dialogueContainer)) return;
        Debug.Log("Saving Scriptable Object");
        SaveExposedProperties(dialogueContainer);

        SceneTransitions sceneTransition = AssetDatabase.LoadAssetAtPath<SceneTransitions>(fileName);
        sceneTransition.Renew(dialogueContainer.SceneTransitionsNodeData, dialogueContainer.SceneConditionNodeData, dialogueContainer.NodeLinks, dialogueContainer.ExposedProperties);
        EditorUtility.SetDirty(sceneTransition);
        AssetDatabase.SaveAssets();
    }

    private void SaveExposedProperties(SceneTransitions sceneTransitionsContainer)
    {
        sceneTransitionsContainer.ExposedProperties.AddRange(_targetGraphView.ExposedProperties);
    }

    private bool SaveNodes(SceneTransitions sceneTransitionsContainer)
    {
        if (!Edges.Any()) return false; //If there is no connection, return

        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as BasicNode;
            var inputNode = connectedPorts[i].input.node as BasicNode;

            sceneTransitionsContainer.NodeLinks.Add(new NodeLinkData()
            {
                baseNodeguid = outputNode.GUID,
                basePortName = connectedPorts[i].output.portName,
                targetPortName = connectedPorts[i].input.portName,
                targetNodeguid = inputNode.GUID
            });
        }
        foreach (var sceneTransitionsNode in sceneTransitionNodes)
        {
            sceneTransitionsContainer.SceneTransitionsNodeData.Add(new SceneTransitionNodeData
            {
                guid = sceneTransitionsNode.GUID,
                scenePath = sceneTransitionsNode.sceneName,
                position = sceneTransitionsNode.GetPosition().position
            });
        }
        foreach (var sceneConditionNode in sceneConditionNodes)
        {
            sceneTransitionsContainer.SceneConditionNodeData.Add(new SceneConditionNodeData
            {
                guid = sceneConditionNode.GUID,
                property = sceneConditionNode.property,
                necessaryState = sceneConditionNode.necessaryState,
                position = sceneConditionNode.GetPosition().position
            });
        }
        return true;

        
    }
    public void LoadGraph(string fileName)
    {
        Debug.Log(fileName);
        _containerCache = AssetDatabase.LoadAssetAtPath<SceneTransitions>(fileName);
        if(_containerCache == null)
        {
            EditorUtility.DisplayDialog("File not Found", "Target Scene Transitions file does not exist!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        CreateExposedProperties();
    }

    private void CreateExposedProperties()
    {
        _targetGraphView.ClearBlackBoardAndExposedProperties();
        foreach(var exposedProperty in _containerCache.ExposedProperties)
        {
            _targetGraphView.AddProptertyToBlackboard(exposedProperty);
        }
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            var connections = _containerCache.NodeLinks.Where(x => x.baseNodeguid == Nodes[i].GUID).ToList();
            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].targetNodeguid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                Port basePort = Nodes[i].Q<Port>(connections[j].basePortName);

                Port targetPort = targetNode.Q<Port>(connections[j].targetPortName);
                LinkNodes(targetPort, basePort);
            }
        }
    }

    private void LinkNodes(Port input, Port output)
    {
        var tempEdge = new Edge()
        {
            input = input,
            output = output
        };
        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);

    }

    private void CreateNodes()
    {
        foreach (var sceneTransitionNodeData in _containerCache.SceneTransitionsNodeData)
        {
            var tempNode = _targetGraphView.CreateTransitionNodeGraphic(sceneTransitionNodeData.scenePath, Vector2.zero);
            tempNode.GUID = sceneTransitionNodeData.guid;
            tempNode.SetPosition(new Rect(sceneTransitionNodeData.position, _targetGraphView.defaultTransitionNodeSize));
            _targetGraphView.AddElement(tempNode);

        }

        foreach (var sceneConditionNodeData in _containerCache.SceneConditionNodeData)
        {
            var tempNode = _targetGraphView.CreateConditionNodeGraphic(sceneConditionNodeData.property, sceneConditionNodeData.necessaryState, Vector2.zero);
            tempNode.GUID = sceneConditionNodeData.guid;
            tempNode.SetPosition(new Rect(sceneConditionNodeData.position, _targetGraphView.defaultTransitionNodeSize));
            _targetGraphView.AddElement(tempNode);

        }
    }

    private void ClearGraph()
    {

        foreach(var node in Nodes)
        {
            //Remove edges connected to the node
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            //remove node
            _targetGraphView.RemoveElement(node);
        }

    }
}
