using System.Text;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Leguar.TotalJSON;

public class TwitterConnection : MonoBehaviour
{
    public static TwitterConnection instance;

    SessionManager sessionManager;

    string apiUrl;

    readonly string localURL = "http://127.0.0.1:5000/",
        vercelURL = "https://twitter-system.vercel.app/";

    [SerializeField] bool localAPI;

    private void Awake()
    {
        if (instance == null) instance = this;
        if (localAPI) apiUrl = localURL;
        else apiUrl = vercelURL;

        sessionManager = GetComponent<SessionManager>();
    }

    private void Update()
    {
        if (localAPI) apiUrl = localURL;
        else apiUrl = vercelURL;
    }

    public void NewPost(TwitterSystem script, string content, Button button) => StartCoroutine(CreatePost(script, content, button));
    public void Posts(TwitterSystem script, int id_user) => StartCoroutine(GetPosts(script, id_user));
    public void AtualizarPosts(TwitterSystem script, int id_user, int count) => StartCoroutine(UpdateAllPosts(script, id_user, count));

    public void Like(Postagem script, int id_user, int id_post, bool active) => StartCoroutine(SetLike(script, id_user, id_post, active));
    public void AtualizarPost(Postagem script, int id_user, int id_post) => StartCoroutine(UpdatePost(script, id_user, id_post));

    public void NewComment(CommentManager script, int id_post, string content, Button button) => StartCoroutine(CreateComment(script, id_post, content, button));
    public void Comments(CommentManager script, int id_post) => StartCoroutine(GetComments(script, id_post));

    IEnumerator CreateComment(CommentManager script, int id_post, string content, Button button)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id_user", AccountManager.instance.Conta.ID.ToString() },
            { "id_post", id_post.ToString() },
            { "content", content },
            { "token_acess", AccountManager.instance.Conta.Token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json.CreateString());
        UnityWebRequest request;

        request = new(apiUrl + "comentar", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.downloadHandler.error);
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
        }
        else script.CreateComment(request.downloadHandler.text);

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator GetComments(CommentManager script, int id_post)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id_user", AccountManager.instance.Conta.ID.ToString() },
            { "id_post", id_post.ToString() },
            { "token_acess", AccountManager.instance.Conta.Token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json.CreateString());
        UnityWebRequest request;

        request = new(apiUrl + "comments", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            script.CreateComments(request.downloadHandler.text);
        else 
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);

        request.Dispose();
    }

    IEnumerator CreatePost(TwitterSystem script, string content, Button button)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id_user", AccountManager.instance.Conta.ID.ToString() },
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
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
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
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
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
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
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
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
        }
        else
        {
            var resultado = request.downloadHandler.text;
            script.UpdateAllPosts(resultado);
        }

        request.Dispose();
    }
}