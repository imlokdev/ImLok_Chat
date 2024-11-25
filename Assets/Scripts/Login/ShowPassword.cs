using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowPassword : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] InputField passwordInput;

    public void OnPointerDown(PointerEventData eventData)
    {
        {
            var temp = passwordInput.text;
            passwordInput.text = null;
            passwordInput.contentType = InputField.ContentType.Standard;
            passwordInput.text = temp;
        }

        {
            Image image = GetComponent<Image>();
            var temp = image.color;
            temp.a = 1f;
            image.color = temp;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        {
            var temp = passwordInput.text;
            passwordInput.text = null;
            passwordInput.contentType = InputField.ContentType.Password;
            passwordInput.text = temp;
        }

        {
            Image image = GetComponent<Image>();
            var temp = image.color;
            temp.a = 0.5f;
            image.color = temp;
        }
    }
}
