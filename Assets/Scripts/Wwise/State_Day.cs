using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Day : MonoBehaviour
{
    public static State_Day Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    public AK.Wwise.State[] dayStates;
    public void SetDay(int dayNumber)
    {
        int index = dayNumber - 1;
        if (dayStates != null && index >= 0 && index < dayStates.Length)
        {
            dayStates[index].SetValue();
        }
    }
}
