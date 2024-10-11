using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MySQLConnection : MonoBehaviour
{
    public static MySQLConnection instance;
    string apiUrl;

    readonly string localURL = "http://127.0.0.1:5000/", 
        vercelURL = "https://api-my-sql.vercel.app/";

    [SerializeField] bool localAPI;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        if (localAPI) apiUrl = localURL;
        else apiUrl = vercelURL;
    }

    public void CreateAccount(string nome, string email, string password, Text feedback, CreateAccManager script)
    => StartCoroutine(PostAndGetAccount(nome, email, password, feedback, script));

    public void LoginAccount(string login, string password, Text feedback, LoginManager script)
    => StartCoroutine(PostAndGetInfos(login, password, feedback, script));

    public void CountAcc(PainelAdminScript script) => StartCoroutine(GetCountAcc(script));
    
    public void SelectAccs(PainelAdminScript script, int v1, int v2) => StartCoroutine(GetSelectAccs(script, v1, v2));

    public void BlockAcc(PainelAdminScript script, Button button, string id, bool state) => StartCoroutine(SetBlockAcc(script, button, id, state));

    IEnumerator SetBlockAcc(PainelAdminScript script, Button button, string id, bool state)
    {
        string data = $"{id},{state}";
        byte[] dataToSend = new System.Text.UTF8Encoding().GetBytes(data);

        UnityWebRequest request = new(apiUrl + "blockaccount", "POST")
        {
            uploadHandler = new UploadHandlerRaw(dataToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "text/plain");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            print("Algo deu errado na tentavida de bloquear/desbloquear a conta.");
        }
        else
        {
            Debug.Log("Resposta do servidor: " + request.downloadHandler.text);
            print("Bloqueio/desbloqueio feito com sucesso.");

            script.Atualizar();
        }

        button.interactable = true;
    }

    IEnumerator GetCountAcc(PainelAdminScript script)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"getcountacc");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            Debug.LogError("Tabela vazia.");
        }
        else
        {
            var resultado = request.downloadHandler.text;
            Debug.Log("Resposta do servidor: " + resultado);
            script.SetCount(GetCount(resultado));
        }
    }

    IEnumerator GetSelectAccs(PainelAdminScript script, int v1, int v2)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"getselectacc?v1={v1}&v2={v2}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            Debug.LogError("Nenhuma conta encontrada.");
        }
        else script.SetContas(request.downloadHandler.text);
    }

    IEnumerator PostAndGetInfos(string login, string password, Text feedback, LoginManager script)
    {
        feedback.text = null;
        string jsonData = $"{{\"login\":\"{login}\",\"password\":{password}}}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        UnityWebRequest request = new(apiUrl + "loginacc", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            feedback.text = "Usuário e/ou senha incorreto.";
            feedback.color = Color.red;
        }
        else
        {
            Debug.Log("Resposta do servidor: " + request.downloadHandler.text);
            feedback.text = "Login feito com sucesso.";
            feedback.color = Color.green;

            string[] infos = GetInfos(request.downloadHandler.text);
            for (int i = 0; i < infos.Length; i++) infos[i] = infos[i].Trim();

            int count = 0;
            AccountManager.instance.SetInfos(int.Parse(infos[count++]), infos[count++], infos[count++], infos[count++] == "1", infos[count++] == "1", System.DateTime.Parse(infos[++count]));
            script.ChangeTela();
        }
    }

    IEnumerator PostAndGetAccount(string nome, string email, string password, Text feedback, CreateAccManager script)
    {
        feedback.text = null;
        string jsonData = $"{{\"login\":\"{nome}\",\"email\":{email},\"password\":{password}}}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        yield return StartCoroutine(GetAccount(nome, _request =>
        {
            if (_request.result != UnityWebRequest.Result.Success)
            {
                feedback.text = "Já existe uma conta criada com este usuário.";
                feedback.color = Color.red;
            }
            else StartCoroutine(PostAccount(nome, email, password, feedback, script));
        }));
    }

    IEnumerator PostAccount(string nome, string email, string password, Text feedback, CreateAccManager script)
    {
        feedback.text = null;
        string jsonData = $"{{\"login\":\"{nome}\",\"email\":{email},\"password\":{password}}}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        UnityWebRequest request = new(apiUrl + "createacc", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            feedback.text = "Houve um erro ao criar sua conta.";
            feedback.color = Color.red;
        }
        else
        {
            Debug.Log("Resposta do servidor: " + request.downloadHandler.text);
            feedback.text = "Conta criada com sucesso.";
            feedback.color = Color.green;
            script.ChangeTela();
        }
    }

    IEnumerator GetAccount(string login, System.Action<UnityWebRequest> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"getaccount/{login}");
        yield return request.SendWebRequest();

        callback(request);
    }

    private string[] GetInfos(string infos)
    {
        var temp = infos.Replace("\"", "");
        temp = temp[(temp.IndexOf("[")+1)..];
        temp = temp[..temp.IndexOf("]")];

        return temp.Split(',');
    }

    private int GetCount(string count)
    {
        string temp = "";

        foreach (var item in count)
            if (char.IsDigit(item))
                temp += item;

        return int.Parse(temp);
    }
}