using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class TwitterSystem : MonoBehaviour
{
    TwitterConnection conn;

    NewsSystem newsSystem;
    CommentManager commentManager;

    [SerializeField] GameObject infosConta, prefab_Post, main, comment, popUp;
    [SerializeField] RectTransform content, canvas;
    [SerializeField] InputField postInput;
    [SerializeField] Button postBtn;

    readonly LinkedList<Post> posts = new();
    
    public float timeUpdateDatetime = 20f;
    private bool blockEndScroll;
    private float timeCD;
    private int newPosts;

    private void Start()
    {
        conn = TwitterConnection.instance;
        newsSystem = GetComponent<NewsSystem>();
        commentManager = FindFirstObjectByType<CommentManager>();

        postBtn.interactable = false;
        conn.Posts(this, postBtn);
        timeCD = Time.time;
    }

    private void Update()
    {
        if (posts.Count > 0 && Time.time - timeCD > timeUpdateDatetime)
        {
            conn.AtualizarPosts(this);
            timeCD = Time.time;
        }
        else timeCD = Time.time;
    }

    public int GetCountPosts() => posts.Count;
    public Post GetFirstPost()
    {
        try { return posts.First.Value; }
        catch { return new(0); }
    }
    public Post GetLastPost() => posts.Last.Value;
    public bool Contains(Post post) => posts.Contains(post);

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
            Post post = Tools.ApiToPost(json[i]);
            posts.AddLast(post);
        }

        SetPostsTela();
        postBtn.interactable = true;
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
        JSON[] json = JSON.ParseStringToMultiple(Tools.Api2Json(result));

        foreach (var item in json)
        {
            Post post = Tools.ApiToPost(item);

            posts.AddFirst(post);
            SetNewPostTela(post);
        }

        newsSystem.CleanNewPosts();
    }

    public void CriarPost(Post post)
    {
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

        if (json.Length != posts.Count) return;

        Post[] sArray = new Post[posts.Count];
        posts.CopyTo(sArray, 0);

        List<int> list = new();

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
                    list.Add(id);
                    break;
                }
        }

        for (int j = 0; j < sArray.Length; j++)
            if (!list.Contains(sArray[j].ID))
                sArray[j].Postagem.AutoDeletePost();
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
                commentManager.OrganizarCommentsTela();
            }

        Destroy(comment.Comentario.gameObject);
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
                int count = 0;

                foreach (var item2 in sArray2)
                {
                    script.SetCommentInScreen(item2, count);
                    count++;
                }
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