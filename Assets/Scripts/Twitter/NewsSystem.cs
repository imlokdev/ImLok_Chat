using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class NewsSystem : MonoBehaviour
{
    NewsConnection conn;

    TwitterSystem twitterSystem;

    [SerializeField] RectTransform objeto, scrollView;
    [SerializeField] Text newPostsText;

    readonly LinkedList<Post> newPosts = new();

    public float timeUpdate = 15f;
    private float timeCD;
    private bool actived;

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
            Post post = Tools.ApiToPost(json[i]);
            newPosts.AddLast(post);
        }

        if (json.Length > 0)
        {
            newPostsText.text = $"Mostrar {json.Length} posts";
            MoveScrollView(true);
        }
    }

    public void NewButton()
    {
        twitterSystem.CancelarUpdate();
        MoveScrollView(false);

        Post[] sArray = new Post[newPosts.Count];
        newPosts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            twitterSystem.CriarPost(item);

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