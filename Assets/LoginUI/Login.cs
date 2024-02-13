using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Login : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;

    public UnityEvent OnLoginPerformed;
    List<LoginData> loginDatas = new();
    VisualElement view;
    TextField usernameTextField;
    TextField passwordTextField;
    Label successLabel;
    class LoginData
    {
        public string username;
        public string password;

        public LoginData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        loginDatas.Add(new LoginData("johann", "1234"));
        loginDatas.Add(new LoginData("luc", "1234"));
        loginDatas.Add(new LoginData("jana", "12345"));
        view = uIDocument.rootVisualElement;
        usernameTextField = view.Q<TextField>("username");
        passwordTextField  = view.Q<TextField>("password");
        successLabel = view.Q<Label>("errorlabel");
        Button loginButton = view.Q<Button>("loginbutton");
        loginButton.clicked += LoginButton_clicked;
    }

    private void LoginButton_clicked()
    {
        string username = usernameTextField.text;
        string password = passwordTextField.text;

        bool success = false;
        foreach (LoginData _loginData in loginDatas)
        {
            if (username == _loginData.username && password == _loginData.password)
            {
                successLabel.text = "Success!";
                successLabel.style.backgroundColor = Color.green;
                OnLoginPerformed?.Invoke();
                success = true;
                break;
            }
        }
        if (!success)
        {
            successLabel.text = "Error1111";
            successLabel.style.backgroundColor = Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
