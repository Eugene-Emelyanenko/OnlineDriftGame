using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CarUI : MonoBehaviourPunCallbacks
{
    private enum CarPanel
    {
        None,
        Pause,
        Settiing,
        Gameover
    }

    [Header("Timer")]
    [Space(5)]
    [SerializeField] private TextMeshProUGUI timerText;

    [Space(5)]
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private float fadeTime;

    [Space(5)]
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject buttonsPanel;
 
    [Space(5)]
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverBestScoreText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private GameObject doubleReward;
    [SerializeField] private Button doubleRewardButton;

    [Space(5)]
    [Header("Speedometer")]
    [SerializeField] private Image speedometer;
    [SerializeField] private TextMeshProUGUI speedText;

    private AdsManager adsManager;

    private bool isRewardDoubled = false;

    public bool IsPaused { get; private set; }

    private bool isGameOver = false;

    private int reward = 0;

    private void Awake()
    {
        adsManager = FindObjectOfType<AdsManager>();
    }

    private void Start()
    {
        Color scoreTextColor = currentScoreText.color;
        scoreTextColor.a = 0f;
        currentScoreText.color = scoreTextColor;

        doubleRewardButton.interactable = true;
        doubleRewardButton.onClick.AddListener(DoubleReward);

        Unpause();
    }

    private void Update()
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer && !isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
            {
                if (IsPaused)
                    Unpause();
                else
                    Pause();
            }
        }
    }

    private void ShowPanel(CarPanel panel)
    {
        if(panel == CarPanel.None)
        {
            pausePanel.SetActive(false);
            settingsPanel.SetActive(false);
            gameOverPanel.SetActive(false);
        }
        pausePanel.SetActive(panel == CarPanel.Pause);
        settingsPanel.SetActive(panel == CarPanel.Settiing);
        gameOverPanel.SetActive(panel == CarPanel.Gameover);
    }

    public void ShowPausePanel()
    {
        ShowPanel(CarPanel.Pause);
    }

    public void ShowSettingsPanel()
    {
        ShowPanel(CarPanel.Settiing);
    }

    public void ClosePanels()
    {
        ShowPanel(CarPanel.None);
    }

    public void Pause()
    {
        ShowPausePanel();
        currentScoreText.gameObject.SetActive(false);
        speedometer.transform.parent.gameObject.SetActive(false);

        buttonsPanel.SetActive(false);
        IsPaused = true;
    }

    public void Unpause()
    {
        ClosePanels();
        currentScoreText.gameObject.SetActive(true);
        speedometer.transform.parent.gameObject.SetActive(true);
        if(Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            buttonsPanel.SetActive(false);
        }
        else
        {
            buttonsPanel.SetActive(true);
        }
        
        IsPaused = false;
    }

    public void ShowGameOverPanel(int currentScore, int bestScore, int reward)
    {
        isGameOver = true;

        doubleReward.SetActive(false);

        ShowPanel(CarPanel.Gameover);

        totalScoreText.gameObject.SetActive(false);
        currentScoreText.gameObject.SetActive(true);
        speedometer.transform.parent.gameObject.SetActive(true);

        this.reward = reward;

        gameOverScoreText.text = currentScore.ToString();
        gameOverBestScoreText.text = bestScore.ToString();
        rewardText.text = $"+{reward}";
    }

    private void DoubleReward()
    {
        if(isRewardDoubled)
        {
            return;
        }

        adsManager.ShowRewarded(() =>
        {
            doubleRewardButton.interactable = false;
            doubleReward.SetActive(true);
            isRewardDoubled = true;
            rewardText.text = $"+{reward * 2}";
            Money.AddMoney(reward);
        });      
    }

    public void Exit()
    {
        bool isMultiplayer = PlayerPrefs.GetInt("IsMultiplayer", 0) == 1;

        if (isMultiplayer)
        {
            if (isMultiplayer)
            {
                StartCoroutine(LeaveRoom());
            }
        }
        else
        {
            SceneManager.LoadScene("Menu");
        }
    }

    public void UpdateSpeedometer(float currentSpeed, int maxSpeed)
    {
        float normalizedSpeed = currentSpeed / maxSpeed;
        speedometer.fillAmount = normalizedSpeed;
        speedText.text = $"{Mathf.RoundToInt(currentSpeed)}";
    }

    IEnumerator LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
            yield return null;
        SceneManager.LoadScene("Menu");
    }

    public void FadeScoreText(bool transparent)
    {
        currentScoreText.DOKill();

        float targetAlpha = transparent ? 0f : 1f;

        currentScoreText.DOColor(new Color(currentScoreText.color.r, currentScoreText.color.g, currentScoreText.color.b, targetAlpha), fadeTime)
            .SetEase(Ease.Linear);
    }

    public void UpdateTimerText(float currentTime)
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateCurrentScoreText(string str)
    {
        currentScoreText.text = str;
    }

    public void UpdateTotalScoreText(string str)
    {
        totalScoreText.text = str;
    }

    public void ChangeCurrentScoreTextColor(Color color)
    {
        currentScoreText.color = color;
    }
}
