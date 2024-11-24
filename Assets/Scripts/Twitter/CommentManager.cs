using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class CommentManager : MonoBehaviour
{
    TwitterConnection conn;

    [SerializeField] TwitterSystem twt;
    [SerializeField] Postagem postagem;

    [SerializeField] GameObject main, commentTela, prefab_Comment;
    [SerializeField] Transform canvas;

    [SerializeField] InputField commentInput;
    [SerializeField] Button commentBtn;
    [SerializeField] RectTransform content;

    private void Start() => conn = TwitterConnection.instance;

    public void OpenPostagem(Post post)
    {
        content.sizeDelta = Vector2.zero;
        twt.ClearCommentsObjects();

        postagem.SetInfos(post);
        twt.ShowExistingComments(this, post.ID);
        conn.Comments(this, post.ID);

        main.SetActive(false);
        commentTela.SetActive(true);
    }

    public void CreateComments(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Tools.Api2Json(result));

        for (int i = 0; i < json.Length; i++)
        {
            int id_post = json[i].GetInt("id_post");
            string user = json[i].GetString("user"),
                   content = json[i].GetString("content");
            DateTime data_pub = DateTime.Parse(json[i].GetString("data_pub"));

            Comment comment = new(id_post, user, content, data_pub);

            twt.AddComment(this, id_post, comment);
            SetCommentsInScreen(twt.GetComments(id_post));
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

                if (i % 2 == 0) temp.GetComponent<Image>().color = Color.gray;
            }

            posY -= 200f;
        }
    }

    private void SetCommentsInScreen(LinkedList<Comment> comments)
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