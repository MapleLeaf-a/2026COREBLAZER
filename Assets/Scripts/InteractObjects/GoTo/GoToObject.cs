using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToObject : InteractableObject
{
    protected string sceneName;

    public override void InteractLogics()
    {
        GoToScene(sceneName);
    }

    public void GoToScene(string sceneName)
    { 
        SceneManager.LoadScene(sceneName);
    }
}
