using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public void PlayClickSound()
    {
        AudioManager.Instance.PlaySFX("Click");
    }
}
