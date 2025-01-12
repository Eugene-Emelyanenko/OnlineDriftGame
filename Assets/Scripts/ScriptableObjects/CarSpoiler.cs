using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarSpoiler", menuName = "ScriptableObjects/New CarSpoiler", order = 1)]
public class CarSpoiler : ScriptableObject
{
    public string spoilerName;
    public Mesh mesh;
    public Sprite icon;
    public int price;
}
