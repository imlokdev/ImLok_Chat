using Leguar.TotalJSON;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NewsConnection : MonoBehaviour
{
    public static NewsConnection instance;

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

    public void NewPosts(NewsSystem script, int id_newest_post) => StartCoroutine(GetNewPosts(script, id_newest_post));

    IEnumerator GetNewPosts(NewsSystem script, int id_newest_post)
    {
        IDictionary dicio = new Dictionary<string, string>
        {
            { "id_user", AccountManager.instance.Conta.ID.ToString() },
            { "id_newest_post", id_newest_post.ToString() },
            { "token_acess", AccountManager.instance.Conta.Token_acess }
        };
        JSON json = new(dicio);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json.CreateString());
        UnityWebRequest request;

        request = new(apiUrl + "newposts", "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            script.SetNewPosts(request.downloadHandler.text);
        else
            sessionManager.FinalizarSessao(request.downloadHandler.text, request.responseCode);

        request.Dispose();
    }
}