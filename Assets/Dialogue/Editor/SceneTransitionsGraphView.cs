using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;

public class SceneTransitionsGraphView : GraphView
{
    public readonly Vector2 defaultNodesize = new Vector2(150, 100);
    private readonly Vector2 defaultPosition = new Vector2(0f, 0f);
    public Blackboard blackboard;
    private NodeSearchWindow nodeSearchWindow;
    private List<ExposedProperty> exposedProperties = new();

    public List<ExposedProperty> ExposedProperties { get => exposedProperties; set => exposedProperties = value; }

    

    private List<string> scenes = new List<string>();

    public SceneTransitionsGraphView(EditorWindow editorWindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        //AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);

        scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
                Debug.Log(scene.path);
            }
        }

    }

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        nodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        nodeSearchWindow.Init(editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), nodeSearchWindow);
    }

    private Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal)
    {
        return node.InstantiatePort(orientation, portDirection, capacity, typeof(float));
    }

    private SceneTransitionNode GenerateEntryPointNode()
    {
        var node = new SceneTransitionNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
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
    public SceneTransitionNode CreateTransitionNodeGraphic(string scenePath, Vector2? position = null)
    {
        if (scenes.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No Scenes in Build Settings. Please add Scenes in order to use this tool.", "Ok");
            return null;
        }
        if (nodes.Count() == scenes.Count)
        {
            EditorUtility.DisplayDialog("Error", "No scenes left. Add more scenes to create another node.", "Ok");
            return null;
        }
        if (!position.HasValue) position = Vector2.zero;
        if (scenePath == "")
        {
            scenePath = scenes[0];
        } 

        var dialogueNode = new SceneTransitionNode
        {
            title = scenePath,
            sceneName = scenePath,
            GUID = Guid.NewGuid().ToString()
        };
        var leftPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Single, Orientation.Horizontal);
        leftPort.portName = SceneTransitions.LEFT_PORT_NAME;
        leftPort.name = SceneTransitions.LEFT_PORT_NAME;

        dialogueNode.inputContainer.Add(leftPort);

        var rightPort = GeneratePort(dialogueNode, Direction.Output, Port.Capacity.Single);
        rightPort.portName = SceneTransitions.RIGHT_PORT_NAME;
        rightPort.name = SceneTransitions.RIGHT_PORT_NAME;
        dialogueNode.outputContainer.Add(rightPort);

        var bottomPort = GeneratePort(dialogueNode, Direction.Output, Port.Capacity.Single, Orientation.Vertical);
        bottomPort.portName = SceneTransitions.BOTTOM_PORT_NAME;
        bottomPort.name = SceneTransitions.BOTTOM_PORT_NAME;


        dialogueNode.extensionContainer.Add(bottomPort);
        VisualElement topContainer = new VisualElement();
        topContainer.style.backgroundColor = Color.green;
        topContainer.Add(bottomPort);

        var topPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Single, Orientation.Vertical);
        topPort.portName = SceneTransitions.TOP_PORT_NAME;
        topPort.name = SceneTransitions.TOP_PORT_NAME;

        topPort.style.borderBottomWidth = 5f;
        topPort.style.borderBottomColor = Color.black;
        topPort.style.color = Color.black;
        topPort.style.justifyContent = Justify.Center;
        bottomPort.style.justifyContent = Justify.Center;

        topPort.style.alignItems = Align.Center;
        bottomPort.style.alignItems = Align.Center;
        //dialogueNode.extensionContainer.Add(inputPort4);
        VisualElement bottomContainer = new VisualElement();
        bottomContainer.style.backgroundColor = Color.yellow;
        bottomContainer.Add(topPort);

        DropdownField dropDown = new DropdownField();
        dropDown.choices = scenes;
        dropDown.RegisterValueChangedCallback((evt) =>
        {
            dialogueNode.sceneName = evt.newValue;
        });
        if (scenes.Contains(scenePath))
        {
            dropDown.index = scenes.IndexOf(scenePath);
        } else
        {
            dropDown.index = 0;
        }
        

        dialogueNode.mainContainer.Add(topContainer);
        dialogueNode.mainContainer.Insert(0, bottomContainer);
        dialogueNode.titleContainer.Add(dropDown);

        dialogueNode.outputContainer.style.backgroundColor = Color.red;
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(position.Value, defaultNodesize));

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        return dialogueNode;
    }

    private SceneConditionNode CreateConditionNodeGraphic()
    {
        SceneConditionNode sceneConditionNode = new SceneConditionNode
        {
            GUID = Guid.NewGuid().ToString()
        };

        var leftPort = GeneratePort(sceneConditionNode, Direction.Input, Port.Capacity.Single, Orientation.Horizontal);
        leftPort.portName = SceneTransitions.LEFT_PORT_NAME;
        leftPort.name = SceneTransitions.LEFT_PORT_NAME;

        sceneConditionNode.inputContainer.Add(leftPort);

        var rightPort = GeneratePort(sceneConditionNode, Direction.Output, Port.Capacity.Single);
        rightPort.portName = SceneTransitions.RIGHT_PORT_NAME;
        rightPort.name = SceneTransitions.RIGHT_PORT_NAME;
        sceneConditionNode.outputContainer.Add(rightPort);
        return sceneConditionNode;
    }


    private void RemovePort(SceneTransitionNode dialogueNode, Port generatedPort)
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

    public void CreateTransitionNode(string nodename, Vector2 position)
    {
        AddElement(CreateTransitionNodeGraphic(nodename, position));
    }

    public void CreateConditionNode()
    {
        AddElement(CreateConditionNodeGraphic());
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
