using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectButton : MonoBehaviour
{
    [SerializeField] private GameObject isSelected;
    [SerializeField] private GameObject price;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI priceText;
    public Button button;

    public void Setup(CustomizationData data, Sprite sprite, string itemName)
    {
        icon.sprite = sprite;
        nameText.text = itemName;
        priceText.text = data.price.ToString();

        isSelected.SetActive(false);

        if (data.isUnlocked)
        {
            price.SetActive(false);
            if (data.isSelected)
            {
                isSelected.SetActive(true);
            }
        }
        else
        {
            price.SetActive(true);
        }   
    }
}
