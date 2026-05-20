using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public AK.Wwise.Event MusicPlayer;

    public AK.Wwise.State state;

    private void Start()
    {
        PlayMusic();
        state.SetValue();
    }

    public void PlayMusic()
    {
        MusicPlayer.Post(gameObject);
    }
}
