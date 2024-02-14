using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodesize = new Vector2(150, 100);
    private readonly Vector2 defaultPosition = new Vector2(0f, 0f);
    public Blackboard blackboard;
    private NodeSearchWindow nodeSearchWindow;
    private List<ExposedProperty> exposedProperties = new();

    public List<ExposedProperty> ExposedProperties { get => exposedProperties; set => exposedProperties = value; }

    public DialogueGraphView(EditorWindow editorWindow)
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
        AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        nodeSearchWindow.Init(editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), nodeSearchWindow);
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
        node.outputContainer.Add(generatedPort);

        node.RefreshExpandedState();
        node.RefreshPorts();
        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }
    //Creates the DialogueNode Visual
    public DialogueNode CreateDialogueNode(string nodename, Vector2? position = null)
    {
        if (!position.HasValue) position = Vector2.zero;

        var dialogueNode = new DialogueNode
        {
            title = nodename,
            DialogueText = nodename,
            GUID = Guid.NewGuid().ToString()
        };
        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi, Orientation.Horizontal);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        var inputPort2 = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort2.portName = "Input2";
        dialogueNode.inputContainer.Add(inputPort2);

        var inputPort3 = GeneratePort(dialogueNode, Direction.Output, Port.Capacity.Multi);
        inputPort3.portName = "Input3";
        dialogueNode.outputContainer.Add(inputPort3);

        var inputPort4 = GeneratePort(dialogueNode, Direction.Output, Port.Capacity.Multi, Orientation.Vertical);
        inputPort4.portName = "Input4";

        dialogueNode.extensionContainer.Add(inputPort4);
        VisualElement topContainer = new VisualElement();
        topContainer.style.backgroundColor = Color.green;
        topContainer.Add(inputPort4);

        var inputPort5 = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi, Orientation.Vertical);
        inputPort5.portName = "Input5";
        inputPort5.style.borderBottomWidth = 5f;
        inputPort5.style.borderBottomColor = Color.black;
        inputPort5.style.color = Color.black;
        inputPort5.style.justifyContent = Justify.Center;
        inputPort5.style.alignItems = Align.Center;
        //dialogueNode.extensionContainer.Add(inputPort4);
        VisualElement bottomContainer = new VisualElement();
        bottomContainer.style.backgroundColor = Color.yellow;
        bottomContainer.Add(inputPort5);

        dialogueNode.mainContainer.Add(topContainer);
        dialogueNode.mainContainer.Insert(0, bottomContainer);



        var button = new Button(() => { AddChoicePort(dialogueNode); });
        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);

        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueText = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.mainContainer.Add(textField);

        dialogueNode.outputContainer.style.backgroundColor = Color.red;
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position.Value, defaultNodesize));

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        return dialogueNode;
    }

    public void AddChoicePort(DialogueNode dialogueNode, string overiddenPortname = "")
    {
        var generatedPort = GeneratePort(dialogueNode, Direction.Output);

        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        //generatedPort.contentContainer.Remove(oldLabel);
        oldLabel.style.display = DisplayStyle.None;

        var outPutPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;


        var choicePortname = string.IsNullOrEmpty(overiddenPortname) ? $"Choice {outPutPortCount}" : overiddenPortname;


        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortname
        };
        textField.style.minWidth = 60;
        textField.style.maxWidth = 100;

        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);

        var deleteButton = new Button(() => RemovePort(dialogueNode, generatedPort))
        {
            text = "X"
        };


        generatedPort.contentContainer.Add(deleteButton);

        generatedPort.portName = choicePortname;
        dialogueNode.outputContainer.Add(generatedPort);
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
    }

    private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        dialogueNode.outputContainer.Remove(generatedPort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    public void CreateNode(string nodename, Vector2 position)
    {
        AddElement(CreateDialogueNode(nodename, position));
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

    public void ClearBlackBoardAndExposedProperties()
    {
        ExposedProperties.Clear();
        blackboard.Clear();
    }
    public void AddProptertyToBlackboard(ExposedProperty exposedProperty)
    {
        var localPropertyName = exposedProperty.PropertyName;
        var localPropertyValue = exposedProperty.PropertyValue;
        while(exposedProperties.Any(x => x.PropertyName == localPropertyName))
        {
            localPropertyName = $"{localPropertyName}(1)";
        }

        var property = new ExposedProperty();
        property.PropertyName = localPropertyName;
        property.PropertyValue = exposedProperty.PropertyValue;
        Debug.Log(property);
        exposedProperties.Add(property);

        var container = new VisualElement();
        var blackboardField = new BlackboardField { text = property.PropertyName, typeText = "string Property" };
        container.Add(blackboardField);
        var propertyValueTextField = new TextField("Value:")
        {
            value = localPropertyValue
        };
        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = exposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
            exposedProperties[changingPropertyIndex].PropertyValue = evt.newValue;
        });
        var blackBoardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
        container.Add(blackBoardValueRow);
        blackboard.Add(container);
    }
}
