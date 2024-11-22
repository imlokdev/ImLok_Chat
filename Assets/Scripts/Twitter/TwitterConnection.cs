using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

using Leguar.TotalJSON;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

public class TwitterConnection : MonoBehaviour
{
    public static TwitterConnection instance;
    string apiUrl;

    readonly string localURL = "http://127.0.0.1:5000/",
        vercelURL = "https://twitter-system.vercel.app/";

    [SerializeField] bool localAPI;

    private void Awake()
    {
        if (instance == null) instance = this;
        if (localAPI) apiUrl = localURL;
        else apiUrl = vercelURL;
    }

    private void Update()
    {
        if (localAPI) apiUrl = localURL;
        else apiUrl = vercelURL;
    }

    public void NewPost(TwitterSystem script, int id_user, string content, Button button) => StartCoroutine(CreatePost(script, id_user, content, button));
    public void Posts(TwitterSystem script, int id_user) => StartCoroutine(GetPosts(script, id_user));
    public void AtualizarPosts(TwitterSystem script, int id_user, int count) => StartCoroutine(UpdateAllPosts(script, id_user, count));

    public void Like(Postagem script, int id_user, int id_post, bool active) => StartCoroutine(SetLike(script, id_user, id_post, active));
    public void AtualizarPost(Postagem script, int id_user, int id_post) => StartCoroutine(UpdatePost(script, id_user, id_post));
    
    IEnumerator CreatePost(TwitterSystem script, int id_user, string content, Button button)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id_user", id_user.ToString() },
            { "content", content },
            { "token_acess", AccountManager.instance.Conta.Token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json.CreateString());
        UnityWebRequest request;

        request = new(apiUrl + "postar", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.downloadHandler.error);
        }
        else script.CriarPost(request.downloadHandler.text);

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator GetPosts(TwitterSystem script, int id_user)
    {
        string token_acess = AccountManager.instance.Conta.Token_acess;
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"postsall/{token_acess}?id_user={id_user}&count={50}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            Debug.LogError("Nenhum post encontrado.");
        }
        else
        {
            var resultado = request.downloadHandler.text;
            script.CriarPosts(resultado);
        }

        request.Dispose();
    }

    IEnumerator SetLike(Postagem script, int id_user, int id_post, bool active)
    {
        IDictionary dicio = new Dictionary<string, int>
        {
            { "id_user", id_user },
            { "id_post", id_post }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json.CreateString());
        UnityWebRequest request;

        if (active)
        {
            request = new(apiUrl + "like", "POST")
            {
                uploadHandler = new UploadHandlerRaw(jsonToSend),
                downloadHandler = new DownloadHandlerBuffer()
            };
        }
        else
        {
            request = new(apiUrl + "like", "DELETE")
            {
                uploadHandler = new UploadHandlerRaw(jsonToSend),
                downloadHandler = new DownloadHandlerBuffer()
            };
        }

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.downloadHandler.error);
        }
        else
        {
            JSON jsoninfo = JSON.ParseString(request.downloadHandler.text);
            bool state = jsoninfo.GetBool("state");

            script.MudarLike(state);
        }

        request.Dispose();
    }

    IEnumerator UpdatePost(Postagem script, int id_user, int id_post)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"postone?id_user={id_user}&id_post={id_post}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            Debug.LogError("Post não encontrado.");
        }
        else
        {
            var resultado = request.downloadHandler.text;
            Debug.Log("Resposta do servidor: " + resultado);
            script.UpdateInfos(resultado);
        }

        request.Dispose();
    }

    IEnumerator UpdateAllPosts(TwitterSystem script, int id_user, int count)
    {
        string token_acess = AccountManager.instance.Conta.Token_acess;
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"postsall/{token_acess}?id_user={id_user}&count={count}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            Debug.LogError("Nenhum post encontrado.");
        }
        else
        {
            var resultado = request.downloadHandler.text;
            Debug.Log("Resposta do servidor: " + resultado);
            script.UpdateAllPosts(resultado);
        }

        request.Dispose();
    }
}