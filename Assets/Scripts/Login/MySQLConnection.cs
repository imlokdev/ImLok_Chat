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

        request.Dispose();
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

        request.Dispose();
    }

    IEnumerator GetSelectAccs(PainelAdminScript script, int v1, int v2)
    {
        print($"V1: {v1}, V2: {v2}");
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"getselectacc?v1={v1}&v2={v2}");
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
            feedback.text = request.downloadHandler.text;
            feedback.color = Color.red;
        }
        else
        {
            feedback.text = "Login feito com sucesso.";
            feedback.color = Color.green;

            string[] infos = GetInfos(request.downloadHandler.text);
            for (int i = 0; i < infos.Length; i++) infos[i] = infos[i].Trim();

            int count = 0;
            int id = int.Parse(infos[count++]);
            string _user = infos[++count],
                   email = infos[++count];
            bool isBlocked = infos[++count] == "1",
                 isAdmin = infos[++count] == "1",
                 isBanned = infos[++count] == "1";
            DateTime created_at = DateTime.Parse(infos[++count+1]);
            bool lastLoginSuccess = DateTime.TryParse(infos[^1], out DateTime lastLoginDate);
            AccountManager.instance.SetInfos(
                id, _user, email, isBlocked, isAdmin, isBanned, created_at, 
                lastLoginSuccess ? lastLoginDate : DateTime.MinValue);
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