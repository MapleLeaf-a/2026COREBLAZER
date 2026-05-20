using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayer : MonoBehaviour
{
    public AK.Wwise.State title;

    void Start()
    {
        title.SetValue();
    }
}
