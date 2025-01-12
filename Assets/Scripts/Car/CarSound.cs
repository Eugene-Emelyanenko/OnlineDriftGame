using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CarSound : MonoBehaviour
{
    [Header("Audio")]
    [Space(5)]
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource tireSource;

    private float startEngineSourcePitch;
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        startEngineSourcePitch = engineSource.pitch;

        engineSource.Play();
    }

    public void HandleSounds(float carVelocity, bool isDrifting)
    {
        if (engineSource != null)
        {
            float engineSoundPitch = startEngineSourcePitch + (Mathf.Abs(carVelocity) / 25f);
            engineSource.pitch = engineSoundPitch;
        }
        if (isDrifting)
        {
            if (!tireSource.isPlaying)
            {
                tireSource.Play();
            }
        }
        else if (!isDrifting)
        {
            tireSource.Stop();
        }
    }

    public void PhotonHandleSounds(float carVelocity, bool isDrifting)
    {
        photonView.RPC("RPCHandleSounds", RpcTarget.All, carVelocity, isDrifting);
    }

    [PunRPC]
    public void RPCHandleSounds(float carVelocity, bool isDrifting)
    {
        if (engineSource != null)
        {
            float engineSoundPitch = startEngineSourcePitch + (Mathf.Abs(carVelocity) / 25f);
            engineSource.pitch = engineSoundPitch;
        }
        if (isDrifting)
        {
            if (!tireSource.isPlaying)
            {
                tireSource.Play();
            }
        }
        else if (!isDrifting)
        {
            tireSource.Stop();
        }
    }

    private void OnDestroy()
    {
        engineSource.Stop();
        tireSource.Stop();
    }
}
