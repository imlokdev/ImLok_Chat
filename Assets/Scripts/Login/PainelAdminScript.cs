using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Leguar.TotalJSON;
using System;

public class PainelAdminScript : MonoBehaviour
{
    MySQLConnection conn;

    [SerializeField] GameObject infosConta, proximoBtn, anteriorBtn;
    [SerializeField] Transform accounts;
    [SerializeField] Text paginasCount;

    Conta[] contas;

    readonly int firstID = 1;
    int accCountDisplay, lastID, page, totalPages;

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
        contas = SetBackup(count, contas);
        if (page == 0) page = 1;
        totalPages = (count + accCountDisplay - 1) / accCountDisplay;
        conn.SelectAccs(this, lastID, lastID + 3);
    }

    public void Proximo()
    {
        page++;
        lastID += 4;
        for (int i = 0; i < accounts.childCount; i++)
            accounts.GetChild(i).gameObject.SetActive(false);

        if (contas[lastID - firstID] == null) conn.SelectAccs(this, lastID, lastID + 3);
        else
        {
            int temp = 0;
            for (int i = lastID - firstID; temp < 4 && i < contas.Length; i++)
                SetContaTela(temp++, contas[i].ID, contas[i].User, contas[i].Email, contas[i].IsBlocked, contas[i].IsAdmin, contas[i].IsBanned);
        } 
    }

    public void Anterior()
    {
        page--;
        lastID -= 4;
        for (int i = 0; i < accounts.childCount; i++)
            accounts.GetChild(i).gameObject.SetActive(false);

        int temp = 0;
        for (int i = lastID - firstID; temp < 4; i++)
            SetContaTela(temp++, contas[i].ID, contas[i].User, contas[i].Email, contas[i].IsBlocked, contas[i].IsAdmin, contas[i].IsBanned);
    }

    public void Atualizar()
    {
        for (int i = 0; i < accounts.childCount; i++)
            accounts.GetChild(i).gameObject.SetActive(false);
        conn.CountAcc(this);
    }

    public void BlockBtn()
    {
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        Text idText = clickedButton.transform.parent.GetChild(0).GetComponent<Text>();

        conn.BlockAcc(this, clickedButton, idText.text, clickedButton.tag);

        clickedButton.interactable = false;
    }

    public void BanBtn()
    {
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        Text idText = clickedButton.transform.parent.GetChild(0).GetComponent<Text>();

        conn.BanAcc(this, clickedButton, idText.text, clickedButton.tag);

        clickedButton.interactable = false;
    }

    public void SetContas(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Api2Json(result));

        for (int i = 0; i < json.Length; i++)
        {
            int id = json[i].GetInt("id");
            string user = json[i].GetString("user"),
                   email = json[i].GetString("email");
            bool isBlocked = json[i].GetInt("isBlocked") == 1,
                 isAdmin = json[i].GetInt("isAdmin") == 1,
                 isBanned = json[i].GetInt("isBanned") == 1,
                 isConfirmed = json[i].GetInt("isConfirmed") == 1;
            DateTime created_at = DateTime.Parse(json[i].GetString("created_at"));
            DateTime.TryParse(json[i].GetString("last_login"), out DateTime lastLoginDate);
            
            Conta conta = new(id, user, email, isBlocked, isAdmin, isBanned, isConfirmed, created_at, lastLoginDate);
            SetContaTela(i, conta.ID, conta.User, conta.Email, conta.IsBlocked, conta.IsAdmin, conta.IsBanned);
            contas[id - 1] = conta;
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

    private void SetContaTela(int index, int id, string user, string email, bool isBlocked, bool isAdmin, bool isBanned)
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
        if (id == AccountManager.instance.Conta.ID) filho.GetChild(temp++).GetComponent<Button>().interactable = false;
        else
        {
            filho.GetChild(temp).GetComponent<Button>().interactable = true;
            if (isBlocked)
            {
                // Desbloquear
                filho.GetChild(temp).GetComponentInChildren<Text>().text = "Desbloquear";
                filho.GetChild(temp).tag = "0";
            }
            else
            {
                // Bloquear
                filho.GetChild(temp).GetComponentInChildren<Text>().text = "Bloquear";
                filho.GetChild(temp).tag = "1";
            }
            temp++;
        }

        // Set Button ban
        if (id == AccountManager.instance.Conta.ID) filho.GetChild(temp++).GetComponent<Button>().interactable = false;
        else
        {
            filho.GetChild(temp).GetComponent<Button>().interactable = true;
            if (isBanned)
            {
                // Desbanir
                filho.GetChild(temp).GetComponentInChildren<Text>().text = "Desbanir";
                filho.GetChild(temp).tag = "0";
            }
            else
            {
                // Banir
                filho.GetChild(temp).GetComponentInChildren<Text>().text = "Banir";
                filho.GetChild(temp).tag = "1";
            }
            temp++;
        }

        // Set Button resent email
        

        filho.gameObject.SetActive(true);
    }

    private void AtualizarBtns()
    {
        paginasCount.text = $"{page}/{totalPages}";

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

    private Conta[] SetBackup(int lenght, Conta[] backup)
    {
        Conta[] temp = new Conta[lenght];
        if (backup != null)
            for (int i = 0; i < lenght; i++)
                temp[i] = backup[i];
        return temp;
    }
}