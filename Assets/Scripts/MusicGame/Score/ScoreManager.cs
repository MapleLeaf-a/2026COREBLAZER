using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public Score score;

    
    //µ¥Àý
    public static ScoreManager ScoreManagerInstance;

    void Awake()
    {
        if (ScoreManagerInstance == null)
        {
            ScoreManagerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        score = new Score();
    }
}
