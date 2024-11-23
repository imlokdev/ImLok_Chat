using System;
using System.Globalization;
using UnityEngine;

public class Tools
{
    public static bool IsInMainCamera(Vector3 position)
    {
        var temp = Camera.main.WorldToViewportPoint(position);
        if (temp.x > 1f || temp.x < 0f || temp.y > 1f || temp.y < 0f) return false;
        return true;
    }

    public static string RemoveCloneName(string name)
    {
        var index = name.IndexOf("(Clone)");
        if (index >= 0) return name.Remove(index);
        else return name;
    }

    public static string ColorirTextoHtml(string text, string textToColor, Color color)
    {
        string temp = text;
        temp = temp.Insert(temp.IndexOf(textToColor), $"<Color={RGBToHEXA(color)}>");
        temp = temp.Insert(temp.IndexOf(textToColor) + textToColor.Length, "</Color>");
        return temp;
    }

    public static string ColorirTextoHtml(string text, string textToColor, string color)
    {
        string temp = text;
        temp = temp.Insert(temp.IndexOf(textToColor), $"<Color={color}>");
        temp = temp.Insert(temp.IndexOf(textToColor) + textToColor.Length, "</Color>");
        return temp;
    }

    public static string RGBToHEXA(Color color)
    {
        // Converta os valores de ponto flutuante para inteiros no intervalo de 0 a 255
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);

        // Crie o código hexadecimal
        string codigoHex = string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);

        return codigoHex;
    }

    public static Sprite GetSpriteInResources(string sprAll, string sprTarget)
    {
        foreach (var item in Resources.LoadAll<Sprite>(sprAll))
            if (item.name == sprTarget) return item;
        return null;
    }

    public static (Vector2,Vector2) GetAreaSprObject(SpriteRenderer objeto)
    {
        Vector2 point1 = objeto.transform.position;
        point1.x += -objeto.bounds.extents.x;
        point1.y += objeto.bounds.extents.y;

        Vector2 point2 = objeto.transform.position;
        point1.x += objeto.bounds.extents.x;
        point1.y += -objeto.bounds.extents.y;

        return (point1, point2);
    }

    public static bool IsInAreaObject(Transform objeto, (Vector2, Vector2) areaTarget)
    {
        Vector2 point1 = areaTarget.Item1, point2 = areaTarget.Item2;

        if (objeto.position.x >= point1.x && objeto.position.x <= point2.x &&
            objeto.position.y >= point2.y && objeto.position.y <= point1.y)
            return true;

        return false;
    }

    public static Vector3 VectorRound(Vector3 position, int digits = 2)
    {
        return new Vector3()
        {
            x = (float)Math.Round(position.x, digits),
            y = (float)Math.Round(position.y, digits),
            z = (float)Math.Round(position.z, digits),
        };
    }

    public static Vector2 VectorRound(Vector2 position, int digits = 2)
    {
        return new Vector2()
        {
            x = (float)Math.Round(position.x, digits),
            y = (float)Math.Round(position.y, digits),
        };
    }

    public static string SecondsToMinAndSec(float time)
    {
        if (time < 0) return null;

        return $"{(int)time / 60:00}:{time - (((int)(time / 60)) * 60):00}";
    }

    public static string SecondsToMinAndSec(double time)
    {
        if (time < 0) return null;

        return $"{(int)time/60:00}:{time-(((int)(time / 60))*60):00}";
    }

    public static string Api2Json(string apiString)
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

    public static string DateTimeToTimer(DateTime data_pub)
    {
        TimeSpan interval = DateTime.Now - data_pub;

        if (interval.TotalDays >= 365)
            return $"{data_pub.Day} {data_pub.ToString("MMM", new CultureInfo("pt-BR"))} {data_pub.Year}";
        if (interval.TotalDays >= 1)
            return $"{data_pub.Day} {data_pub.ToString("MMM", new CultureInfo("pt-BR"))}";
        if (interval.TotalHours >= 1)
            return $"{interval.Hours} h";
        if (interval.TotalMinutes >= 1)
            return $"{interval.Minutes} min";
        if (interval.TotalSeconds > 0)
            return $"{interval.Seconds} s";
        return null;
    }
}