using Leguar.TotalJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitterSystem : MonoBehaviour
{
    TwitterConnection conn;

    [SerializeField] GameObject infosConta, painelPostagem, prefab_Post;
    [SerializeField] Transform content;

    List<Post> posts = new();

    private void Start()
    {
        conn = TwitterConnection.instance;
    }

    public void ContaButton()
    {
        gameObject.SetActive(false);
        infosConta.SetActive(true);
    }

    public void AbrirPainelPostagem()
    {
        gameObject.SetActive(false);
        painelPostagem.SetActive(true);
    }

    public void CriarPosts(string result)
    {
        JSON[] json = JSON.ParseStringToMultiple(Api2Json(result));

        print(json.Length);
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
}