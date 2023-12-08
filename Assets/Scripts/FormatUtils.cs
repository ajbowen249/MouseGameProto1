using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormatUtils : MonoBehaviour
{
    public static string FormatHours(float time)
    {
        var hours = Mathf.Floor(time);
        var minutes = Mathf.Floor((time - hours) * 60f);
        var minutePad = minutes < 10 ? "0" : "";
        return $"{hours}:{minutePad}{minutes}";
    }

    public static string FormatMoney(float money)
    {
        return $"${money.ToString("0.00")}";
    }
}
