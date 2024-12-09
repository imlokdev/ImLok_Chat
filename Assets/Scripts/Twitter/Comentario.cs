using UnityEngine;
using UnityEngine.UI;

public class Comentario : MonoBehaviour
{
    CommentManager commentManager;
    PopUpManager popUpManager;

    Comment comment;

    [SerializeField] GameObject delete;
    [SerializeField] Text user, content, timer;

    private void Start()
    {
        popUpManager = transform.GetComponentInParent<PopUpLink>().popUpManager;
        commentManager = FindFirstObjectByType<CommentManager>();
    }

    public void SetInfos(Comment _comment)
    {
        comment = _comment;

        user.text = comment.User;
        content.text = comment.Content;
        timer.text = Tools.DateTimeToTimer(comment.Data_pub, comment.Horario);

        Conta conta = AccountManager.instance.Conta;
        if (comment.User == conta.User || conta.IsAdmin) delete.SetActive(true);
        else delete.SetActive(false);
    }

    public void DeleteBtn() => popUpManager.SetComment(comment);
}