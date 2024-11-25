using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    MySQLConnection conn;

    [SerializeField] InputField nicknameInput, passwordInput;
    [SerializeField] Text feedback;
    [SerializeField] GameObject infosTela, painelAdmin, criarConta;

    bool conectando;
    readonly string key = "LoginSystem-user";

    private void Awake() => LembrarUsuario(true);
    private void Start() => conn = MySQLConnection.instance;
    private void Update() => Controles();

    public void EntrarConta()
    {
        if (conectando) return;
        if (string.IsNullOrEmpty(nicknameInput.text) || string.IsNullOrEmpty(passwordInput.text)) return;

        conn.LoginAccount(nicknameInput.text, passwordInput.text, feedback, this);

        nicknameInput.readOnly = true;
        passwordInput.readOnly = true;
        conectando = true;
    }

    public void ChangeTela()
    {
        nicknameInput.readOnly = false;
        passwordInput.readOnly = false;
        conectando = false;

        passwordInput.text = null;

        LembrarUsuario();

        gameObject.SetActive(false);
        infosTela.SetActive(true);
        painelAdmin.SetActive(AccountManager.instance.Conta.IsAdmin);
    }

    public void CriarConta()
    {
        gameObject.SetActive(false);
        criarConta.SetActive(true);
    }

    public void MostrarSenha()
    {
        var temp = passwordInput.text;
        passwordInput.text = null;

        if (passwordInput.contentType == InputField.ContentType.Password)
            passwordInput.contentType = InputField.ContentType.Standard;
        else passwordInput.contentType = InputField.ContentType.Password;

        passwordInput.text = temp;
    }

    private void Controles()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (nicknameInput.isFocused) passwordInput.Select();
            else if (passwordInput.isFocused) nicknameInput.Select();
            else nicknameInput.Select();
        }

        if (Input.GetKeyDown(KeyCode.Return)) EntrarConta();
    }

    private void LembrarUsuario(bool awake = false)
    {
        if (!awake) PlayerPrefs.SetString(key, nicknameInput.text);
        else if (PlayerPrefs.HasKey(key)) nicknameInput.text = PlayerPrefs.GetString(key);

        passwordInput.Select();
    } 
}