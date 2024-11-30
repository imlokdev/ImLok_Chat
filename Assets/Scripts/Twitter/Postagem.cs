using System;
using System.Globalization;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class Postagem : MonoBehaviour
{
    TwitterConnection conn;

    Post post;
    CommentManager commManager;
    PopUpManager popUpManager;

    [SerializeField] GameObject delete;
    [SerializeField] Image likeImg;
    [SerializeField] Text user, content, likes, comments, timer;
    [SerializeField] Button likeBtn;
    [SerializeField] Sprite likeVazio, likePreenchido;

    public float timeUpdateDatetime = 10f;
    private float timeCD;

    private void Start()
    {
        conn = TwitterConnection.instance;
        timeCD = Time.time;
        commManager = transform.parent.GetComponent<CommentManager>();
        if (!CompareTag("Desabilitar"))
        {
            popUpManager = commManager.popUpManager;
            GetComponent<Button>().onClick.AddListener(() => commManager.OpenPostagem(post));

            Conta conta = AccountManager.instance.Conta;
            if (post.User == conta.User || conta.IsAdmin) delete.SetActive(true);
        }
    }

    private void Update()
    {
        if (Time.time -  timeCD > timeUpdateDatetime)
        {
            timer.text = Tools.DateTimeToTimer(post.Data_pub);
            timeCD = Time.time;
        }
    }

    public void SetInfos(Post _post)
    {
        post = _post;

        user.text = post.User;
        content.text = post.Content;
        likes.text = post.Total_likes.ToString();
        comments.text = post.Total_comments.ToString();
        timer.text = Tools.DateTimeToTimer(post.Data_pub);

        if (post.User_liked) likeImg.sprite = likePreenchido;
        else likeImg.sprite = likeVazio;
    }

    public void UpdateInfos(string infos)
    {
        JSON json = JSON.ParseString(Tools.Api2Json(infos));

        int id = json.GetInt("id"),
                total_likes = json.GetInt("total_likes"),
                total_comments = json.GetInt("total_comments");
        string user = json.GetString("user"),
               content = json.GetString("content");
        DateTime data_pub = DateTime.Parse(json.GetString("data_pub"));
        bool user_liked = json.GetInt("user_liked") == 1;

        SetInfos(new(id, user, content, data_pub, total_likes, total_comments, user_liked));
    }

    public void UpdateInfos(Post post) => SetInfos(post);

    public void LikeBtn()
    {
        likeBtn.interactable = false;

        if (post.User_liked) conn.Like(this, post.ID, false);
        else conn.Like(this, post.ID, true);
    }

    public void CommentBtn()
    {
        if (CompareTag("Desabilitar")) return;
        commManager.OpenPostagem(post);
    }

    public void DeleteBtn() => popUpManager.SetPost(post);

    public void MudarLike(bool state)
    {
        if (state) likeImg.sprite = likePreenchido;
        else likeImg.sprite = likeVazio;

        likeBtn.interactable = true;
        Atualizar();
    }

    public void Atualizar() => conn.AtualizarPost(this, post.ID);
}