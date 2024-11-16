using System;
using System.Globalization;

using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class Postagem : MonoBehaviour
{
    TwitterConnection conn;

    Post post;

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
    }

    private void Update()
    {
        if (Time.time -  timeCD > timeUpdateDatetime)
        {
            timer.text = DateTimeToTimer(post.Data_pub);
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
        timer.text = DateTimeToTimer(post.Data_pub);

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

    private string DateTimeToTimer(DateTime data_pub)
    {
        TimeSpan interval = DateTime.Now - data_pub;

        if (interval.TotalDays >= 365) 
            return $"{data_pub.Day} {data_pub.ToString("MMM", new CultureInfo("pt-BR"))} {data_pub.Year}";
        if (interval.TotalDays >= 1)
            return $"{data_pub.Day} {data_pub.ToString("MMM", new CultureInfo("pt-BR"))}";
        if (interval.TotalHours >= 1) 
            return $"{interval.Hours} h";
        if (interval.TotalMinutes >= 1)
            return $"{interval.Minutes} min";
        if (interval.TotalSeconds > 0)
            return $"{interval.Seconds} s";
        return null;
    }

    public void LikeBtn()
    {
        likeBtn.interactable = false;

        if (post.User_liked) conn.Like(this, 1, post.ID, false);
        else conn.Like(this, 1, post.ID, true);
    }

    public void CommentBtn() => print("Clicando no comentario");

    public void MudarLike(bool state)
    {
        if (state) likeImg.sprite = likePreenchido;
        else likeImg.sprite = likeVazio;

        likeBtn.interactable = true;
        Atualizar();
    }

    public void Atualizar() => conn.AtualizarPost(this, 1, post.ID);
}