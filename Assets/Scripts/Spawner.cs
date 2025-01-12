using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviourPunCallbacks
{
    [Space(5)]
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Space(5)]
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;

    private bool isMultiplayer = false;

    private GameObject playerPrefab;
    private CarColor selectedCarColor;
    private CarSpoiler selectedCarSpoiler;

    void Awake()
    {
        LoadPlayerData();

        isMultiplayer = PlayerPrefs.GetInt("IsMultiplayer", 0) == 1;
        Debug.Log($"Multiplayer: {isMultiplayer}");

        loadingScreen.SetActive(true);

        if (isMultiplayer)
        {
            JoinRoom();
        }
        else
        {
            SpawnCar();
        }
    }

    private void JoinRoom()
    {
        string roomName = PlayerPrefs.GetString("RoomToJoin", string.Empty);
        if (roomName != string.Empty && PhotonNetwork.IsConnected && !PhotonNetwork.OfflineMode)
            PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
        else
        {
            PlayerPrefs.SetInt("IsMultiplayer", 0);
            PlayerPrefs.Save();
            PhotonNetwork.OfflineMode = true;
            SpawnCar();
            Debug.LogWarning("Cannot join, entering to offlineMode");
        }

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        SpawnMultiplayerCar();
    }

    private void SpawnMultiplayerCar()
    {
        loadingScreen.SetActive(false);

        GameObject _player = PhotonNetwork.Instantiate($"Cars/{playerPrefab.name}", spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].position, Quaternion.Euler(0f, 90f, 0f));

        CarSetup carSetup = _player.GetComponent<CarSetup>();
        carSetup.SetupCar(selectedCarColor, selectedCarSpoiler, isMultiplayer);

        Debug.Log($"Player spawned: {PhotonNetwork.LocalPlayer.NickName}");
        Debug.Log($"Total players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");
        Debug.Log($"Room name: {PhotonNetwork.CurrentRoom.Name}");
    }


    private void SpawnCar()
    {
        loadingScreen.SetActive(false);

        GameObject _player = Instantiate(playerPrefab, spawnPoints[0].position, Quaternion.Euler(0f, 90f, 0f));
        CarSetup carSetup = _player.GetComponent<CarSetup>();
        carSetup.SetupCar(selectedCarColor, selectedCarSpoiler, isMultiplayer);
    }

    private void LoadPlayerData()
    {
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        CarData carData = CarDataManager.LoadCarData().Find(data => data.index == selectedCarIndex);

        if (carData == null)
        {
            Debug.LogError("CarData not found!");
            return;
        }

        string carPath = $"CarData/{carData.carName}";
        selectedCarColor = Array.Find(Resources.LoadAll<CarColor>($"{carPath}/Colors"), c => c.name == carData.color);
        selectedCarSpoiler = Array.Find(Resources.LoadAll<CarSpoiler>($"{carPath}/Spoilers"), s => s.spoilerName == carData.spoiler);

        playerPrefab = Resources.Load<GameObject>($"Cars/{carData.carName}");
    }
}