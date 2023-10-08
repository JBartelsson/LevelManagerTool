using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;

public class DialogueGraphView : GraphView
{
    private readonly Vector2 defaultNodesize = new Vector2(150, 100);

    public DialogueGraphView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        AddElement(GenerateEntryPointNode());
    }

    private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
    {
        return node.InstantiatePort(orientation, portDirection, capacity, typeof(float));
    }

    private DialogueNode GenerateEntryPointNode()
    {
        var node = new DialogueNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "ENTRYPOINT",
            EntryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.inputContainer.Add(generatedPort);

        node.RefreshExpandedState();
        node.RefreshPorts();
        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    public DialogueNode CreateDialogueNode(string nodename)
    {
        var dialogueNode = new DialogueNode
        {
            title = nodename,
            DialogueText = nodename,
            GUID = Guid.NewGuid().ToString()
        };
        //var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi, Orientation.Vertical);
        //inputPort.portName = "Input1";
        //dialogueNode.titleContainer.Add(inputPort);

        var inputPort2 = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort2.portName = "Input2";
        dialogueNode.inputContainer.Add(inputPort2);

        var inputPort3 = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort3.portName = "Input3";
        dialogueNode.outputContainer.Add(inputPort3);

        var inputPort4 = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi, Orientation.Vertical);
        inputPort4.portName = "Input4";
        
        //dialogueNode.extensionContainer.Add(inputPort4);
        VisualElement v = new VisualElement();
        v.style.backgroundColor = Color.green;
        v.Add(inputPort4);

        var inputPort5 = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi, Orientation.Vertical);
        inputPort5.portName = "Input5";
        inputPort5.style.borderBottomWidth = 5f;
        inputPort5.style.borderBottomColor = Color.black;
        inputPort5.style.color = Color.black;
        inputPort5.style.justifyContent = Justify.Center;
        inputPort5.style.alignItems = Align.Center;
        //dialogueNode.extensionContainer.Add(inputPort4);
        VisualElement v2 = new VisualElement();
        v2.style.backgroundColor = Color.yellow;
        v2.Add(inputPort5);

        dialogueNode.mainContainer.Add(v);
        dialogueNode.mainContainer.Insert(0, v2);



        var button = new Button(() => { AddChoicePort(dialogueNode); });
        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodesize));
        return dialogueNode;
    }

    private void AddChoicePort(DialogueNode dialogueNode)
    {
        var generatedPort = GeneratePort(dialogueNode, Direction.Output);
        var outPutPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
        generatedPort.portName = $"Choice {outPutPortCount}";

        dialogueNode.extensionContainer.Add(generatedPort);
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
    }

    public void CreateNode(string nodename)
    {
        AddElement(CreateDialogueNode(nodename));
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }
}
