using System;

public class Post
{
    public int ID { get; private set; }
    public string User {  get; private set; }
    public string Content { get; private set; }
    public DateTime Data_pub { get; private set; }
    public int Total_likes { get; private set; }
    public int Total_comments { get; private set; }
    public bool User_liked { get; private set; }

    public Post (int id, string user, string content, DateTime data_pub, int total_likes, int total_comments, bool user_liked)
    {
        ID = id;
        User = user;
        Content = content;
        Data_pub = data_pub;
        Total_likes = total_likes;
        Total_comments = total_comments;
        User_liked = user_liked;
    }
}