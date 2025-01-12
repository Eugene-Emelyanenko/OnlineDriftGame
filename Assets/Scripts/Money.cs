using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Money
{
    public static readonly string MoneyKey = "Money";

    public static int GetMoney() => PlayerPrefs.GetInt(MoneyKey, 0);

    public static void SetMoney(int value)
    {
        PlayerPrefs.SetInt(MoneyKey, value);
        PlayerPrefs.Save();
    }

    public static void AddMoney(int value)
    {
        int money = GetMoney();
        money += value;
        SetMoney(money);
    }
}
