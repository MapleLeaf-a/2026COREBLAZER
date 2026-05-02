using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicGame_AudioPlayer : MonoBehaviour
{
    public AK.Wwise.Event MusicGamePlayer;

    private void Start()
    {
        PlayMusicGamesMusic();
    }

    private void PlayMusicGamesMusic()
    {
        MusicGamePlayer.Post(gameObject);
    }
}
