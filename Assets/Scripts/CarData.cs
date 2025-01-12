using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CarData
{
    public int index;
    public string color;
    public string spoiler;
    public string carName;

    public CarData(int index, string color, string spoiler, string carName)
    {
        this.index = index;
        this.color = color;
        this.spoiler = spoiler;
        this.carName = carName;
    }
}

public static class CarDataManager
{
    private static readonly string FileName = "CarData.json";

    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void SaveCarData(List<CarData> carDatas)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(new CarDataWrapper(carDatas), true);
            File.WriteAllText(FilePath, jsonData);
            Debug.Log($"Car data saved successfully to: {FilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving car data: {ex.Message}");
        }
    }

    public static List<CarData> LoadCarData()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning($"No saved car data found at: {FilePath}");
            return new List<CarData>();
        }

        try
        {
            string jsonData = File.ReadAllText(FilePath);
            CarDataWrapper wrapper = JsonUtility.FromJson<CarDataWrapper>(jsonData);
            Debug.Log("Car data loaded successfully.");
            return wrapper.carDatas;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading car data: {ex.Message}");
            return new List<CarData>();
        }
    }

    [Serializable]
    private class CarDataWrapper
    {
        public List<CarData> carDatas;

        public CarDataWrapper(List<CarData> carDatas)
        {
            this.carDatas = carDatas;
        }
    }
}
