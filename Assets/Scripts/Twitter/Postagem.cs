using System;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class Postagem : MonoBehaviour
{
    TwitterConnection conn;
    TwitterSystem twitterSystem;

    Post post;
    CommentManager commManager;
    PopUpManager popUpManager;

    [SerializeField] GameObject delete;
    [SerializeField] Image likeImg;
    [SerializeField] Text user, content, likes, comments, timer;
    [SerializeField] Button likeBtn;
    [SerializeField] Sprite likeVazio, likePreenchido;

    private void Start()
    {
        conn = TwitterConnection.instance;
        commManager = transform.parent.GetComponent<CommentManager>();
        if (!CompareTag("Desabilitar"))
        {
            popUpManager = commManager.popUpManager;
            GetComponent<Button>().onClick.AddListener(() => commManager.OpenPostagem(post));

            Conta conta = AccountManager.instance.Conta;
            if (post.User == conta.User || conta.IsAdmin) delete.SetActive(true);
        }
    }

    public void AutoDeletePost(string result, long httpcode)
    {
        if (httpcode != 500) return;

        JSON json = JSON.ParseString(result);

        if (json.GetString("error").Contains("foreign key") || json.GetString("state") == "deleted")
            twitterSystem.DeletePost(post);
    }

    public void AutoDeletePost() => twitterSystem.DeletePost(post);

    public void SetInfos(Post _post)
    {
        post = _post;

        user.text = post.User;
        content.text = post.Content;
        likes.text = post.Total_likes.ToString();
        comments.text = post.Total_comments.ToString();
        timer.text = Tools.DateTimeToTimer(post.Data_pub, post.Horario);

        if (post.User_liked) likeImg.sprite = likePreenchido;
        else likeImg.sprite = likeVazio;
    }

    public void SetInfos(int total_likes, int total_comments, DateTime horario, bool user_liked)
    {
        post.UpdateInfos(total_likes, total_comments, user_liked, horario);

        likes.text = post.Total_likes.ToString();
        comments.text = post.Total_comments.ToString();
        timer.text = Tools.DateTimeToTimer(post.Data_pub, post.Horario);

        if (post.User_liked) likeImg.sprite = likePreenchido;
        else likeImg.sprite = likeVazio;
    }

    public void SetScript(TwitterSystem script) => twitterSystem = script;

    public void UpdateInfos(string infos)
    {
        JSON json = JSON.ParseString(infos);

        int total_likes = json.GetInt("total_likes"),
            total_comments = json.GetInt("total_comments");
        DateTime horario = DateTime.Parse(json.GetString("horario"));
        bool user_liked = json.GetInt("user_liked") == 1;

        SetInfos(total_likes, total_comments, horario, user_liked);
    }

    public void UpdateInfos(Post post) => SetInfos(post);

    public void LikeBtn()
    {
        likeBtn.interactable = false;

        if (post.User_liked) conn.Like(this, post.ID, false, likeBtn);
        else conn.Like(this, post.ID, true, likeBtn);
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