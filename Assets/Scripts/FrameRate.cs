using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRate : MonoBehaviour
{
    public int targetFrameRate = 120;

    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
