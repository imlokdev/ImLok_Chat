using UnityEngine;
using UnityEngine.UI;

public class CreateAccManager : MonoBehaviour
{
    MySQLConnection conn;

    [SerializeField] InputField nicknameInput, emailInput, passwordInput;
    [SerializeField] Text feedback;
    [SerializeField] GameObject loginTela;

    private void Start() => conn = MySQLConnection.instance;

    public void CriarConta()
    {
        if (string.IsNullOrEmpty(nicknameInput.text) || string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text)) return;
        conn.CreateAccount(nicknameInput.text, emailInput.text, passwordInput.text, feedback, this);
        nicknameInput.text = null;
        emailInput.text = null;
        passwordInput.text = null;
    }

    public void ChangeTela()
    {
        gameObject.SetActive(false);
        loginTela.SetActive(true);
    }
}