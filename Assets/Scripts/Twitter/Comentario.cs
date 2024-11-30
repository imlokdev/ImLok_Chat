using UnityEngine;
using UnityEngine.UI;

public class Comentario : MonoBehaviour
{
    TwitterConnection conn;

    PopUpManager popUpManager;

    Comment comment;

    [SerializeField] Text user, content, timer;

    public float timeUpdateDatetime = 10f;
    private float timeCD;

    private void Start()
    {
        conn = TwitterConnection.instance;
        popUpManager = transform.GetComponentInParent<PopUpLink>().popUpManager;
    }

    private void Update()
    {
        if (Time.time - timeCD > timeUpdateDatetime)
        {
            timer.text = Tools.DateTimeToTimer(comment.Data_pub);
            timeCD = Time.time;
        }
    }

    public void SetInfos(Comment _comment)
    {
        comment = _comment;

        user.text = comment.User;
        content.text = comment.Content;
        timer.text = Tools.DateTimeToTimer(comment.Data_pub);
    }

    public void DeleteBtn() => popUpManager.SetComment(comment);
}