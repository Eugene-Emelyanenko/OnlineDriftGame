using Photon.Pun;
using System;
using UnityEngine;

public class CarSetup : MonoBehaviour
{
    private CarController carController;
    private GameObject carCamera;
    private MeshRenderer bodyRenderer;
    private MeshFilter spoilerMeshFilter;

    private PhotonView photonView;
    private PhotonRigidbodyView photonRigidbodyView;
    private PhotonTransformView photonTransformView;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        carCamera = transform.Find("CarCamera ----DISABLE----").gameObject;
        bodyRenderer = transform.Find("Body").GetComponent<MeshRenderer>();
        spoilerMeshFilter = transform.Find("Spoiler").GetComponent<MeshFilter>();

        photonView = GetComponent<PhotonView>();
        photonRigidbodyView = GetComponent<PhotonRigidbodyView>();
        photonTransformView = GetComponent<PhotonTransformView>();
    }

    public void SetupCar(CarColor carColor, CarSpoiler carSpoiler, bool isMultiplayer)
    {
        DisableMultiplayerComponentsIfNeeded(isMultiplayer);

        if (photonView.IsMine || !isMultiplayer)
        {
            ApplyCarSetup(carColor, carSpoiler);

            if (isMultiplayer)
            {
                int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
                photonView.RPC(nameof(RPC_ApplyCarSetup), RpcTarget.OthersBuffered, carColor.name, carSpoiler.spoilerName, selectedCarIndex);
            }
        }

        carController.enabled = true;
        carCamera.SetActive(true);
    }

    private void DisableMultiplayerComponentsIfNeeded(bool isMultiplayer)
    {
        if (!isMultiplayer)
        {
            if (photonView != null) photonView.enabled = false;
            if (photonRigidbodyView != null) photonRigidbodyView.enabled = false;
            if (photonTransformView != null) photonTransformView.enabled = false;
        }
    }

    private void ApplyCarSetup(CarColor carColor, CarSpoiler carSpoiler)
    {
        bodyRenderer.material = carColor.material;

        if (carSpoiler.spoilerName == "None")
        {
            spoilerMeshFilter.gameObject.SetActive(false);
        }
        else
        {
            spoilerMeshFilter.gameObject.SetActive(true);
            spoilerMeshFilter.mesh = carSpoiler.mesh;
        }
    }

    [PunRPC]
    private void RPC_ApplyCarSetup(string carColorName, string carSpoilerName, int selectedCarIndex)
    {
        CarData carData = GetCarData(selectedCarIndex);

        if (carData == null)
        {
            Debug.LogError($"Car data null for index {selectedCarIndex}");
            return;
        }

        CarColor carColor = LoadCarColor(carData.carName, carColorName);
        CarSpoiler carSpoiler = LoadCarSpoiler(carData.carName, carSpoilerName);

        if (carColor != null && carSpoiler != null)
        {
            ApplyCarSetup(carColor, carSpoiler);
        }
        else
        {
            Debug.LogError("Car color or spoiler null");
        }
    }

    private CarData GetCarData(int selectedCarIndex)
    {
        return CarDataManager.LoadCarData().Find(data => data.index == selectedCarIndex);
    }

    private CarColor LoadCarColor(string carName, string colorName)
    {
        var colors = Resources.LoadAll<CarColor>($"CarData/{carName}/Colors");
        CarColor color = Array.Find(colors, color => color.name == colorName);
        return color;
    }

    private CarSpoiler LoadCarSpoiler(string carName, string spoilerName)
    {
        var spoilers = Resources.LoadAll<CarSpoiler>($"CarData/{carName}/Spoilers");
        CarSpoiler spoiler = Array.Find(spoilers, spoiler => spoiler.spoilerName == spoilerName);
        return spoiler;
    }
}
