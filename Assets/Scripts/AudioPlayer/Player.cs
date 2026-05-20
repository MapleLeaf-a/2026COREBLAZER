using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AK.Wwise.Event MusicPlayer;

    private void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        MusicPlayer.Post(gameObject);
    }
}
