using UnityEngine;
using UnityEngine.UI;

public class Comentario : MonoBehaviour
{
    TwitterConnection conn;

    PopUpManager popUpManager;

    Comment comment;

    [SerializeField] Text user, content, timer;

    private void Start()
    {
        conn = TwitterConnection.instance;
        popUpManager = transform.GetComponentInParent<PopUpLink>().popUpManager;
    }

    public void SetInfos(Comment _comment)
    {
        comment = _comment;

        user.text = comment.User;
        content.text = comment.Content;
        timer.text = Tools.DateTimeToTimer(comment.Data_pub, comment.Horario);
    }

    public void DeleteBtn() => popUpManager.SetComment(comment);
}