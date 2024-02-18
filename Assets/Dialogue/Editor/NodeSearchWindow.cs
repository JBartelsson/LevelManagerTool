using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private SceneTransitionsGraphView _graphView;
    private EditorWindow editorWindow;
    private Texture2D _indentationIcon;

    

    public void Init(EditorWindow editorWindow, SceneTransitionsGraphView graphView)
    {
        this._graphView = graphView;
        this.editorWindow = editorWindow;
        _indentationIcon = new Texture2D(1,1);
        _indentationIcon.SetPixel(0,0, new Color(0,0,0,0));
        _indentationIcon.Apply();
    }
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements")),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            new SearchTreeEntry(new GUIContent("Dialogue Node", _indentationIcon))
            {
                userData = new SceneTransitionNode(), level = 2
            },
            new SearchTreeEntry(new GUIContent("Condition", _indentationIcon))
            {
                userData = new SceneConditionNode(), level = 2
            },
            new SearchTreeEntry(new GUIContent("Trigger", _indentationIcon))
            {
                userData = null, level = 2
            }

        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldMousePos = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
        var localMousePosition = _graphView.contentContainer.WorldToLocal(worldMousePos);
        switch (SearchTreeEntry.userData)
        {
            case SceneTransitionNode:
                _graphView.CreateTransitionNode("", localMousePosition);
                return true;
            case SceneConditionNode:
                _graphView.CreateConditionNode(null, false, localMousePosition);
                return true;
            default:
                return false;
        }
    }
}
