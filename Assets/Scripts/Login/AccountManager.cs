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
    public bool IsBanned { get; private set; }
    public DateTime Created_at { get; private set; }
    public DateTime Last_login { get; private set; }

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

    public void SetInfos(int _id, string _login, string _email, bool _isBlocked, bool _isAdmin, bool _isBanned, DateTime _created_at, DateTime _last_login)
    {
        ID = _id;
        Login = _login;
        Email = _email;
        IsBlocked = _isBlocked;
        IsAdmin = _isAdmin;
        IsBanned = _isBanned;
        Created_at = _created_at;
        Last_login = _last_login;

        if (IsBanned)
        {
            feedback.text = "Sua conta está banida.";
            feedback.color = Color.red;
        }
        else if (IsBlocked && !IsAdmin)
        {
            feedback.text = "Sua conta está bloqueada, peça a um administrador para liberá-la.";
            feedback.color = Color.yellow;
        }

        string text = "";

        text += $"id: {ID}\n";
        text += $"Login: {Login}\n";
        text += $"Email: {Email}\n\n";
        text += $"Sua conta foi criada há {(DateTime.Now - Created_at).Days} dias.\n\n";
        if (Last_login != DateTime.MinValue) text += $"Seu ultimo login foi há {(DateTime.Now - Last_login).Days} dias.\n\n";
        else if (!IsBlocked || IsAdmin) text += "Este é o seu primeiro login, seja bem vindo.\n\n";
        if (IsAdmin) text += "<Color=Yellow>~Conta Admin~</Color>";

        informacoes.text = text;
        informacoes.gameObject.SetActive(true);

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