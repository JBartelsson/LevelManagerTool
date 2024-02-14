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
    private DialogueGraphView _targetGraphView;
    private SceneTransitions _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        var dialogueContainer = ScriptableObject.CreateInstance<SceneTransitions>();
        if(!SaveNodes(dialogueContainer)) return;
        SaveExposedProperties(dialogueContainer);

        SceneTransitions sceneTransition = AssetDatabase.LoadAssetAtPath<SceneTransitions>(fileName);
        sceneTransition.Renew(dialogueContainer.DialogueNodeData, dialogueContainer.NodeLinks, dialogueContainer.ExposedProperties);
        AssetDatabase.SaveAssets();
    }

    private void SaveExposedProperties(SceneTransitions dialogueContainer)
    {
        dialogueContainer.ExposedProperties.AddRange(_targetGraphView.ExposedProperties);
    }

    private bool SaveNodes(SceneTransitions dialogueContainer)
    {
        if (!Edges.Any()) return false; //If there is no connection, return

        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.NodeLinks.Add(new NodeLinkData()
            {
                baseNodeguid = outputNode.GUID,
                basePortName = connectedPorts[i].output.portName,
                targetPortName = connectedPorts[i].input.portName,
                targetNodeguid = inputNode.GUID
            });
        }
        foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
        {
            dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
            {
                guid = dialogueNode.GUID,
                scenePath = dialogueNode.sceneName,
                position = dialogueNode.GetPosition().position
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
            EditorUtility.DisplayDialog("File not Found", "Target dialogue graph file does not exist!", "OK");
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
                Debug.Log($"{connections[j].basePortName} finds {basePort}");

                Port targetPort = targetNode.Q<Port>(connections[j].targetPortName);
                Debug.Log($"{connections[j].targetPortName} finds {targetPort}");
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
        foreach (var nodeData in _containerCache.DialogueNodeData)
        {
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.scenePath, Vector2.zero);
            tempNode.GUID = nodeData.guid;
            tempNode.SetPosition(new Rect(_containerCache.DialogueNodeData.First(x => x.guid == tempNode.GUID).position, _targetGraphView.defaultNodesize));
            _targetGraphView.AddElement(tempNode);

        }
    }

    private void ClearGraph()
    {
        if (_containerCache.NodeLinks.Count == 0) return;
        //Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].baseNodeguid;

        foreach(var node in Nodes)
        {
            if (node.EntryPoint) continue;
            //Remove edges connected to the node
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            //remove node
            _targetGraphView.RemoveElement(node);
        }

    }
}
