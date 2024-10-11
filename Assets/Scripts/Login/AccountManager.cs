using System;
using UnityEngine;
using UnityEngine.UI;

public class AccountManager : MonoBehaviour
{
    public static AccountManager instance;

    public int ID { get; private set; }
    public string Login { get; private set; }
    public string Email { get; private set; }
    public bool IsBlocked { get; private set; }
    public bool IsAdmin { get; private set; }
    public DateTime Created_at { get; private set; }

    [SerializeField] GameObject painelAdmin, login;
    [SerializeField] Text feedback, informacoes;
    [SerializeField] Button sairBtn;

    private void Awake()
    {
        if (instance == null) instance = this;
        gameObject.SetActive(false);
        for (int i = 0; i < 2; i++)
            transform.GetChild(i).gameObject.SetActive(true);
    }

    public void SetInfos(int _id, string _login, string _email, bool _isBlocked, bool _isAdmin, DateTime _created_at)
    {
        ID = _id;
        Login = _login;
        Email = _email;
        IsBlocked = _isBlocked;
        IsAdmin = _isAdmin;
        Created_at = _created_at;

        if (IsBlocked && !IsAdmin)
        {
            feedback.text = "Sua conta está bloqueada, peça a um administrador para liberá-la.";
            feedback.color = Color.red;
        }
        else
        {
            string text = "";

            text += $"id: {ID}\n\n";
            text += $"Login: {Login}\n\n";
            text += $"Email: {Email}\n\n";
            text += $"Sua conta foi criada há {(DateTime.Today - Created_at).Days} dias.\n\n";
            if (IsAdmin) text += "<Color=Yellow>~Conta Admin~</Color>";

            informacoes.text = text;
            informacoes.gameObject.SetActive(true);
        }

        sairBtn.gameObject.SetActive(true);
    }

    public void SairButton()
    {
        gameObject.SetActive(false);
        login.SetActive(true);
        feedback.text = null;
    }

    public void PainelAdmin()
    {
        gameObject.SetActive(false);
        painelAdmin.SetActive(true);
        feedback.text = null;
    }
}