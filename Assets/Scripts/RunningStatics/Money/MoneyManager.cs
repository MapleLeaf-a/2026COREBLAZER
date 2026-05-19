using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoneyManager
{
    public static event Action<int> OnMoneyChanged;  // Ìí¼ÓÊÂ¼þ

    private static int money = 0;

    public static int Money => money;

    public static void IncreaseMoney(int incr)
    { 
        money += incr;
        OnMoneyChanged?.Invoke(money);
    }
}
