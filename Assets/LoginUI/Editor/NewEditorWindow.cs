using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class NewEditorWindow : EditorWindow
{
    [MenuItem("NewEditorWindow/NewEditor")]
    public static void OpenNewEditor()
    {
        CreateInstance<NewEditorWindow>().Show();
    }

    [SerializeField] private VisualTreeAsset template;
    void CreateGUI()
    {
        TemplateContainer templateVisualElement = template.Instantiate();
        Button loginButton = templateVisualElement.Q<Button>();
        loginButton.clicked += LoginButton_clicked;
        rootVisualElement.Add(templateVisualElement);

    }

    private void LoginButton_clicked()
    {
        Debug.Log("Hey");
    }
}
