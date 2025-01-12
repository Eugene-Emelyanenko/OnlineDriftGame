using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum MenuPanel
{
    Menu,
    Loading,
    Multiplayer,
    Settings,
    Customization,
}

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [Space(5)]
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject customizationPanel;

    [Space(5)]
    [Header("Menu Best Score")]
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Space(5)]
    [Header("Menu Money")]
    [SerializeField] private TextMeshProUGUI moneyText;

    private AdsManager adsManager;

    private void Awake()
    {
        adsManager = FindObjectOfType<AdsManager>();
    }

    public void ShowPanel(MenuPanel panel)
    {
        if (menuPanel == null || loadingScreen == null || multiplayerPanel == null || settingsPanel == null || customizationPanel == null)
            return;

        menuPanel.SetActive(panel == MenuPanel.Menu);
        loadingScreen.SetActive(panel == MenuPanel.Loading);
        multiplayerPanel.SetActive(panel == MenuPanel.Multiplayer);
        settingsPanel.SetActive(panel == MenuPanel.Settings);
        customizationPanel.SetActive(panel == MenuPanel.Customization);
        UpdateUI();
    }

    public void ShowMenuPanel()
    {
        ShowPanel(MenuPanel.Menu);
    }

    public void ShowSettingsPanel()
    {
        ShowPanel(MenuPanel.Settings);
    }

    public void ShowMultiplayerPanel()
    {
        ShowPanel(MenuPanel.Multiplayer);
    }

    public void ShowCustomizationPanel()
    {
        ShowPanel(MenuPanel.Customization);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void TestAddMoney(int value)
    {
        Money.AddMoney(value);
        UpdateUI();
    }

    public void TestShowRewarded(int rewardMoney)
    {
        adsManager.ShowRewarded(() =>
        {            
            Money.AddMoney(rewardMoney);
            UpdateUI();
        });
    }

    public void Play()
    {
        PlayerPrefs.SetInt("IsMultiplayer", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Track");
    }

    private void UpdateUI()
    {
        bestScoreText.text = BestScore.GetBestScore().ToString();
        moneyText.text = Money.GetMoney().ToString();
    }
}
