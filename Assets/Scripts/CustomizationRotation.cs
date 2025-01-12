using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationCamera : MonoBehaviour
{
    [Header("References")]
    [Space(5)]
    [SerializeField] private Transform carTransform;

    [Space(5)]
    [Header("Camera Settings")]
    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float zoomSpeed = 0.05f;

    private float distance;
    private float currentYaw = 0f;
    private float currentPitch = 20f;

    private Vector2 previousTouchPosition;
    private bool isRotating = false;

    private float previousPinchDistance = 0f;

    private bool isEnabled = false;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        distance = (minDistance + maxDistance) / 2f;
    }

    private void Update()
    {
        if (isEnabled)
        {
            HandleCameraInput();
            UpdateCameraPosition();
        }
    }

    private void HandleCameraInput()
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            HandleMouseInput();
        }
        else
        {
            HandleTouchInput();
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            previousTouchPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = (Vector2)Input.mousePosition - previousTouchPosition;
            previousTouchPosition = Input.mousePosition;

            currentYaw += mouseDelta.x * rotationSpeed * Time.deltaTime * 100f;
            currentPitch -= mouseDelta.y * rotationSpeed * Time.deltaTime * 100f;
            currentPitch = Mathf.Clamp(currentPitch, 10f, 80f);
        }

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            distance -= scrollInput * zoomSpeed * 100f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPosition = touch.position;
                isRotating = true;
            }

            if (touch.phase == TouchPhase.Moved && isRotating)
            {
                Vector2 touchDelta = touch.deltaPosition;
                currentYaw += touchDelta.x * rotationSpeed;
                currentPitch -= touchDelta.y * rotationSpeed;
                currentPitch = Mathf.Clamp(currentPitch, 10f, 80f);
            }

            if (touch.phase == TouchPhase.Ended)
            {
                isRotating = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);

            if (previousPinchDistance == 0f)
            {
                previousPinchDistance = currentPinchDistance;
            }

            float pinchDelta = currentPinchDistance - previousPinchDistance;
            distance -= pinchDelta * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            previousPinchDistance = currentPinchDistance;

            if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
            {
                previousPinchDistance = 0f;
            }
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 offset = new Vector3(
            Mathf.Sin(currentYaw * Mathf.Deg2Rad) * Mathf.Cos(currentPitch * Mathf.Deg2Rad),
            Mathf.Sin(currentPitch * Mathf.Deg2Rad),
            Mathf.Cos(currentYaw * Mathf.Deg2Rad) * Mathf.Cos(currentPitch * Mathf.Deg2Rad)
        ) * distance;

        transform.position = carTransform.position + offset;
        transform.LookAt(carTransform.position);
    }

    private void OnEnable()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        isEnabled = true;
    }

    private void OnDisable()
    {
        isEnabled = false;
    }
}
