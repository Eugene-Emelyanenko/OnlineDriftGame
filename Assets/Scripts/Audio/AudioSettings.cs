using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Audio Mixer")]
    [Space(5)]
    [SerializeField] private AudioMixer audioMixer;

    [Space(5)]
    [Header("Music")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValueText;
    private float musicValue;

    [Space(5)]
    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    private float sfxValue;

    [Space(5)]
    [Header("Car")]
    [SerializeField] private Slider carSlider;
    [SerializeField] private TextMeshProUGUI carValueText;
    private float carValue;

    [Space(5)]
    [Header("Default Settings")]
    [SerializeField][Range(0f, 1f)] private float defaultMusicValue = 0.75f;
    [SerializeField][Range(0f, 1f)] private float defaultSfxValue = 0.75f;
    [SerializeField][Range(0f, 1f)] private float defaultCarValue = 0.75f;

    private void Start()
    {
        LoadSettings();

        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChange);

        sfxSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.AddListener(OnSfxVolumeChange);

        carSlider.onValueChanged.RemoveAllListeners();
        carSlider.onValueChanged.AddListener(OnCarVolumeChange);
    }

    public void OnMusicVolumeChange(float value)
    {
        musicValue = value;
        SetVolume("Music", musicValue, musicValueText);
        PlayerPrefs.SetFloat("Music", musicValue);
    }

    public void OnSfxVolumeChange(float value)
    {
        sfxValue = value;
        SetVolume("Sfx", sfxValue, sfxValueText);
        PlayerPrefs.SetFloat("Sfx", sfxValue);
    }

    public void OnCarVolumeChange(float value)
    {
        carValue = value;
        SetVolume("Car", carValue, carValueText);
        PlayerPrefs.SetFloat("Car", carValue);
    }

    private void SetVolume(string parameter, float value, TextMeshProUGUI valueText)
    {
        value /= 10f;
        if (value <= 0)
        {
            audioMixer.SetFloat(parameter, -80f);
            valueText.text = "0.0";
        }
        else
        {
            audioMixer.SetFloat(parameter, Mathf.Log10(value) * 20);
            valueText.text = value.ToString("0.0");
        }
    }

    private void LoadSettings()
    {
        musicValue = PlayerPrefs.GetFloat("Music", defaultMusicValue * 10f);
        sfxValue = PlayerPrefs.GetFloat("Sfx", defaultSfxValue * 10f);
        carValue = PlayerPrefs.GetFloat("Car", defaultCarValue * 10f);

        SetVolume("Music", musicValue, musicValueText);
        SetVolume("Sfx", sfxValue, sfxValueText);
        SetVolume("Car", carValue, carValueText);

        musicSlider.value = musicValue;
        sfxSlider.value = sfxValue;
        carSlider.value = carValue;
    }
}
