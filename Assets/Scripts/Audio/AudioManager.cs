using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class Sound
{
    public string soundName;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [Space(5)]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Space(5)]
    [Header("Sounds")]
    [SerializeField] List<Sound> sounds = new List<Sound>();

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic();
    }

    public void PlaySFX(string soundName)
    {
        AudioClip soundClip = FindClip(soundName);

        if (soundClip != null)
        {
            sfxSource.PlayOneShot(soundClip);
        }
        else
        {
            Debug.LogWarning($"Cannot find sound: {soundClip.name}");
        }
    }

    public void PlayMusic()
    {
        AudioClip musicClip = FindClip("Music");

        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public AudioClip FindClip(string clipName) => sounds.Find(s => s.soundName == clipName).clip;

    private void OnDestroy()
    {
        sfxSource.Stop();
        musicSource.Stop();
    }
}