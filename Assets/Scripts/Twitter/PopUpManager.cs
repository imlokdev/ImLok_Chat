using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    TwitterConnection conn;

    [SerializeField] TwitterSystem twitterSystem;

    enum Publi {None, Post, Comment}
    Publi choice = Publi.None;

    Post post;
    Comment comment;

    [SerializeField] Button[] buttons;

    private void Start() => conn = TwitterConnection.instance;

    public void CancelBtn()
    {
        if ((post != null || comment != null) && choice == Publi.None) return;

        gameObject.SetActive(false);
        post = null;
        comment = null;
    }

    public void SetPost(Post post)
    {
        this.post = post;
        choice = Publi.Post;
        gameObject.SetActive(true);
    }

    public void SetComment(Comment comment)
    {
        this.comment = comment;
        choice = Publi.Comment;
        gameObject.SetActive(true);
    }

    public void DeleteBtn()
    {
        foreach (var item in buttons)
            item.interactable = false;

        switch (choice)
        {
            case Publi.Post:
                {
                    conn.DeletarPost(this, post.ID, buttons);
                    break;
                }
            case Publi.Comment:
                {
                    conn.DeletarComment(this, comment.ID, buttons);
                    break;
                }
        }

        choice = Publi.None;
    }

    public void DeletarPostagem()
    {
        twitterSystem.DeletePost(post);

        gameObject.SetActive(false);
        post = null;
    }

    public void DeletarComentario()
    {
        twitterSystem.DeleteComment(comment);

        gameObject.SetActive(false);
        comment = null;
    }
}