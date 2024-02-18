using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SceneTransitionsGraphView : GraphView
{
    private const string TRIGGER_LEFT_PORT_NAME = "Scene Left";
    private const string TRIGGER_RIGHT_PORT_NAME = "Scene Right";
    private const string DROPDOWN_NAME = "Dropdown-Container";
    private const string TOGGLE_NAME = "Toggle-Container";

    public readonly Vector2 defaultTransitionNodeSize = new Vector2(150, 100);
    public readonly Vector2 defaultConditionNodeSize = new Vector2(150, 70);
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

        var sceneTransitionNode = new SceneTransitionNode
        {
            title = scenePath,
            sceneName = scenePath,
            GUID = Guid.NewGuid().ToString()
        };
        var leftPort = GeneratePort(sceneTransitionNode, Direction.Input, Port.Capacity.Single, Orientation.Horizontal);
        leftPort.portName = SceneTransitions.LEFT_PORT_NAME;
        leftPort.name = SceneTransitions.LEFT_PORT_NAME;

        sceneTransitionNode.inputContainer.Add(leftPort);

        var rightPort = GeneratePort(sceneTransitionNode, Direction.Output, Port.Capacity.Single);
        rightPort.portName = SceneTransitions.RIGHT_PORT_NAME;
        rightPort.name = SceneTransitions.RIGHT_PORT_NAME;
        sceneTransitionNode.outputContainer.Add(rightPort);

        var bottomPort = GeneratePort(sceneTransitionNode, Direction.Output, Port.Capacity.Single, Orientation.Vertical);
        bottomPort.portName = SceneTransitions.BOTTOM_PORT_NAME;
        bottomPort.name = SceneTransitions.BOTTOM_PORT_NAME;


        sceneTransitionNode.extensionContainer.Add(bottomPort);
        VisualElement bottomContainer = new VisualElement();
        bottomContainer.style.backgroundColor = Color.green;
        bottomContainer.Add(bottomPort);

        var topPort = GeneratePort(sceneTransitionNode, Direction.Input, Port.Capacity.Single, Orientation.Vertical);
        topPort.portName = SceneTransitions.TOP_PORT_NAME;
        topPort.name = SceneTransitions.TOP_PORT_NAME;

        
        topPort.style.justifyContent = Justify.Center;
        bottomPort.style.justifyContent = Justify.Center;

        topPort.style.alignItems = Align.Center;
        bottomPort.style.alignItems = Align.Center;
        VisualElement topContainer = new VisualElement();
        topContainer.style.backgroundColor = Color.yellow;
        topContainer.Add(topPort);

        DropdownField dropDown = new DropdownField();
        dropDown.choices = scenes;
        dropDown.RegisterValueChangedCallback((evt) =>
        {
            sceneTransitionNode.sceneName = evt.newValue;
        });
        if (scenes.Contains(scenePath))
        {
            dropDown.index = scenes.IndexOf(scenePath);
        } else
        {
            dropDown.index = 0;
        }
        

        sceneTransitionNode.mainContainer.Add(bottomContainer);
        sceneTransitionNode.mainContainer.Insert(0, topContainer);
        sceneTransitionNode.titleContainer.Add(dropDown);

        sceneTransitionNode.outputContainer.style.backgroundColor = Color.red;
        sceneTransitionNode.RefreshExpandedState();
        sceneTransitionNode.RefreshPorts();
        sceneTransitionNode.SetPosition(new Rect(position.Value, defaultTransitionNodeSize));

        sceneTransitionNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        return sceneTransitionNode;
    }

    public SceneConditionNode CreateConditionNodeGraphic(ExposedProperty exposedProperty, bool necessaryState, Vector2 position)
    {
        SceneConditionNode sceneConditionNode = new SceneConditionNode
        {
            GUID = Guid.NewGuid().ToString(),
            title = "Condition"
        };

        var leftPort = GeneratePort(sceneConditionNode, Direction.Input, Port.Capacity.Single, Orientation.Horizontal);
        leftPort.portName = TRIGGER_LEFT_PORT_NAME;
        leftPort.name = TRIGGER_LEFT_PORT_NAME;

        var rightPort = GeneratePort(sceneConditionNode, Direction.Output, Port.Capacity.Single);
        rightPort.portName = TRIGGER_RIGHT_PORT_NAME;
        rightPort.name = TRIGGER_RIGHT_PORT_NAME;
        //DropDown Field
        DropdownField dropDown = new DropdownField();
        List<string> dropDownChoices = new();
        foreach (var item in exposedProperties)
        {
            dropDownChoices.Add(item.PropertyName);
        }
        dropDown.choices = dropDownChoices;
        dropDown.RegisterValueChangedCallback((evt) =>
        {
            sceneConditionNode.property = exposedProperties.FirstOrDefault((x) => x.PropertyName == evt.newValue);
        });
        VisualElement dropDownContainer = new VisualElement();
        Label conditionLabel = new Label();
        conditionLabel.text = "Condition";
        dropDownContainer.name = DROPDOWN_NAME;
        dropDownContainer.Add(conditionLabel);
        dropDownContainer.Add(dropDown);

        VisualElement toggleContainer = new VisualElement();
        toggleContainer.name = TOGGLE_NAME;
        Label necessaryStateLabel = new Label();
        necessaryStateLabel.text = "Necessary State:";
        UnityEngine.UIElements.Toggle toggle = new UnityEngine.UIElements.Toggle();
        toggle.RegisterValueChangedCallback((evt) =>
        {
            sceneConditionNode.necessaryState = evt.newValue;
        });
        toggleContainer.Add(necessaryStateLabel);
        toggleContainer.Add(toggle);

        sceneConditionNode.inputContainer.Add(leftPort);

        
        sceneConditionNode.contentContainer.Add(dropDownContainer);
        sceneConditionNode.contentContainer.Add(toggleContainer);
        sceneConditionNode.outputContainer.Add(rightPort);
        sceneConditionNode.SetPosition(new Rect(position, defaultConditionNodeSize));
        sceneConditionNode.styleSheets.Add(Resources.Load<StyleSheet>("ConditionNode"));

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

    public void CreateTransitionNode(string scenePath, Vector2 position)
    {
        AddElement(CreateTransitionNodeGraphic(scenePath, position));
    }

    public void CreateConditionNode(ExposedProperty exposedProperty, bool necessaryState, Vector2 position)
    {
        AddElement(CreateConditionNodeGraphic(exposedProperty, necessaryState, position));
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
