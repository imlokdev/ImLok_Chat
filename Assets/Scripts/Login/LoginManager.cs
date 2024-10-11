using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    MySQLConnection conn;

    [SerializeField] InputField nicknameInput, passwordInput;
    [SerializeField] Text feedback;
    [SerializeField] GameObject infosTela, painelAdmin;

    private void Start() => conn = MySQLConnection.instance;

    public void EntrarConta()
    {
        if (string.IsNullOrEmpty(nicknameInput.text) || string.IsNullOrEmpty(passwordInput.text)) return;
        conn.LoginAccount(nicknameInput.text, passwordInput.text, feedback, this);
        nicknameInput.text = null;
        passwordInput.text = null;
    }

    public void ChangeTela()
    {
        gameObject.SetActive(false);
        infosTela.SetActive(true);
        painelAdmin.SetActive(AccountManager.instance.IsAdmin);
    }
}