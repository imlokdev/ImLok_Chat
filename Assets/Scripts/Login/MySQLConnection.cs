using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Leguar.TotalJSON;
using System.Collections.Generic;

public class MySQLConnection : MonoBehaviour
{
    public static MySQLConnection instance;
    SessionManager sessionManager;

    string apiUrl;

    readonly string localURL = "http://127.0.0.1:5000/", 
        vercelURL = "https://api-my-sql.vercel.app/";

    [SerializeField] bool localAPI;

    private void Awake()
    {
        if (instance == null) instance = this;
        sessionManager = GetComponent<SessionManager>();
    }

    private void Update()
    {
        if (localAPI) apiUrl = localURL;
        else apiUrl = vercelURL;
    }

    public void CreateAccount(CreateAccManager script, Text feedback, string nome, string email, string password)
    => StartCoroutine(PostAccount(script, feedback, nome, email, password));

    public void LoginAccount(LoginManager script, Text feedback, string login, string password)
    => StartCoroutine(PostAndGetInfos(script, feedback, login, password));

    public void CountAcc(PainelAdminScript script) => StartCoroutine(GetCountAcc(script));
    
    public void SelectAccs(PainelAdminScript script, int v1, int v2) => StartCoroutine(GetSelectAccs(script, v1, v2));

    public void BlockAcc(PainelAdminScript script, Button button, string id, string state) => StartCoroutine(SetBlockAcc(script, button, id, state));

    public void BanAcc(PainelAdminScript script, Button button, string id, string state) => StartCoroutine(SetBanAcc(script, button, id, state));
    public void ResentEmailAcc(PainelAdminScript script, Button button, string id) => StartCoroutine(ResentEmail(script, button, id));

    IEnumerator ResentEmail(PainelAdminScript script, Button button, string id)
    {
        string token_acess = AccountManager.instance.Conta.Token_acess;
        int id_user = AccountManager.instance.Conta.ID;
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"resent/{token_acess}?id_user={id_user}&id={id}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            Debug.LogError("Conta inválida.");
        }
        else
        {
            var resultado = request.downloadHandler.text;
            script.Atualizar();
        }

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator SetBanAcc(PainelAdminScript script, Button button, string id, string state)
    {
        string token_acess = AccountManager.instance.Conta.Token_acess;
        int id_user = AccountManager.instance.Conta.ID;
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id", id },
            { "state", state },
            { "id_user", id_user.ToString()},
            { "token_acess", token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json.CreateString());

        UnityWebRequest request = new(apiUrl + "ban", "PUT")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
            print("Algo deu errado na tentavida de banir/desbanir a conta.");
        }
        else
        {
            print("banimento/desbanimento feito com sucesso.");
            script.Atualizar();
        }

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator SetBlockAcc(PainelAdminScript script, Button button, string id, string state)
    {
        string token_acess = AccountManager.instance.Conta.Token_acess;
        int id_user = AccountManager.instance.Conta.ID;
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id", id },
            { "state", state },
            { "id_user", id_user.ToString()},
            { "token_acess", token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json.CreateString());

        UnityWebRequest request = new(apiUrl + "block", "PUT")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
            print("Algo deu errado na tentavida de bloquear/desbloquear a conta.");
        }
        else
        {
            print("Bloqueio/desbloqueio feito com sucesso.");
            script.Atualizar();
        }

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator GetCountAcc(PainelAdminScript script)
    {
        string token_acess = AccountManager.instance.Conta.Token_acess;
        int id_user = AccountManager.instance.Conta.ID;
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"count/{token_acess}?id_user={id_user}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
            Debug.LogError("Tabela vazia.");
        }
        else
        {
            var resultado = request.downloadHandler.text;
            script.SetCount(GetCount(resultado));
        }

        request.Dispose();
    }

    IEnumerator GetSelectAccs(PainelAdminScript script, int v1, int v2)
    {
        string token_acess = AccountManager.instance.Conta.Token_acess;
        int id_user = AccountManager.instance.Conta.ID;
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"select/{token_acess}?id_user={id_user}&v1={v1}&v2={v2}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.downloadHandler.text);
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
        }
        else script.SetContas(request.downloadHandler.text);

        request.Dispose();
    }

    IEnumerator PostAndGetInfos(LoginManager script, Text feedback, string user, string password)
    {
        feedback.text = null;
        IDictionary dicio = new Dictionary<string, string>
        {
            { "user", user },
            { "password", password }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json.CreateString());

        UnityWebRequest request = new(apiUrl + "login", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            feedback.text = request.downloadHandler.text;
            feedback.color = Color.red;
        }
        else
        {
            feedback.text = "Login feito com sucesso.";
            feedback.color = Color.green;

            JSON jsoninfo = JSON.ParseString(request.downloadHandler.text);
            DateTime.TryParse(jsoninfo.GetString("last_login"), out DateTime lastLoginDate);

            Conta conta = new(
                jsoninfo.GetInt("id"),
                jsoninfo.GetString("user"),
                jsoninfo.GetString("email"),
                jsoninfo.GetInt("isBlocked") == 1,
                jsoninfo.GetInt("isAdmin") == 1,
                jsoninfo.GetInt("isBanned") == 1,
                jsoninfo.GetInt("isConfirmed") == 1,
                DateTime.Parse(jsoninfo.GetString("created_at")),
                lastLoginDate,
                jsoninfo.GetString("token_acess")
                );
                   
            AccountManager.instance.SetInfos(conta);
            script.ChangeTela();
        }

        script.ResetInputs();

        request.Dispose();
    }

    IEnumerator PostAccount(CreateAccManager script, Text feedback, string nome, string email, string password)
    {
        feedback.text = null;
        IDictionary dicio = new Dictionary<string, string>
        {
            { "user", nome },
            { "email", email },
            { "password", password }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json.CreateString());

        UnityWebRequest request = new(apiUrl + "register", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            feedback.text = request.downloadHandler.text;
            feedback.color = Color.red;
        }
        else
        {
            feedback.text = "Conta criada com sucesso.";
            feedback.color = Color.green;
            script.ChangeTela();
        }

        script.ResetInputs();

        request.Dispose();
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