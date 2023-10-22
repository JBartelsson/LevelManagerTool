using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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
        if (!Edges.Any()) return; //If there is no connection, return

        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.nodeLinks.Add(new NodeLinkData()
            {
                baseNodeguid = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeguid = inputNode.GUID
            });
        }
        foreach (var dialogueNode in Nodes.Where(node => !node.EntryPoint))
        {
            dialogueContainer.dialogueNodeData.Add(new DialogueNodeData
            {
                guid = dialogueNode.GUID,
                DialogueText = dialogueNode.DialogueText,
                position = dialogueNode.GetPosition().position
            });
        }

        //Auto creates resource folder if exists
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }
    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<DialogueContainer>(fileName);
        if(_containerCache == null)
        {
            EditorUtility.DisplayDialog("File not Found", "Target dialogue graph file does not exist!", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ConnectNodes()
    {
        throw new NotImplementedException();
    }

    private void CreateNodes()
    {
        foreach (var nodeData in _containerCache.dialogueNodeData)
        {
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.DialogueText);
            tempNode.GUID = nodeData.guid;
            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeguid == nodeData.guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ClearGraph()
    {
        Nodes.Find(x => x.EntryPoint).GUID = _containerCache.nodeLinks[0].baseNodeguid;

        foreach(var node in Nodes)
        {
            if (node.EntryPoint) return;
            //Remove edges connected to the node
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            //remove node
            _targetGraphView.RemoveElement(node);
        }

    }
}
