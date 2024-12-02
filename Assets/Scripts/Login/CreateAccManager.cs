using UnityEngine;
using UnityEngine.UI;

public class CreateAccManager : MonoBehaviour
{
    MySQLConnection conn;

    [SerializeField] GameObject loginTela;
    [SerializeField] Text feedback;
    [SerializeField] Button createBtn;
    [SerializeField] InputField nicknameInput, emailInput, passwordInput;
    
    bool criando;

    private void Start() => conn = MySQLConnection.instance;

    public void CriarConta()
    {
        if (criando) return;
        if (string.IsNullOrEmpty(nicknameInput.text) || string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text)) return;

        conn.CreateAccount(this, feedback, nicknameInput.text, emailInput.text, passwordInput.text);

        nicknameInput.readOnly = true;
        emailInput.readOnly = true;
        passwordInput.readOnly = true;

        criando = true;
        createBtn.interactable = false;
    }

    public void ChangeTela()
    {
        nicknameInput.readOnly = false;
        emailInput.readOnly = false;
        passwordInput.readOnly = false;

        nicknameInput.text = null;
        emailInput.text = null;
        passwordInput.text = null;

        gameObject.SetActive(false);
        loginTela.SetActive(true);
        criando = false;
    }

    public void ResetInputs()
    {
        nicknameInput.readOnly = false;
        emailInput.readOnly = false;
        passwordInput.readOnly = false;

        createBtn.interactable = true;

        criando = false;
    }

    public void Tab()
    {
        if (nicknameInput.isFocused) emailInput.Select();
        else if (emailInput.isFocused) passwordInput.Select();
        else if (passwordInput.isFocused) nicknameInput.Select();
        else nicknameInput.Select();
    }
}