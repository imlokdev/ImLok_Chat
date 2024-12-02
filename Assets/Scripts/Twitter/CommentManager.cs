using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class CommentManager : MonoBehaviour
{
    TwitterConnection conn;

    Post postOpenned;

    [SerializeField] TwitterSystem twt;
    [SerializeField] Postagem postagem;
    public PopUpManager popUpManager;

    [SerializeField] GameObject main, commentTela, prefab_Comment;
    [SerializeField] Transform canvas;

    [SerializeField] InputField commentInput;
    [SerializeField] Button commentBtn;
    [SerializeField] RectTransform content;

    private void Start() => conn = TwitterConnection.instance;

    public void OpenPostagem(Post post)
    {
        postOpenned = post;

        content.sizeDelta = Vector2.zero;
        twt.ClearCommentsObjects();

        postagem.SetInfos(post);
        twt.ShowExistingComments(this, post.ID);
        conn.Comments(this, post.ID);

        main.SetActive(false);
        commentTela.SetActive(true);
    }

    public void ComentarButton()
    {
        if (String.IsNullOrEmpty(commentInput.text)) return;

        conn.NewComment(this, postOpenned.ID, commentInput.text, commentBtn);
        commentBtn.interactable = false;
        commentInput.text = null;
    }

    public void CreateComment(string result)
    {
        JSON json = JSON.ParseString(result);

        int id = json.GetInt("id"),
            id_post = json.GetInt("id_post");
        string user = json.GetString("user"),
               content = json.GetString("content");
        DateTime data_pub = DateTime.Parse(json.GetString("data_pub")),
                 horario = DateTime.Parse(json.GetString("horario"));

        Comment comment = new(id, id_post, user, content, data_pub, horario);

        if (!twt.ContainsInPost(id_post, comment))
        {
            twt.AddComment(this, id_post, comment, true);
            var comments = twt.GetComments(id_post);
            Comment[] sArray = new Comment[comments.Count];
            comments.CopyTo(sArray, 0);
            SetCommentInScreen(sArray, comment);
        }

        commentBtn.interactable = false;
        postagem.Atualizar();
    }

    public void CreateComments(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Tools.Api2Json(result));

        for (int i = 0; i < json.Length; i++)
        {
            int id = json[i].GetInt("id"),
                id_post = json[i].GetInt("id_post");
            string user = json[i].GetString("user"),
                   content = json[i].GetString("content");
            DateTime data_pub = DateTime.Parse(json[i].GetString("data_pub")),
                     horario = DateTime.Parse(json[i].GetString("horario")); ;

            Comment comment = new(id, id_post, user, content, data_pub, horario);

            if (!twt.ContainsInPost(id_post, comment))
            {
                twt.AddComment(this, id_post, comment);
                SetCommentsInScreen(twt.GetComments(id_post));
            }
        }
    }

    public void SetCommentInScreen(Comment[] comments, Comment comment)
    {
        int totalPosts = comments.Length;
        float tamanhoContent = totalPosts * 200f;
        float posY = tamanhoContent / 2 - 100f;

        content.sizeDelta = new(0, tamanhoContent);

        for (int i = 0; i < totalPosts; i++)
        {
            if (comments[i].Equals(comment))
            {
                var temp = Instantiate(prefab_Comment, canvas);
                temp.transform.SetParent(content);
                temp.GetComponent<RectTransform>().anchoredPosition = new(0, posY);

                var classe = temp.GetComponent<Comentario>();
                classe.SetInfos(comment);

                comment.SetClasse(classe);

                if (totalPosts % 2 == 0) temp.GetComponent<Image>().color = Color.gray;
            }
            else
            {
                RectTransform rect = comments[i].Comentario.GetComponent<RectTransform>();
                rect.anchoredPosition = new(0, posY);
            }

            posY -= 200f;
        }
    }

    public void SetCommentsInScreen(LinkedList<Comment> comments)
    {
        int totalPosts = comments.Count;
        float tamanhoContent = totalPosts * 200f;
        float posY = tamanhoContent / 2 - 100f;

        content.sizeDelta = new(0, tamanhoContent);

        Comment[] sArray = new Comment[comments.Count];
        comments.CopyTo(sArray, 0);

        for (int i = 0; i < comments.Count; i++)
        {
            if (i != totalPosts-1)
            {
                RectTransform rect = sArray[i].Comentario.GetComponent<RectTransform>();
                rect.anchoredPosition = new(0, posY);
                rect.gameObject.SetActive(true);
            }
            else
            {
                Comment comment = sArray[i];
                var temp = Instantiate(prefab_Comment, canvas);
                temp.transform.SetParent(content);
                temp.GetComponent<RectTransform>().anchoredPosition = new(0, posY);

                var classe = temp.GetComponent<Comentario>();
                classe.SetInfos(comment);

                comment.SetClasse(classe);

                if (i % 2 == 0) temp.GetComponent<Image>().color = Color.gray;
            }

            posY -= 200f;
        }
    }
}