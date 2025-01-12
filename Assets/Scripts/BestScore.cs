using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BestScore
{
    public static readonly string BestScoreKey = "BestScore";

    public static int GetBestScore() => PlayerPrefs.GetInt(BestScoreKey, 0);

    public static void SetBestScore(int value) => PlayerPrefs.SetInt(BestScoreKey, value);
}
