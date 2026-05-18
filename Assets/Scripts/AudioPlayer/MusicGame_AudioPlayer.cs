using UnityEngine;

public class MusicGame_AudioPlayer : MonoBehaviour
{
	public static MusicGame_AudioPlayer instance;

	public AK.Wwise.Event MusicGamePlayer;
	public AK.Wwise.Event MusicGamePause;
	public AK.Wwise.Event MusicGameResume;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		PlayMusicGamesMusic();
	}

	public void PlayMusicGamesMusic()
	{
		MusicGamePlayer.Post(gameObject);
	}

	public void PauseMusic()
	{
		MusicGamePause.Post(gameObject);
	}

	public void ResumeMusic()
	{
		MusicGameResume.Post(gameObject);
	}
}