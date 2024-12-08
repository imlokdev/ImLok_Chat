using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;
using System;

public class NewsSystem : MonoBehaviour
{
    NewsConnection conn;

    TwitterSystem twitterSystem;

    [SerializeField] RectTransform objeto, scrollView;
    [SerializeField] Text newPostsText;

    readonly LinkedList<Post> newPosts = new();

    public float timeUpdate = 15f;
    private float timeCD;
    private bool actived = false;

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
        int postValidos = 0;

        for (int i = 0; i < json.Length; i++)
        {
            Post post = Tools.ApiToPost(json[i]);
            if (twitterSystem.Contains(post)) continue;
            newPosts.AddLast(post);
            postValidos++;
        }

        if (postValidos > 0)
        {
            newPostsText.text = $"Mostrar {postValidos} posts";
            MoveScrollView(true);
        }
    }

    public void NewButton()
    {
        MoveScrollView(false);

        Post[] sArray = new Post[newPosts.Count];
        newPosts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            twitterSystem.CriarPost(item);

        newPosts.Clear();
    }

    public void CleanNewPosts()
    {
        MoveScrollView(false);
        newPosts.Clear();
    }

    private void MoveScrollView(bool active)
    {
        if (!actived && active)
        {
            actived = active;

            {
                var temp = scrollView.anchoredPosition;
                temp.y -= objeto.sizeDelta.y;

                scrollView.anchoredPosition = temp;
            }

            {
                var temp = scrollView.sizeDelta;
                temp.y -= objeto.sizeDelta.y;

                scrollView.sizeDelta = temp;
            }

            objeto.gameObject.SetActive(true);
        }
        else if (actived && !active)
        {
            objeto.gameObject.SetActive(false);

            actived = active;

            {
                var temp = scrollView.anchoredPosition;
                temp.y += objeto.sizeDelta.y;

                scrollView.anchoredPosition = temp;
            }

            {
                var temp = scrollView.sizeDelta;
                temp.y += objeto.sizeDelta.y;

                scrollView.sizeDelta = temp;
            }
        }
    }
}