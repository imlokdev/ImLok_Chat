using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Leguar.TotalJSON;
using System.Collections.Generic;
using System.Net;
using System.Text;

public class TwitterConnection : MonoBehaviour
{
    public static TwitterConnection instance;
    string apiUrl;

    readonly string localURL = "http://127.0.0.1:5000/",
        vercelURL = "https://api-my-sql.vercel.app/";

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

    public void Posts(TwitterSystem script, int id_user) => StartCoroutine(GetPosts(script, id_user));

    IEnumerator GetPosts(TwitterSystem script, int id_user)
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + $"postsall/{id_user}");
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
            script.CriarPosts(resultado);
        }

        request.Dispose();
    }
}