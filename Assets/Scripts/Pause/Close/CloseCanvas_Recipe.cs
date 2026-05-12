using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseCanvas_Recipe : CloseCanvas
{
    protected override void Close()
    {
        base.Close();
        InputManager.instance.SetContext(InputContext.CHARACTER);
    }
}
