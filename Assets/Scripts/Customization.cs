using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DefaultCarData
{
    public string name;

    public Sprite icon;

    public CarPrice carPrice;

    public CarColor[] colors;

    public CarSpoiler[] spoilers;

    public DefaultCarData(string name, Sprite icon, CarPrice carPrice, CarColor[] colors, CarSpoiler[] spoilers)
    {
        this.name = name;
        this.icon = icon;
        this.colors = colors;
        this.spoilers = spoilers;
        this.carPrice = carPrice;
    }
}

public class Customization : MonoBehaviour
{
    private enum CustomizationPanel
    {
        Main,
        Car,
        Color,
        Spoiler
    }

    [Header("References")]
    [Space(5)]
    [SerializeField] private GameObject selectButtonPrefab;
    [SerializeField] private TextMeshProUGUI moneyText;

    [Space(5)]
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject carPanel;
    [SerializeField] private GameObject colorPanel;
    [SerializeField] private GameObject spoilerPanel;

    [Space(5)]
    [Header("Cars")]
    [SerializeField] private GameObject[] cars;
    [SerializeField] private Transform carButtonContainer;
    [SerializeField] private Transform colorButttonContainer;
    [SerializeField] private Transform spoilerButtonContainer;

    private List<DefaultCarData> defaultCarDatas = new List<DefaultCarData>();

    private int selectedCarIndex = 0;

    private List<CarData> carDatas = new List<CarData>();

    private List<CustomizationData> customizationDatas = new List<CustomizationData>();
    private List<CustomizationData> carCustomizationDatas = new List<CustomizationData>();
    private List<CustomizationData> colorCustomizationDatas = new List<CustomizationData>();
    private List<CustomizationData> spoilerCustomizationDatas = new List<CustomizationData>();

    private void Start()
    {
        selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);

        AddDatas();

        carDatas = CarDataManager.LoadCarData();

        if (carDatas.Count == 0)
        {
            int i = 0;
            foreach (DefaultCarData defaultCarData in defaultCarDatas)
            {
                carDatas.Add(new CarData(i, defaultCarData.colors[0].name, defaultCarDatas[0].spoilers[0].spoilerName, defaultCarData.name));
                SetUpCars(i, defaultCarDatas[i].colors[0], defaultCarDatas[i].spoilers[0]);
                i++;
            }

            CarDataManager.SaveCarData(carDatas);
        }
        else
        {
            for (int i = 0; i < carDatas.Count; i++)
            {
                int index = carDatas[i].index;
                CarColor selectedColor = Array.Find(defaultCarDatas[index].colors, color => color.name == carDatas[i].color);
                CarSpoiler selectedSpoiler = Array.Find(defaultCarDatas[index].spoilers, spoiler => spoiler.spoilerName == carDatas[i].spoiler);
                SetUpCars(index, selectedColor, selectedSpoiler);
            }
        }

        customizationDatas = CustomizationDataManager.LoadCustomizationData();

        if(customizationDatas.Count == 0)
        {         
            for (int i = 0; i < defaultCarDatas.Count; i++)
            {
                //Add Cars
                customizationDatas.Add(new CustomizationData(defaultCarDatas[i].name, i == 0, i == 0, defaultCarDatas[i].carPrice.price));

                //Add Colors
                for (int j = 0; j < defaultCarDatas[i].colors.Length; j++)
                {
                    customizationDatas.Add(new CustomizationData($"{defaultCarDatas[i].name}/{defaultCarDatas[i].colors[j].name}", j == 0, j == 0, defaultCarDatas[i].colors[j].price));
                }

                //Add Spoilers
                for (int k = 0; k < defaultCarDatas[i].spoilers.Length; k++)
                {
                    customizationDatas.Add(new CustomizationData($"{defaultCarDatas[i].name}/{defaultCarDatas[i].spoilers[k].spoilerName}", k == 0, k == 0, defaultCarDatas[i].spoilers[k].price));
                }
            }

            CustomizationDataManager.SaveCustomizationData(customizationDatas);
        }

        UpdateMoneyText();

        ShowMainPanel();
     
        UpdateCar();

        UpdateCarButtons();

        UpdateColorButtons();

        UpdateSpoilerButtons();
    }

    private void OnEnable()
    {
        UpdateMoneyText();
    }

    private void AddDatas()
    {
        defaultCarDatas.Clear();

        UnityEngine.Object[] cars = Resources.LoadAll("Cars");

        for (int i = 0; i < cars.Length; i++)
        {
            Sprite carIcon = Resources.Load<Sprite>($"CarData/{cars[i].name}/Icon");

            CarPrice carPrice = Resources.Load<CarPrice>($"CarData/{cars[i].name}/CarPrice");

            CarColor[] carColors = Resources.LoadAll<CarColor>($"CarData/{cars[i].name}/Colors")
                .OrderBy(color => color.price)
                .ToArray();

            CarSpoiler[] carSpoilers = Resources.LoadAll<CarSpoiler>($"CarData/{cars[i].name}/Spoilers")
                .OrderBy(spoiler => spoiler.price)
                .ToArray();

            DefaultCarData defaultCarData = new DefaultCarData(
                cars[i].name,
                carIcon,
                carPrice,
                carColors,
                carSpoilers
            );
            defaultCarDatas.Add(defaultCarData);
        }
    }

    private void UpdateCarButtons()
    {
        foreach (Transform t in carButtonContainer)
        {
            Destroy(t.gameObject);
        }

        carCustomizationDatas.Clear();

        for (int i = 0; i < defaultCarDatas.Count; i++)
        {         
            GameObject selectCarObj = Instantiate(selectButtonPrefab, carButtonContainer);
            SelectButton selectButton = selectCarObj.GetComponent<SelectButton>();
            CustomizationData data = GetCustomizationData($"{defaultCarDatas[i].name}");
            carCustomizationDatas.Add(data);
            selectButton.Setup(data, defaultCarDatas[i].icon, defaultCarDatas[i].name);
            int selectedIndex = i;
            selectButton.button.onClick.AddListener(() =>
            {
                OnCarSelectButtonClick(selectedIndex, data);
            });
        }       
    }

    private void OnCarSelectButtonClick(int selectedIndex, CustomizationData customizationData)
    {
        if (customizationData.isSelected)
            return;

        if (customizationData.isUnlocked)
        {
            UpdateCustomizationDataSelection(customizationData, carCustomizationDatas);
            selectedCarIndex = selectedIndex;
            PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
            PlayerPrefs.Save();

            UpdateCar();
        }
        else
        {
            int money = Money.GetMoney();
            if (money >= customizationData.price)
            {
                Money.AddMoney(-customizationData.price);
                UpdateMoneyText();

                customizationData.isUnlocked = true;

                UpdateCustomizationDataUnlocked(customizationData, carCustomizationDatas);
            }
            else
            {
                return;
            }
        }

        CustomizationDataManager.SaveCustomizationData(customizationDatas);

        UpdateCarButtons();
        UpdateColorButtons();
        UpdateSpoilerButtons();
    }

    private void UpdateColorButtons()
    {
        foreach (Transform t in colorButttonContainer)
        {
            Destroy(t.gameObject);
        }

        colorCustomizationDatas.Clear();

        for (int i = 0; i < defaultCarDatas[selectedCarIndex].colors.Length; i++)
        {
            GameObject selectColorObj = Instantiate(selectButtonPrefab, colorButttonContainer);
            SelectButton selectButton = selectColorObj.GetComponent<SelectButton>();
            CustomizationData data = GetCustomizationData($"{defaultCarDatas[selectedCarIndex].name}/{defaultCarDatas[selectedCarIndex].colors[i].name}");
            colorCustomizationDatas.Add(data);
            selectButton.Setup(data, defaultCarDatas[selectedCarIndex].colors[i].sprite, defaultCarDatas[selectedCarIndex].colors[i].name);
            int selectedIndex = i;
            selectButton.button.onClick.AddListener(() =>
            {
                OnColorSelectButtonClick(selectedIndex, data);
            });
        }
    }

    private void OnColorSelectButtonClick(int selectedIndex, CustomizationData customizationData)
    {
        if (customizationData.isSelected)
            return;

        if (customizationData.isUnlocked)
        {
            UpdateCustomizationDataSelection(customizationData, colorCustomizationDatas);
            CarColor selectedCarColor = defaultCarDatas[selectedCarIndex].colors[selectedIndex];
            UpdateMaterial(selectedCarColor);
        }
        else
        {
            int money = Money.GetMoney();
            if (money >= customizationData.price)
            {
                Money.AddMoney(-customizationData.price);
                UpdateMoneyText();

                customizationData.isUnlocked = true;

                UpdateCustomizationDataUnlocked(customizationData, colorCustomizationDatas);
            }
            else
            {
                return;
            }
        }

        CustomizationDataManager.SaveCustomizationData(customizationDatas);

        UpdateColorButtons();
    }

    public void UpdateMaterial(CarColor carColor)
    {
        MeshRenderer meshRenderer = cars[selectedCarIndex].transform.Find("Body").GetComponent<MeshRenderer>();
        if (meshRenderer.material == carColor.material)
            return;
        meshRenderer.material = carColor.material;
        CarData selectedCarData = carDatas.Find(car => car.index == PlayerPrefs.GetInt("SelectedCarIndex", 0));
        selectedCarData.color = carColor.name;
        carDatas[PlayerPrefs.GetInt("SelectedCarIndex", 0)] = selectedCarData;
        CarDataManager.SaveCarData(carDatas);
    }

    private void UpdateSpoilerButtons()
    {
        foreach (Transform t in spoilerButtonContainer)
        {
            Destroy(t.gameObject);
        }

        spoilerCustomizationDatas.Clear();

        for (int i = 0; i < defaultCarDatas[selectedCarIndex].spoilers.Length; i++)
        {
            GameObject selectSSpoilerObj = Instantiate(selectButtonPrefab, spoilerButtonContainer);

            SelectButton selectButton = selectSSpoilerObj.GetComponent<SelectButton>();
            CustomizationData data = GetCustomizationData($"{defaultCarDatas[selectedCarIndex].name}/{defaultCarDatas[selectedCarIndex].spoilers[i].spoilerName}");
            spoilerCustomizationDatas.Add(data);
            selectButton.Setup(data, defaultCarDatas[selectedCarIndex].spoilers[i].icon, defaultCarDatas[selectedCarIndex].spoilers[i].spoilerName);
            int selectedIndex = i;
            selectButton.button.onClick.AddListener(() =>
            {
                OnSpoilerSelectButtonClick(selectedIndex, data);
            });
        }
    }

    private void OnSpoilerSelectButtonClick(int selectedIndex, CustomizationData customizationData)
    {
        if (customizationData.isSelected)
            return;

        if (customizationData.isUnlocked)
        {
            UpdateCustomizationDataSelection(customizationData, spoilerCustomizationDatas);

            CarSpoiler selectedCarSpoiler = defaultCarDatas[selectedCarIndex].spoilers[selectedIndex];
            UpdateSpoiler(selectedCarSpoiler);
        }
        else
        {
            int money = Money.GetMoney();
            if (money >= customizationData.price)
            {
                Money.AddMoney(-customizationData.price);
                UpdateMoneyText();

                customizationData.isUnlocked = true;

                UpdateCustomizationDataUnlocked(customizationData, spoilerCustomizationDatas);
            }
            else
            {
                return;
            }
        }

        CustomizationDataManager.SaveCustomizationData(customizationDatas);

        UpdateSpoilerButtons();
    }

    public void UpdateSpoiler(CarSpoiler carSpoiler)
    {
        MeshFilter meshFilter = cars[selectedCarIndex].transform.Find("Spoiler").GetComponent<MeshFilter>();

        if (meshFilter.mesh == carSpoiler.mesh)
            return;

        if (carSpoiler.spoilerName == "None")
        {
            Debug.Log("Selected none spoiler. Disable spoiler");
            meshFilter.gameObject.SetActive(false);
        }
        else
        {
            meshFilter.gameObject.SetActive(true);
            meshFilter.mesh = carSpoiler.mesh;
        }      
        CarData selectedCarData = carDatas.Find(car => car.index == PlayerPrefs.GetInt("SelectedCarIndex", 0));
        selectedCarData.spoiler = carSpoiler.spoilerName;
        carDatas[PlayerPrefs.GetInt("SelectedCarIndex", 0)] = selectedCarData;
        CarDataManager.SaveCarData(carDatas);
    }

    public void SetUpCars(int carIndex, CarColor carColor, CarSpoiler carSpoiler)
    {
        MeshRenderer meshRenderer = cars[carIndex].transform.Find("Body").GetComponent<MeshRenderer>();
        meshRenderer.material = carColor.material;

        Transform spoilerTransform = cars[carIndex].transform.Find("Spoiler");
        if (carSpoiler.spoilerName == "None")
        {
            spoilerTransform.gameObject.SetActive(false);
        }
        else
        {
            spoilerTransform.gameObject.SetActive(true);
            MeshFilter meshFilter = spoilerTransform.GetComponent<MeshFilter>();
            meshFilter.mesh = carSpoiler.mesh;
        }
    }

    public void UpdateCar()
    {
        for (int i = 0; i < cars.Length; i++)
        {
            if (i == selectedCarIndex)
            {
                cars[i].SetActive(true);
            }
            else
                cars[i].SetActive(false);
        }
    }

    private void UpdateCustomizationDataSelection(CustomizationData selectedCustomizationData, List<CustomizationData> currentCustomizationDatas)
    {
        foreach (CustomizationData carCustomizationData in currentCustomizationDatas)
        {
            if (carCustomizationData.name != selectedCustomizationData.name)
            {
                carCustomizationData.isSelected = false;
            }
            else
            {
                carCustomizationData.isSelected = true;
            }

            CustomizationData mainCustomizationData = customizationDatas.Find(data => data.name == carCustomizationData.name);
            if (mainCustomizationData != null)
            {
                mainCustomizationData.isSelected = carCustomizationData.isSelected;
            }
        }
    }

    private void UpdateCustomizationDataUnlocked(CustomizationData unlockedCustomizationData, List<CustomizationData> currentCustomizationDatas)
    {
        foreach (CustomizationData carCustomizationData in currentCustomizationDatas)
        {
            if (carCustomizationData.name == unlockedCustomizationData.name)
            {
                carCustomizationData.isUnlocked = true;
            }

            CustomizationData mainCustomizationData = customizationDatas.Find(data => data.name == carCustomizationData.name);
            if (mainCustomizationData != null)
            {
                mainCustomizationData.isUnlocked = carCustomizationData.isUnlocked;
            }
        }
    }

    private CustomizationData GetCustomizationData(string name)
    {
        CustomizationData data = customizationDatas.Find(data => data.name == name);
        if (data == null)
        {
            Debug.LogError($"{name} notFounded");
        }
        return data;
    }

    private void UpdateMoneyText()
    {
        moneyText.text = Money.GetMoney().ToString();
    }

    private void ShowPanel(CustomizationPanel panel)
    {
        mainPanel.SetActive(panel == CustomizationPanel.Main);
        carPanel.SetActive(panel == CustomizationPanel.Car);
        colorPanel.SetActive(panel == CustomizationPanel.Color);
        spoilerPanel.SetActive(panel == CustomizationPanel.Spoiler);
    }

    public void ShowMainPanel()
    {
        ShowPanel(CustomizationPanel.Main);
    }

    public void ShowCarPanel()
    {
        ShowPanel(CustomizationPanel.Car);
    }

    public void ShowColorPanel()
    {
        ShowPanel(CustomizationPanel.Color);
    }

    public void ShowSpoilerPanel()
    {
        ShowPanel(CustomizationPanel.Spoiler);
    }

    
}
