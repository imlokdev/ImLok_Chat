using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Postagem : MonoBehaviour
{
    Post post;
    [SerializeField] Text user, content, likes, comments, timer;

    public void SetInfos(Post _post)
    {
        post = _post;

        user.text = post.User;
        content.text = post.Content;
        likes.text = post.Total_likes.ToString();
        comments.text = post.Total_comments.ToString();
        timer.text = DateTimeToTimer(post.Data_pub);
    }

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
}