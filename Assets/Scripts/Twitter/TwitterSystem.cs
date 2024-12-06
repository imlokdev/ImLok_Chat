using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class TwitterSystem : MonoBehaviour
{
    TwitterConnection conn;

    [SerializeField] GameObject infosConta, prefab_Post, main, comment, popUp;
    [SerializeField] RectTransform content, canvas;
    [SerializeField] InputField postInput;
    [SerializeField] Button postBtn;

    readonly LinkedList<Post> posts = new();
    
    public float timeUpdateDatetime = 60f;
    private bool blockEndScroll;
    private float timeCD;
    private int newPosts;

    private void Start()
    {
        conn = TwitterConnection.instance;
        conn.Posts(this);
        timeCD = Time.time;
    }

    private void Update()
    {
        if (Time.time - timeCD > timeUpdateDatetime)
        {
            conn.AtualizarPosts(this, posts.Count);
            timeCD = Time.time;
        }
    }

    public int GetCountPosts() => posts.Count;
    public Post GetFirstPost() => posts.First.Value;

    public void ContaButton()
    {
        if (popUp.activeSelf) return;
        gameObject.SetActive(false);
        infosConta.SetActive(true);
    }

    public void CommentToMain()
    {
        comment.SetActive(false);
        main.SetActive(true);
    }

    public void PostarButton()
    {
        if (popUp.activeSelf) return;
        if (String.IsNullOrEmpty(postInput.text)) return;

        postBtn.interactable = false;
        conn.NewPost(this, postInput.text, postBtn);
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
            DateTime data_pub = DateTime.Parse(json[i].GetString("data_pub")),
                     horario = DateTime.Parse(json[i].GetString("horario")); ;
            bool user_liked = json[i].GetInt("user_liked") == 1;

            Post post = new(id, user, content, data_pub, horario, total_likes, total_comments, user_liked);
            
            posts.AddLast(post);
        }

        SetPostsTela();
    }

    private void SetPostsTela()
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        for (int i = 0; i < posts.Count; i++)
        {
            Post post = sArray[i];

            var temp = Instantiate(prefab_Post, canvas);
            temp.transform.SetParent(content);

            var classe = temp.GetComponent<Postagem>();

            classe.SetInfos(post);
            classe.SetScript(this);
            post.SetClasse(classe);

            if (i % 2 == 0) temp.GetComponent<Image>().color = Color.gray;
        }

        OrganizarPostsTela();
    }

    public void CriarPost(string result)
    {
        JSON json = JSON.ParseString(Tools.Api2Json(result));

        int id = json.GetInt("id"),
                total_likes = json.GetInt("total_likes"),
                total_comments = json.GetInt("total_comments");
        string user = json.GetString("user"),
               content = json.GetString("content");
        DateTime data_pub = DateTime.Parse(json.GetString("data_pub")),
                 horario = DateTime.Parse(json.GetString("horario"));
        bool user_liked = json.GetInt("user_liked") == 1;

        Post post = new(id, user, content, data_pub, horario, total_likes, total_comments, user_liked);

        posts.AddFirst(post);
        SetNewPostTela(post);
    }

    private void SetNewPostTela(Post post)
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        for (int i = 0; i < posts.Count; i++)
        {
            if (sArray[i] == sArray[0])
            {
                var temp = Instantiate(prefab_Post, canvas);
                temp.transform.SetParent(content);

                var classe = temp.GetComponent<Postagem>();
                classe.SetInfos(post);

                sArray[i].SetClasse(classe);

                if (newPosts > 0 && newPosts % 2 != 0) temp.GetComponent<Image>().color = Color.gray;
            }
        }

        newPosts++;
        OrganizarPostsTela();
    }

    private void OrganizarPostsTela()
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
            post.Postagem.GetComponent<RectTransform>().anchoredPosition = new(0, posY);
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
            bool user_liked = json[i].GetInt("user_liked") == 1;
            DateTime horario = DateTime.Parse(json[i].GetString("horario"));

            for (int j = 0; j < sArray.Length; j++)
                if (sArray[j].ID == id)
                {
                    sArray[j].UpdateInfos(total_likes, total_comments, user_liked, horario);
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

    public void DeletePost(Post post)
    {
        RecolorirPosts(posts.Find(post));
        posts.Remove(post);
        Destroy(post.Postagem.gameObject);
        OrganizarPostsTela();

        comment.SetActive(false);
        main.SetActive(true);
    }

    public void DeleteComment(Comment comment)
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            if (item.ID == comment.ID_post)
            {
                RecolorirComments(item.Comments, item.Comments.Find(comment));
                item.Comments.Remove(comment);
                //OrganizarCommentsTela(item.Comments);
            }

        Destroy(comment.Comentario.gameObject);
    }

    public void ShowExistingComments(CommentManager script, int id_post)
    {
        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        foreach (var item in sArray)
            if (item.ID == id_post)
                script.SetCommentsInScreen(item.Comments);
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
                    item2.Comentario?.gameObject.SetActive(false);
            }
    }

    public bool ContainsInPost(int id_post, Comment comment)
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

    private void RecolorirPosts(LinkedListNode<Post> node, bool inside = false)
    {
        if (node != posts.Last) RecolorirPosts(node.Next, true);

        if (inside) node.Value.Postagem.GetComponent<Image>().color = node.Previous.Value.Postagem.GetComponent<Image>().color;
    }

    private void RecolorirComments(LinkedList<Comment> lista, LinkedListNode<Comment> node, bool inside = false)
    {
        if (node != lista.Last) RecolorirComments(lista, node.Next, true);

        if (inside) node.Value.Comentario.GetComponent<Image>().color = node.Previous.Value.Comentario.GetComponent<Image>().color;
    }
}