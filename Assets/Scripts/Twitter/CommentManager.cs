using Leguar.TotalJSON;
using System;
using UnityEngine;
using UnityEngine.UI;

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

    public void AbrirPostagem(Post post)
    {
        postagem.SetInfos(post);
        conn.Comments(this, post.ID);

        main.SetActive(false);
        commentTela.SetActive(true);
    }

    public void CriarComentarios(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Tools.Api2Json(result));

        for (int i = 0; i < json.Length; i++)
        {
            int id_post = json[i].GetInt("id_post");
            string user = json[i].GetString("user"),
                   content = json[i].GetString("content");
            DateTime data_pub = DateTime.Parse(json[i].GetString("data_pub"));

            Comment comment = new(id_post, user, content, data_pub);

            twt.comments.AddLast(comment);
        }

        SetCommentsTela();
    }

    private void SetCommentsTela()
    {
        int totalPosts = twt.comments.Count;
        float tamanhoContent = totalPosts * 200f;
        float posY = tamanhoContent / 2 - 100f;

        content.sizeDelta = new(0, tamanhoContent);

        Comment[] sArray = new Comment[twt.comments.Count];
        twt.comments.CopyTo(sArray, 0);

        for (int i = 0; i < twt.comments.Count; i++)
        {
            Comment comment = sArray[i];

            var temp = Instantiate(prefab_Comment, canvas);
            temp.transform.SetParent(content);
            temp.GetComponent<RectTransform>().anchoredPosition = new(0, posY);

            var classe = temp.GetComponent<Comentario>();
            classe.SetInfos(comment);

            comment.SetClasse(classe);

            if (i % 2 == 0) temp.GetComponent<Image>().color = Color.gray;

            posY -= 200f;
        }
    }
}