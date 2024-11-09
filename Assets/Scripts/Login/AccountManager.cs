using System;

using UnityEngine;
using UnityEngine.UI;

public class AccountManager : MonoBehaviour
{
    public static AccountManager instance;

    public Conta Conta { get; private set; }

    [SerializeField] GameObject painelAdmin, login, twitter;
    [SerializeField] Text feedback, informacoes;
    [SerializeField] Button sairBtn;

    private void Awake()
    {
        if (instance == null) instance = this;
        gameObject.SetActive(false);
        for (int i = 0; i < 2; i++) transform.GetChild(i).gameObject.SetActive(true);
    }

    public void SetInfos(Conta _conta)
    {
        Conta = _conta;

        if (Conta.IsBanned)
        {
            feedback.text = "Sua conta est� banida.";
            feedback.color = Color.red;
        }
        else if (!Conta.IsConfirmed)
        {
            feedback.text = "Seu email ainda n�o foi confirmado, por favor verifique seu email";
            feedback.color = Color.yellow;
        }
        else if (Conta.IsBlocked && !Conta.IsAdmin)
        {
            feedback.text = "Sua conta est� bloqueada, pe�a a um administrador para liber�-la.";
            feedback.color = Color.yellow;
        }

        string text = "";
        TimeSpan interval = DateTime.Now - Conta.Created_at;

        text += $"id: {Conta.ID}\n";
        text += $"User: {Conta.User}\n";
        text += $"Email: {Conta.Email}\n\n";
        text += $"Sua conta foi criada h� {TimeSpanToString(interval)}.\n\n";
        if (Conta.Last_login != DateTime.MinValue)
        {
            TimeSpan interval2 = DateTime.Now - Conta.Last_login;
            text += $"Seu ultimo login foi h� {TimeSpanToString(interval2)}.\n\n";
        }
        else if (!Conta.IsBlocked || Conta.IsAdmin) text += "Este � o seu primeiro login, seja bem vindo.\n\n";
        if (Conta.IsAdmin) text += "<Color=Yellow>~Conta Admin~</Color>";

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

    public void IniciarButton()
    {
        gameObject.SetActive(false);
        twitter.SetActive(true);
        feedback.text = null;
    }

    private string TimeSpanToString(TimeSpan interval)
    {
        string temp = "";
        if (interval.TotalDays >= 1) temp += $"{interval.Days} dias, ";
        if (interval.TotalHours >= 1) temp += $"{interval.Hours} horas, ";
        if (interval.TotalMinutes >= 1) temp += $"{interval.Minutes} minutos e ";

        temp += $"{interval.Seconds} segundos";

        return temp;
    }
}