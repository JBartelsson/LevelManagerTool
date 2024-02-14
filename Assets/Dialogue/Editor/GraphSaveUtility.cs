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
    private DialogueContainer _containerCache;

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
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        if(!SaveNodes(dialogueContainer)) return;
        SaveExposedProperties(dialogueContainer);

        AssetDatabase.CreateAsset(dialogueContainer, $"{fileName}");
        AssetDatabase.SaveAssets();
    }

    private void SaveExposedProperties(DialogueContainer dialogueContainer)
    {
        dialogueContainer.ExposedProperties.AddRange(_targetGraphView.ExposedProperties);
    }

    private bool SaveNodes(DialogueContainer dialogueContainer)
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
                portName = connectedPorts[i].output.portName,
                targetNodeguid = inputNode.GUID
            });
        }
        foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
        {
            dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
            {
                guid = dialogueNode.GUID,
                DialogueText = dialogueNode.DialogueText,
                position = dialogueNode.GetPosition().position
            });
        }
        return true;

        
    }
    public void LoadGraph(string fileName)
    {
        Debug.Log(fileName);
        _containerCache = AssetDatabase.LoadAssetAtPath<DialogueContainer>(fileName);
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
            Debug.Log(connections.Count);
            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].targetNodeguid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                Debug.Log((Port)targetNode.inputContainer[0]);
                Debug.Log(Nodes[i].outputContainer.childCount);

                Debug.Log(Nodes[i].outputContainer[j].Q<Port>());
                LinkNodes((Port)targetNode.inputContainer[0], Nodes[i].outputContainer[j].Q<Port>());
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
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText, Vector2.zero);
            tempNode.GUID = nodeData.guid;
            tempNode.SetPosition(new Rect(_containerCache.DialogueNodeData.First(x => x.guid == tempNode.GUID).position, _targetGraphView.defaultNodesize));
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.NodeLinks.Where(x => x.baseNodeguid == nodeData.guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        if (_containerCache.NodeLinks.Count == 0) return;
        Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].baseNodeguid;

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
