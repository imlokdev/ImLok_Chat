using Leguar.TotalJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TwitterSystem : MonoBehaviour
{
    TwitterConnection conn;

    [SerializeField] GameObject infosConta, painelPostagem, prefab_Post;
    [SerializeField] RectTransform content, canvas;

    List<Post> posts = new();

    private void Start()
    {
        conn = TwitterConnection.instance;
        conn.Posts(this, 1);
    }

    public void ContaButton()
    {
        gameObject.SetActive(false);
        infosConta.SetActive(true);
    }

    public void AbrirPainelPostagem()
    {
        gameObject.SetActive(false);
        painelPostagem.SetActive(true);
    }

    public void CriarPosts(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Api2Json(result));

        for (int i = 0; i < json.Length; i++)
        {
            int id = json[i].GetInt("id"),
                total_likes = json[i].GetInt("total_likes"),
                total_comments = json[i].GetInt("total_comments");
            string user = json[i].GetString("user"),
                   content = json[i].GetString("content");
            DateTime data_pub = DateTime.Parse(json[i].GetString("data_pub"));
            bool user_liked = json[i].GetInt("user_liked") == 1;

            Post post = new(id, user, content, data_pub, total_likes, total_comments, user_liked);
            
            posts.Add(post);
        }

        SetPostsTela();
    }

    private void SetPostsTela()
    {
        int totalPosts = posts.Count;
        float tamanhoContent = totalPosts * 200f;
        float posY = tamanhoContent / 2 - 100f;

        content.sizeDelta = new(0, tamanhoContent);

        for (int i = 0; i < posts.Count; i++)
        {
            var temp = Instantiate(prefab_Post, canvas);
            temp.transform.SetParent(content);
            temp.GetComponent<RectTransform>().anchoredPosition = new(0, posY);

            posY -= 200f;
        }
    }

    private string Api2Json(string apiString)
    {
        var temp = apiString.Replace("[", "").Replace("]", "").Trim();

        while (true)
        {
            int index = -1;

            for (int i = 0; i < temp.Length; i++)
                if (i + 1 < temp.Length)
                    if (temp[i] == '}' && temp[i + 1] == ',')
                    {
                        index = i + 1;
                        break;
                    }

            if (index < 0) break;
            else temp = temp.Remove(index, 1);
        }

        return temp;
    }
}