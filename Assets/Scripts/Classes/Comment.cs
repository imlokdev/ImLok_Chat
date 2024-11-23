using System;

public class Comment
{
    public int ID_post { get; private set; }
    public string User { get; private set; }
    public string Content { get; private set; }
    public DateTime Data_pub { get; private set; }
    public Comentario Comentario { get; private set; }

    public Comment(int id_post, string user, string content, DateTime data_pub)
    {
        ID_post = id_post;
        User = user;
        Content = content;
        Data_pub = data_pub;
    }

    public void SetClasse(Comentario objeto) => Comentario = objeto;
}