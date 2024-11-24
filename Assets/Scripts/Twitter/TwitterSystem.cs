using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class TwitterSystem : MonoBehaviour
{
    TwitterConnection conn;

    [SerializeField] GameObject infosConta, prefab_Post, main, comment;
    [SerializeField] RectTransform content, canvas;
    [SerializeField] InputField postInput;
    [SerializeField] Button postBtn;

    readonly LinkedList<Post> posts = new();
    
    public float timeUpdateDatetime = 60f;
    private bool blockEndScroll;
    private float timeCD;

    private void Start()
    {
        conn = TwitterConnection.instance;
        conn.Posts(this, 1);
        timeCD = Time.time;
    }

    private void Update()
    {
        if (Time.time - timeCD > timeUpdateDatetime)
        {
            conn.AtualizarPosts(this, 1, posts.Count);
            timeCD = Time.time;
        }
    }

    public void ContaButton()
    {
        gameObject.SetActive(false);
        infosConta.SetActive(true);
    }

    public void PostarButton()
    {
        if (String.IsNullOrEmpty(postInput.text)) return;

        postBtn.interactable = false;
        conn.NewPost(this, 1, postInput.text, postBtn);
        postInput.text = null;
    }

    public void CriarPosts(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Tools.Api2Json(result));

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
            
            posts.AddLast(post);
        }

        SetPostsTela();
    }

    private void SetPostsTela()
    {
        int totalPosts = posts.Count;
        float tamanhoContent = totalPosts * 200f;
        float posY = tamanhoContent / 2 - 100f;

        content.sizeDelta = new(0, tamanhoContent);

        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        for (int i = 0; i < posts.Count; i++)
        {
            Post post = sArray[i];

            var temp = Instantiate(prefab_Post, canvas);
            temp.transform.SetParent(content);
            temp.GetComponent<RectTransform>().anchoredPosition = new(0, posY);

            var classe = temp.GetComponent<Postagem>();
            classe.SetInfos(post);

            post.SetClasse(classe);

            if (i % 2 == 0) temp.GetComponent<Image>().color = Color.gray;

            posY -= 200f;
        }
    }

    public void CriarPost(string result)
    {
        JSON json = JSON.ParseString(Tools.Api2Json(result));

        int id = json.GetInt("id"),
                total_likes = json.GetInt("total_likes"),
                total_comments = json.GetInt("total_comments");
        string user = json.GetString("user"),
               content = json.GetString("content");
        DateTime data_pub = DateTime.Parse(json.GetString("data_pub"));
        bool user_liked = json.GetInt("user_liked") == 1;

        Post post = new(id, user, content, data_pub, total_likes, total_comments, user_liked);

        posts.AddFirst(post);
        SetNewPostTela(post);
    }

    private void SetNewPostTela(Post post)
    {
        int totalPosts = posts.Count;
        float tamanhoContent = totalPosts * 200f;
        float posY = tamanhoContent / 2 - 100f;

        content.sizeDelta = new(0, tamanhoContent);

        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        for (int i = 0; i < posts.Count; i++)
        {
            if (sArray[i] == sArray[0])
            {
                var temp = Instantiate(prefab_Post, canvas);
                temp.transform.SetParent(content);
                temp.GetComponent<RectTransform>().anchoredPosition = new(0, posY);

                var classe = temp.GetComponent<Postagem>();
                classe.SetInfos(post);

                sArray[i].SetClasse(classe);

                print(posts.Count);
                if ((posts.Count-1) % 2 == 0) temp.GetComponent<Image>().color = Color.gray;
            }
            else sArray[i].Postagem.gameObject.GetComponent<RectTransform>().anchoredPosition = new(0, posY);

            posY -= 200f;
        }
    }

    public void UpdateAllPosts(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Tools.Api2Json(result));

        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        for (int i = 0; i < json.Length; i++)
        {
            int id = json[i].GetInt("id"),
                total_likes = json[i].GetInt("total_likes"),
                total_comments = json[i].GetInt("total_comments");

            for (int j = 0; j < sArray.Length; j++)
                if (sArray[j].ID == id)
                {
                    sArray[j].UpdateInfos(total_likes, total_comments);
                    break;
                }
        }
    }

    public void EndScroll(Vector2 position)
    {
        if (position.y <= 0.01f && !blockEndScroll) // 0.01 para tolerância
        {
            Debug.Log("Chegou ao final da Scroll View!");
            blockEndScroll = true;
        }
    }

    public void CommentToMain()
    {
        comment.SetActive(false);
        main.SetActive(true);
    }

    public void AddComment(CommentManager script, int id_post, Comment comment, bool @new = false)
    {
        if (ContainsInPost(id_post, comment)) return;

        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            if (item.ID == id_post)
            {
                if (@new) item.Comments.AddFirst(comment);
                else item.Comments.AddLast(comment);
                break;
            }
    }

    public void ShowExistingComments(CommentManager script, int id_post)
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            if (item.ID == id_post)
            {
                Comment[] sArray2 = new Comment[item.Comments.Count];
                item.Comments.CopyTo(sArray2, 0);

                foreach (var item2 in sArray2)
                    script.SetCommentInScreen(sArray2, item2);
            }
    }

    public LinkedList<Comment> GetComments(int id_post)
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            if (item.ID == id_post) return item.Comments;
        return null;
    }

    public void ClearCommentsObjects()
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            if (item.Total_comments > 0)
            {
                Comment[] sArray2 = new Comment[item.Comments.Count];
                item.Comments.CopyTo(sArray2, 0);

                foreach (var item2 in sArray2)
                    if (item2.Comentario != null)
                    {
                        print($"Destruindo o objeto do comentário: {item2.Content}");
                        Destroy(item2.Comentario.gameObject);
                    }
            }
    }

    private bool ContainsInPost(int id_post, Comment comment)
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            if (item.ID == id_post)
            {
                Comment[] sArray2 = new Comment[item.Comments.Count];
                item.Comments.CopyTo(sArray2, 0);

                foreach (var item2 in sArray2)
                    if (item2.Equals(comment)) return true;
            }
        return false;
    }
}