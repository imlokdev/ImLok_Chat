using UnityEngine;
using UnityEngine.UI;

using Leguar.TotalJSON;

public class SessionManager : MonoBehaviour
{
    [SerializeField] GameObject[] screens;
    [SerializeField] GameObject telaLogin;
    [SerializeField] Text feedback;

    public void FinalizarSessao(string result, long httpcode)
    {
        JSON json = JSON.ParseString(result);

        print(json.GetString("error"));

        if (json.GetString("error") == "session_expired" && httpcode == 401)
        {
            foreach (var item in screens)
                item.SetActive(false);

            telaLogin.SetActive(true);

            feedback.text = "Sessão expirada";
            feedback.color = Color.red;
        }
    }
}
