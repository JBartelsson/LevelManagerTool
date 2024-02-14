using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.IO;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView _graphView;
    private static string _filename = "New Narrative";
    [MenuItem("Tools/Level Graph")]
    public static void OpenDialogueGraphWindow(string filename)
    {
        _filename = filename;
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent(Path.GetFileName(filename));
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMinimap();
        GenerateBlackboard();
        RequestDataOperation(false);
    }

    private void GenerateBlackboard()
    {
        var blackboard = new Blackboard(_graphView);
        blackboard.Add(new BlackboardSection { title = "Exposed Properties" });
        blackboard.addItemRequested = _blackboard => { _graphView.AddProptertyToBlackboard(new ExposedProperty()); };
        blackboard.editTextRequested = (blackboard1, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField)element).text;
            if (_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exists", "OK");
                return;
            }

            var propertyIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
            _graphView.ExposedProperties[propertyIndex].PropertyName = newValue;
            ((BlackboardField)element).text = newValue;
        };
        blackboard.SetPosition(new Rect(10, 30, 200, 300));
        _graphView.blackboard = blackboard;
        _graphView.Add(blackboard);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    private void OnDestroy()
    {
        if(EditorUtility.DisplayDialog("Save?", "Want to save the asset?", "yes", "no")){
            RequestDataOperation(true);
        }
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

        toolbar.Add(new Button(() => RequestDataOperation(true)){ text = "Save"});
        
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
        var minimapWidth = 200f;
        var coords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.position.width - minimapWidth - 10, 30));
        minimap.SetPosition(new Rect(coords.x, coords.y, minimapWidth, 140));
        _graphView.Add(minimap);
    }

    

}
