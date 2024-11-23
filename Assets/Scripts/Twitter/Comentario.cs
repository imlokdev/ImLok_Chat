using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Comentario : MonoBehaviour
{
    TwitterConnection conn;

    Comment comment;

    [SerializeField] Text user, content, timer;

    private void Start() => conn = TwitterConnection.instance;

    public void SetInfos(Comment _comment)
    {
        comment = _comment;

        user.text = comment.User;
        content.text = comment.Content;
        timer.text = Tools.DateTimeToTimer(comment.Data_pub);
    }
}