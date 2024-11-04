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
    => StartCoroutine(PostAccount(nome, email, password, feedback, script));

    public void LoginAccount(string login, string password, Text feedback, LoginManager script)
    => StartCoroutine(PostAndGetInfos(login, password, feedback, script));

    public void CountAcc(PainelAdminScript script) => StartCoroutine(GetCountAcc(script));
    
    public void SelectAccs(PainelAdminScript script, int v1, int v2) => StartCoroutine(GetSelectAccs(script, v1, v2));

    public void BlockAcc(PainelAdminScript script, Button button, string id, string state) => StartCoroutine(SetBlockAcc(script, button, id, state));

    public void BanAcc(PainelAdminScript script, Button button, string id, string state) => StartCoroutine(SetBanAcc(script, button, id, state));

    IEnumerator SetBanAcc(PainelAdminScript script, Button button, string id, string state)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id", id },
            { "state", state }
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
            print("Algo deu errado na tentavida de banir/desbanir a conta.");
        }
        else
        {
            Debug.Log("Resposta do servidor: " + request.downloadHandler.text);
            print("banimento/desbanimento feito com sucesso.");

            script.Atualizar();
        }

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator SetBlockAcc(PainelAdminScript script, Button button, string id, string state)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id", id },
            { "state", state }
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
            print("Algo deu errado na tentavida de bloquear/desbloquear a conta.");
        }
        else
        {
            Debug.Log("Resposta do servidor: " + request.downloadHandler.text);
            print("Bloqueio/desbloqueio feito com sucesso.");

            script.Atualizar();
        }

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator GetCountAcc(PainelAdminScript script)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"count");
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

        request.Dispose();
    }

    IEnumerator GetSelectAccs(PainelAdminScript script, int v1, int v2)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"select?v1={v1}&v2={v2}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("Erro: " + request.downloadHandler.text);
        else script.SetContas(request.downloadHandler.text);

        request.Dispose();
    }

    IEnumerator PostAndGetInfos(string user, string password, Text feedback, LoginManager script)
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
                lastLoginDate
                );
                   
            AccountManager.instance.SetInfos(conta);
            script.ChangeTela();
        }

        request.Dispose();
    }

    IEnumerator PostAccount(string nome, string email, string password, Text feedback, CreateAccManager script)
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
            Debug.Log("Resposta do servidor: " + request.downloadHandler.text);
            feedback.text = "Conta criada com sucesso.";
            feedback.color = Color.green;
            script.ChangeTela();
        }

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