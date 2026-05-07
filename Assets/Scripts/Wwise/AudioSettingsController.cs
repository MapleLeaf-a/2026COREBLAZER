using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

public class AudioSettingsController : MonoBehaviour
{
    [Header("Wwise RTPC Parameters")]
    public RTPC musicRTPC = new RTPC();
    public RTPC sfxRTPC = new RTPC();
    public RTPC masterRTPC = new RTPC();

    [Header("UI Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider masterVolumeSlider;

    private void Start()
    {
        // 初始化滑块值（从记忆加载或使用默认值）
        musicVolumeSlider.value = PlayerPrefs.GetFloat("music_volume", 0.8f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("sfx_volume", 0.8f);
        masterVolumeSlider.value = PlayerPrefs.GetFloat("master_volume", 0.8f);

        // 立即应用初始值
        SetMusicVolume(musicVolumeSlider.value);
        SetSFXVolume(sfxVolumeSlider.value);
        SetMasterVolume(masterVolumeSlider.value);

        // 添加监听事件
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
    }

    public void SetMusicVolume(float value)
    {
        // 设置RTPC值（0-100范围）
        musicRTPC.SetGlobalValue(value * 100f);
        PlayerPrefs.SetFloat("music_volume", value);
    }

    public void SetSFXVolume(float value)
    {
        // 设置RTPC值（0-100范围）
        sfxRTPC.SetGlobalValue(value * 100f);
        PlayerPrefs.SetFloat("sfx_volume", value);
    }

    public void SetMasterVolume(float value)
    {
        // 设置RTPC值（0-100范围）
        masterRTPC.SetGlobalValue(value * 100f);
        PlayerPrefs.SetFloat("master_volume", value);
    }
}