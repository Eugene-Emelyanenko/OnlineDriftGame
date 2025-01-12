using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomList : MonoBehaviourPunCallbacks
{
    [Header("Rooms Initialization")]
    [Space(5)]
    [SerializeField] private Transform roomsContainer;
    [SerializeField] private GameObject roomItemPrefab;

    [Space(5)]
    [Header("UI")]
    [SerializeField] private Button createRoomButton;

    [Space(5)]
    [Header("Menu Manager")]
    [SerializeField] private MenuManager menuManager;

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    private string newRoomName = string.Empty;

    void Start()
    {
        menuManager.ShowPanel(MenuPanel.Loading);

        ChangeRoomToJoin(string.Empty);

        if (IsInternetAvailable())
        {
            Debug.Log("Internet available. Trying to connect to Photon...");
            EnterOnlineMode();
        }
        else
        {
            Debug.Log("No internet connection. Switching to offline mode.");
            EnterOfflineMode();
        }
    }
    private bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogWarning($"Disconnected from Photon. Cause: {cause}. Switching to offline mode.");
        EnterOfflineMode();
    }

    private void EnterOfflineMode()
    {
        Debug.Log("Cannot connect to server. Entering offline mode.");
        PhotonNetwork.OfflineMode = true;
        Debug.Log("Offline mode is on");
        menuManager.ShowPanel(MenuPanel.Menu);
    }

    private void EnterOnlineMode()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        PhotonNetwork.OfflineMode = false;
        Debug.Log("Connecting...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        if (PhotonNetwork.OfflineMode)
        {
            Debug.Log("Offline mode is enabled. Skipping lobby connection.");
            return;
        }

        Debug.Log("Connected to server");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        menuManager.ShowPanel(MenuPanel.Menu);

        Debug.Log("Connected to lobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        if(cachedRoomList.Count <= 0)
        {
            cachedRoomList = roomList;
        }
        else
        {
            foreach (RoomInfo room in roomList)
            {
                for (int i = 0; i < cachedRoomList.Count; i++)
                {
                    if (cachedRoomList[i].Name == room.Name)
                    {
                        List<RoomInfo> newList = cachedRoomList;

                        if(room.RemovedFromList)
                        {
                            newList.Remove(newList[i]);
                        }
                        else
                        {
                            newList[i] = room;
                        }

                        cachedRoomList = newList;
                    }
                }
            }
        }

        UpdateRoomList();
    }

    private void UpdateRoomList()
    {
        foreach (Transform t in roomsContainer)
        {
            Destroy(t.gameObject);
        }

        foreach (RoomInfo room in cachedRoomList)
        {
            GameObject roomItem = Instantiate(roomItemPrefab, roomsContainer);

            TextMeshProUGUI roomNameText = roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            roomNameText.text = room.Name;

            TextMeshProUGUI roomPlayersCountText = roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            roomPlayersCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";

            Button roomButton = roomItem.GetComponent<Button>();
            roomButton.onClick.RemoveAllListeners();
            roomButton.onClick.AddListener(() =>
            {
                JoinRoom(room.Name);
            });
        }
    }

    private void JoinRoom(string roomName)
    {
        newRoomName = string.Empty;
        ChangeRoomToJoin(roomName);
        PlayerPrefs.SetInt("IsMultiplayer", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Track");
    }

    private void ChangeRoomToJoin(string roomName)
    {
        PlayerPrefs.SetString("RoomToJoin", roomName);
        PlayerPrefs.Save();
    }

    public void SetRoomName(string str)
    {
        if (PhotonNetwork.OfflineMode)
        {
            Debug.LogWarning("Cannot create rooms in offline mode.");
            createRoomButton.interactable = false;
            return;
        }

        string trimmedStr = str.Replace(" ", string.Empty);

        if (!string.IsNullOrEmpty(trimmedStr))
        {
            createRoomButton.interactable = true;
            newRoomName = trimmedStr;
        }
        else
        {
            createRoomButton.interactable = false;
        }
    }

    public void CreateRoom()
    {
        JoinRoom(newRoomName);
    }
}
