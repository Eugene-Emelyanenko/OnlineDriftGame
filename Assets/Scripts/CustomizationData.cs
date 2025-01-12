using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CustomizationData
{
    public string name;
    public bool isSelected;
    public bool isUnlocked;
    public int price;

    public CustomizationData(string name, bool isSelected, bool isUnlocked, int price)
    {
        this.name = name;
        this.isSelected = isSelected;
        this.isUnlocked = isUnlocked;
        this.price = price;
    }
}

public static class CustomizationDataManager
{
    private static readonly string FileName = "CustomizationData.json";

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void SaveCustomizationData(List<CustomizationData> carDatas)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(new CustomizationDataWrapper(carDatas), true);
            File.WriteAllText(FilePath, jsonData);
            Debug.Log($"Customization data saved successfully to: {FilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving customization data: {ex.Message}");
        }
    }

    public static List<CustomizationData> LoadCustomizationData()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning($"No saved customization data found at: {FilePath}");
            return new List<CustomizationData>();
        }

        try
        {
            string jsonData = File.ReadAllText(FilePath);
            CustomizationDataWrapper wrapper = JsonUtility.FromJson<CustomizationDataWrapper>(jsonData);
            Debug.Log("Customization data loaded successfully.");
            return wrapper.customizationDatas;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading customization data: {ex.Message}");
            return new List<CustomizationData>();
        }
    }

    [Serializable]
    private class CustomizationDataWrapper
    {
        public List<CustomizationData> customizationDatas;

        public CustomizationDataWrapper(List<CustomizationData> customizationDatas)
        {
            this.customizationDatas = customizationDatas;
        }
    }
}
