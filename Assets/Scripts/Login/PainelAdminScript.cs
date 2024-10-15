using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Leguar.TotalJSON;

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
        var temp = SetBackup(count, contas);
        contas = temp;
        page = 1;

        totalPages = (count + accCountDisplay - 1) / accCountDisplay;
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

        if (contas[lastID - firstID] == null) conn.SelectAccs(this, lastID, lastID + 3);
        else
        {
            int temp = 0;
            for (int i = lastID - firstID; temp < 4 && i < contas.Length; i++)
                SetContaTela(temp++, contas[i].ID, contas[i].User, contas[i].Email, contas[i].IsBlocked, contas[i].IsAdmin);
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
            SetContaTela(temp++, contas[i].ID, contas[i].User, contas[i].Email, contas[i].IsBlocked, contas[i].IsAdmin);
    }

    public void Atualizar()
    {
        for (int i = 0; i < accounts.childCount; i++)
            accounts.GetChild(i).gameObject.SetActive(false);
        conn.CountAcc(this);
    }

    public void BlockBtn(Text idText)
    {
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        switch (clickedButton.tag)
        {
            case "Bloquear": conn.BlockAcc(this, clickedButton, idText.text, true); ; break;
            case "Desbloquear": conn.BlockAcc(this, clickedButton, idText.text, false); break;
        }

        clickedButton.interactable = false;
    }

    public void BlockBtnsLista(bool activate)
    {
        if (activate)
        {

        }
        else
        {

        }
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
                 isBanned = json[i].GetInt("isBanned") == 1;

            contas[id - 1] = new(id, user, email, isBlocked, isAdmin, isBanned);
            SetContaTela(i, contas[id - 1].ID, contas[id - 1].User, contas[id - 1].Email, contas[id - 1].IsBlocked, contas[id - 1].IsAdmin);
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

    private Conta[] SetBackup(int lenght, Conta[] backup)
    {
        Conta[] temp = new Conta[lenght];
        if (backup != null)
            for (int i = 0; i < lenght; i++)
                temp[i] = backup[i];
        return temp;
    }
}