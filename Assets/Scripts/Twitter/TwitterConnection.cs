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
    public void Posts(TwitterSystem script, Button btn) => StartCoroutine(GetPosts(script, btn));
    public void AtualizarPosts(TwitterSystem script) => StartCoroutine(UpdateAllPosts(script));
    public void DeletarPost(PopUpManager script, int id_post, Button[] buttons) => StartCoroutine(DeletePost(script, id_post, buttons));

    public void Like(Postagem script, int id_post, bool active, Button button) => StartCoroutine(SetLike(script, id_post, active, button));
    public void AtualizarPost(Postagem script, int id_post) => StartCoroutine(UpdatePost(script, id_post));

    public void NewComment(CommentManager script, int id_post, string content, Button button) => StartCoroutine(CreateComment(script, id_post, content, button));
    public void Comments(CommentManager script, int id_post) => StartCoroutine(GetComments(script, id_post));
    public void DeletarComment(PopUpManager script, int id_comment, Button[] buttons) => StartCoroutine(DeleteComment(script, id_comment, buttons));

    IEnumerator DeletePost(PopUpManager script, int id_post, Button[] buttons)
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

        request = new(apiUrl + "delpost", "DELETE")
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
        else script.DeletarPostagem();

        foreach (var item in buttons)
            item.interactable = true;

        request.Dispose();
    }

    IEnumerator DeleteComment(PopUpManager script, int id_comment, Button[] buttons)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id_user", AccountManager.instance.Conta.ID.ToString() },
            { "id_comment", id_comment.ToString() },
            { "token_acess", AccountManager.instance.Conta.Token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json.CreateString());
        UnityWebRequest request;

        request = new(apiUrl + "delcomment", "DELETE")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.downloadHandler.text);
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
        }
        else script.DeletarComentario();

        foreach (var item in buttons)
            item.interactable = true;

        request.Dispose();
    }

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
        else script.CreateComments(request.downloadHandler.text);

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
            { "id_newest_post", script.GetFirstPost().ID.ToString() },
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

    IEnumerator GetPosts(TwitterSystem script, Button btn)
    {
        int id_user = AccountManager.instance.Conta.ID;
        string token_acess = AccountManager.instance.Conta.Token_acess;

        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"postsall/{token_acess}?id_user={id_user}&count={50}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.error);
            Debug.LogError("Nenhum post encontrado.");
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
        }
        else script.CriarPosts(request.downloadHandler.text);

        btn.interactable = true;

        request.Dispose();
    }

    IEnumerator SetLike(Postagem script, int id_post, bool active, Button button)
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
            var resultado = request.downloadHandler.text;
            script.AutoDeletePost(resultado, request.responseCode);
            sessionManager.FinalizarSessao(resultado, request.responseCode);
        }
        else
        {
            JSON jsoninfo = JSON.ParseString(request.downloadHandler.text);
            bool state = jsoninfo.GetBool("state");

            script.MudarLike(state);
        }

        button.interactable = true;

        request.Dispose();
    }

    IEnumerator UpdatePost(Postagem script, int id_post)
    {
        int id_user = AccountManager.instance.Conta.ID;
        string token_acess = AccountManager.instance.Conta.Token_acess;

        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"postone/{token_acess}?id_user={id_user}&id_post={id_post}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            var resultado = request.downloadHandler.text;

            Debug.LogError("Erro: " + resultado);
            Debug.LogError($"Post ID: {id_post} não encontrado.");

            script.AutoDeletePost(resultado, request.responseCode);
            sessionManager.FinalizarSessao(resultado, request.responseCode);
        }
        else script.UpdateInfos(request.downloadHandler.text);

        request.Dispose();
    }

    IEnumerator UpdateAllPosts(TwitterSystem script)
    {
        int id_user = AccountManager.instance.Conta.ID;
        string token_acess = AccountManager.instance.Conta.Token_acess;

        IDictionary dicio = new Dictionary<string, string>
        {
            { "id_user", id_user.ToString() },
            { "id_newest_post", script.GetFirstPost().ID.ToString() },
            { "id_older_post", script.GetLastPost().ID.ToString() },
            { "token_acess", token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json.CreateString());
        UnityWebRequest request;

        request = new(apiUrl + "updateall", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Erro: " + request.downloadHandler.text);
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);
        }
        else script.UpdateAllPosts(request.downloadHandler.text);

        request.Dispose();
    }
}