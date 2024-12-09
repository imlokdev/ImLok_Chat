using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;
using System.Xml.Linq;

public class CommentManager : MonoBehaviour
{
    TwitterConnection conn;

    public PopUpManager popUpManager;

    Post postOpenned;

    [SerializeField] TwitterSystem twt;
    [SerializeField] Postagem postagem;

    [SerializeField] GameObject main, commentTela, prefab_Comment, delete;
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

        Conta conta = AccountManager.instance.Conta;
        if (postOpenned.User == conta.User || conta.IsAdmin) delete.SetActive(true);
        else delete.SetActive(false);
    }

    public void ComentarButton()
    {
        if (String.IsNullOrEmpty(commentInput.text)) return;

        conn.NewComment(this, postOpenned.ID, commentInput.text, commentBtn);
        commentBtn.interactable = false;
        commentInput.text = null;
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
                SetCommentInScreen(comment, i, json.Length);
            }
        }

        postagem.Atualizar();
    }

    public void SetCommentInScreen(Comment comment, int index, int resultLenght = 0)
    {
        var totalComments = postOpenned.Comments.Count;
        var temp = Instantiate(prefab_Comment, canvas);
        temp.transform.SetParent(content);

        var classe = temp.GetComponent<Comentario>();
        classe.SetInfos(comment);

        comment.SetClasse(classe);

        if (resultLenght > 0) index = totalComments;
        if (index % 2 == 0) temp.GetComponent<Image>().color = Color.gray;
        OrganizarCommentsTela();
    }

    public void OrganizarCommentsTela()
    {
        var comments = postOpenned.Comments;
        int totalPosts = comments.Count;
        float tamanhoContent = totalPosts * 200f;
        float posY = tamanhoContent / 2 - 100f;

        content.sizeDelta = new(0, tamanhoContent);

        Comment[] sArray = new Comment[comments.Count];
        comments.CopyTo(sArray, 0);

        for (int i = 0; i < comments.Count; i++)
        {
            Comment post = sArray[i];
            post.Comentario.GetComponent<RectTransform>().anchoredPosition = new(0, posY);
            posY -= 200f;
        }

        commentBtn.interactable = true;
        postagem.Atualizar();
    }
}