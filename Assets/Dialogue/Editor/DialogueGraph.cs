using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private string _filename = "New Narrative";
    [MenuItem("Tools/Level Graph")]
    public static void OpenDialogueGraphWindow()
    {
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateMinimap();
        GenerateToolbar();
        GenerateClickEvents();
    }

    private void GenerateClickEvents()
    {
        rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
        {
        if (evt.keyCode == KeyCode.Space)
                {
                _graphView.CreateNode("New Node", Event.current.mousePosition);
        }
    });
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }




    private void ConstructGraphView()
    {
        _graphView = new DialogueGraphView(this) { name = "Dialogue Graph" };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name");
        fileNameTextField.SetValueWithoutNotify(_filename);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _filename = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true)){ text = "Save Data"});
        toolbar.Add(new Button(() => RequestDataOperation(false)){ text = "Load Data"});
        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_filename))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid filename", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
        {
            saveUtility.SaveGraph(_filename);
        } else
        {
            saveUtility.LoadGraph(_filename);
        }
    }

    private void GenerateMinimap()
    {
        var minimap = new MiniMap { anchored = true };
        minimap.SetPosition(new Rect(10, 30, 200, 140));
        _graphView.Add(minimap);
    }

}
