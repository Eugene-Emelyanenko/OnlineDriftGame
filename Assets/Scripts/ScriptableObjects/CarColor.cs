using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarColor", menuName = "ScriptableObjects/New CarColor", order = 1)]
public class CarColor : ScriptableObject
{
    public Sprite sprite;
    public Material material;
    public int price;
}
