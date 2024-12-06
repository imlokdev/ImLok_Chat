using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;
using System.Collections.Generic;
using System;

public class NewsSystem : MonoBehaviour
{
    NewsConnection conn;

    TwitterSystem twitterSystem;

    [SerializeField] Text newPostsText;

    readonly LinkedList<Post> newPosts = new();

    public float timeUpdate = 15f;
    private float timeCD;

    private void Start()
    {
        conn = NewsConnection.instance;
        twitterSystem = GetComponent<TwitterSystem>();
    }

    private void Update()
    {
        if (twitterSystem.GetCountPosts() > 0 && Time.time - timeCD > timeUpdate)
        {
            conn.NewPosts(this, twitterSystem.GetFirstPost().ID);
            timeCD = Time.time;
        }
        else if (twitterSystem.GetCountPosts() == 0) timeCD = Time.time;
    }

    public void SetNewPosts(string result)
    {
        newPosts.Clear();
        JSON[] json = JSON.ParseStringToMultiple(Tools.Api2Json(result));

        for (int i = 0; i < json.Length; i++)
        {
            int id = json[i].GetInt("id"),
                total_likes = json[i].GetInt("total_likes"),
                total_comments = json[i].GetInt("total_comments");
            string user = json[i].GetString("user"),
                   content = json[i].GetString("content");
            DateTime data_pub = DateTime.Parse(json[i].GetString("data_pub")),
                     horario = DateTime.Parse(json[i].GetString("horario")); ;
            bool user_liked = json[i].GetInt("user_liked") == 1;

            Post post = new(id, user, content, data_pub, horario, total_likes, total_comments, user_liked);

            newPosts.AddLast(post);
        }

        ShowNewPosts(json.Length);
    }

    private void ShowNewPosts(int count) => newPostsText.text = $"Mostrar {count} posts";
}