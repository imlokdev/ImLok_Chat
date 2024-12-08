using System;
using System.Collections.Generic;
using UnityEngine;

public class Post
{
    public int ID { get; private set; }
    public string User {  get; private set; }
    public string Content { get; private set; }
    public DateTime Data_pub { get; private set; }
    public DateTime Horario { get; private set; }
    public int Total_likes { get; private set; }
    public int Total_comments { get; private set; }
    public bool User_liked { get; private set; }
    public Postagem Postagem { get; private set; }
    public LinkedList<Comment> Comments { get; private set; }

    public Post (int id) => ID = id;

    public Post (int id, string user, string content, DateTime data_pub, DateTime horario, int total_likes, int total_comments, bool user_liked)
    {
        ID = id;
        User = user;
        Content = content;
        Data_pub = data_pub;
        Horario = horario;
        Total_likes = total_likes;
        Total_comments = total_comments;
        User_liked = user_liked;
        Comments = new();
    }

    public void SetClasse(Postagem objeto) => Postagem = objeto;

    public void UpdateInfos(int total_likes, int total_comments, bool user_liked, DateTime horario)
    {
        Total_likes = total_likes;
        Total_comments = total_comments;
        User_liked = user_liked;
        Horario = horario;
        Postagem.UpdateInfos(this);
    }

    public string ToString() => 
        $"ID: {ID}, USER: {User}, CONTENT: {Content}, DATA_PUB: {Data_pub}, USER_LIKED: {User_liked}";

}