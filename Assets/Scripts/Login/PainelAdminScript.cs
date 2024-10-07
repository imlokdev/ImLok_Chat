using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leguar.TotalJSON;
using System.Reflection;
using UnityEngine.EventSystems;

public class PainelAdminScript : MonoBehaviour
{
    MySQLConnection conn;

    [SerializeField] GameObject infosConta, proximoBtn, anteriorBtn;
    [SerializeField] Transform accounts;
    [SerializeField] Text paginasCount;

    Conta[] contas;

    public int accCount, accCountDisplay, firstID = 4, lastID;
    int page, totalPages;

    private void Awake() => accCountDisplay = accounts.childCount;

    private void Start()
    {
        conn = MySQLConnection.instance;
        conn.CountAcc(this);
        lastID = firstID;
    }

    private void Update() => AtualizarBtns();

    public void InfosConta()
    {
        gameObject.SetActive(false);
        infosConta.SetActive(true);
    }

    public void SetCount(int count)
    {
        accCount = count;
        page = 1;

        totalPages = (accCount + accCountDisplay - 1) / accCountDisplay;
        paginasCount.text = $"1/{totalPages}";

        conn.SelectAccs(this, lastID, lastID + 3);
    }

    // Falta uma forma de bloquear multiplos cliques, antes de realmente atualizar
    public void Proximo()
    {
        page++;
        lastID += 4;
        for (int i = 0; i < accounts.childCount; i++)
            accounts.GetChild(i).gameObject.SetActive(false);
        conn.SelectAccs(this, lastID, lastID + 3);
    }
    public void Anterior()
    {
        page--;
        lastID -= 4;
        for (int i = 0; i < accounts.childCount; i++)
            accounts.GetChild(i).gameObject.SetActive(false);
        conn.SelectAccs(this, lastID, lastID + 3);
    }
    // Criar botao atualizar infos
    public void BlockBtn(Text idText)
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        print(idText.text);

        switch (clickedButton.tag)
        {
            // Inciar connexao MySQL com o id
            case "Bloquear": clickedButton.tag = "Desbloquear"; break;
            case "Desbloquear": clickedButton.tag = "Bloquear"; break;
        }
    }

    public void SetContas(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Api2Json(result));
        contas = new Conta[json.Length];

        for (int i = 0; i < contas.Length; i++)
        {
            int id = json[i].GetInt("id");
            string user = json[i].GetString("login"),
                   email = json[i].GetString("email");
            bool isBlocked = json[i].GetInt("isBlocked") == 1,
                 isAdmin = json[i].GetInt("isAdmin") == 1;

            contas[i] = new(id, user, email, isBlocked, isAdmin);
            SetContaTela(i, contas[i].ID, contas[i].User, contas[i].Email, contas[i].IsBlocked, contas[i].IsAdmin);
        }
    }

    private string Api2Json(string apiString)
    {
        var temp = apiString.Replace("[", "").Replace("]", "").Trim();

        while (true)
        {
            int index = -1;

            for (int i = 0; i < temp.Length; i++)
                if (i + 1 < temp.Length)
                    if (temp[i] == '}' && temp[i + 1] == ',')
                    {
                        index = i + 1;
                        break;
                    }

            if (index < 0) break;
            else temp = temp.Remove(index, 1);
        }

         return temp;
    }

    private void SetContaTela(int index, int id, string user, string email, bool isBlocked, bool isAdmin)
    {
        Transform filho = accounts.GetChild(index);
        int temp = 0;

        // Set id
        filho.GetChild(temp++).GetComponent<Text>().text = id.ToString();
        // Set User
        if (!isAdmin) filho.GetChild(temp++).GetComponent<Text>().text = user;
        else filho.GetChild(temp++).GetComponent<Text>().text = Tools.ColorirTextoHtml(user, user, Color.green);
        // Set Email
        filho.GetChild(temp++).GetComponent<Text>().text = email;
        // Set Button block
        {
            if (isBlocked)
            {
                string a = "Desbloquear";
                filho.GetChild(temp).GetComponentInChildren<Text>().text = a;
                filho.GetChild(temp).tag = a;
                temp++;
            }
            else
            {
                string a = "Bloquear";
                filho.GetChild(temp).GetComponentInChildren<Text>().text = a;
                filho.GetChild(temp).tag = a;
                temp++;
            }
        }

        filho.gameObject.SetActive(true);
    }

    private void AtualizarBtns()
    {
        if (page < totalPages && page > 1)
        {
            proximoBtn.SetActive(true);
            anteriorBtn.SetActive(true);
        }
        else if (page < totalPages)
        {
            proximoBtn.SetActive(true);
            anteriorBtn.SetActive(false);
        }
        else if (page > 1)
        {
            proximoBtn.SetActive(false);
            anteriorBtn.SetActive(true);
        }
        else
        {
            proximoBtn.SetActive(false);
            anteriorBtn.SetActive(false);
        }
    }
}