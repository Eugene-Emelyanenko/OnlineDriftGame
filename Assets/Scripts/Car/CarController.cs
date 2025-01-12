using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public enum Axel
{
    Front, Rear
}

public enum Side
{
    Left, Right
}

[Serializable]
public struct WheelData
{
    public GameObject mesh;
    public WheelCollider collider;
    public bool isUsingEffects;
    public ParticleSystem particleSystem;
    public TrailRenderer trailRenderer;
    public Axel axel;
    public Side side;
}

[RequireComponent(typeof(Rigidbody), typeof(CarUI), typeof(CarSound))]
public class CarController : MonoBehaviour
{
    [Header("Wheels")]
    [Space(5)]
    [SerializeField] private List<WheelData> wheels = new List<WheelData>();

    [Space(5)]
    [Header("Speed")]
    [SerializeField] private int maxSpeed = 90;
    [SerializeField] private int maxReverseSpeed = 45;
    [SerializeField] private int accelerationMultiplier = 2;

    [Space(5)]
    [Header("Steering")]
    [SerializeField] private int maxSteeringAngle = 27;
    [SerializeField] private float steeringSpeed = 0.5f;
    [SerializeField] private float minDriftAngle = 4f;

    [Space(5)]
    [Header("Brakes")]
    [SerializeField] private int brakeForce = 350;
    [SerializeField] private int handbrakeDriftMultiplier = 5;

    [Space(5)]
    [Header("Body Mass Center")]
    [SerializeField] private Vector3 centerOfMass;

    [Space(5)]
    [Header("Score")]
    [SerializeField] private int scoreIncreaser;
    [SerializeField] private float scoreAddTime = 0.5f;

    [Space(5)]
    [Header("Timer")]
    [SerializeField] private float maxTime = 120f;
    private float currentTime = 0f;
    private bool timerIsRunning = false;

    private PhotonView photonView;

    private Rigidbody carRigidbody;

    private float steeringAxis;

    private float throttleAxis;

    private float driftingAxis;

    private float localVelocityZ;
    private float localVelocityX;

    private bool isDrifting = false;
    private bool wasDrifting = false;
    private bool hasCollided = false;

    private int currentScore = 0;
    private int totalScore = 0;

    private Dictionary<WheelCollider, WheelFrictionCurve> wheelFrictionCurves = new();

    private bool isGameOver = false;

    private CarSound carSound;

    private CarUI carUI;

    private bool isMultiplayer = false;

    private bool isLeftButtonPressed = false;
    private bool isRightButtonPressed = false;
    private bool isForwardButtonPressed = false;
    private bool isReverseButtonPressed = false;
    private bool isHandbrakeButtonPressed = false;
    private bool isHandbrakeButtonUp = false;

    private void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carUI = GetComponent<CarUI>();
        carSound = GetComponent<CarSound>();
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        isMultiplayer = PlayerPrefs.GetInt("IsMultiplayer", 0) == 1;

        carRigidbody.centerOfMass = centerOfMass;

        foreach (var wheel in wheels)
        {
            var frictionCurve = wheel.collider.sidewaysFriction;
            wheelFrictionCurves[wheel.collider] = new WheelFrictionCurve
            {
                extremumSlip = frictionCurve.extremumSlip,
                extremumValue = frictionCurve.extremumValue,
                asymptoteSlip = frictionCurve.asymptoteSlip,
                asymptoteValue = frictionCurve.asymptoteValue,
                stiffness = frictionCurve.stiffness
            };
        }

        ResetTimer();
        StartTimer();
    }

    private void Update()
    {
        if (!isGameOver)
            HandleInput();

        HandleDrifting();
        if (isMultiplayer)
            carSound.PhotonHandleSounds(carRigidbody.velocity.magnitude, isDrifting);
        else
            carSound.HandleSounds(carRigidbody.velocity.magnitude, isDrifting);
        UpdateCarSpeed();
        if (isMultiplayer)
            AnimateWheelMeshesRPC();
        else
            AnimateWheelMeshes();

        if (timerIsRunning)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= maxTime)
            {
                currentTime = maxTime;
                StopTimer();
                GameOver();
            }

            carUI.UpdateTimerText(currentTime);
        }
    }

    private void FixedUpdate()
    {
        carUI.UpdateSpeedometer(GetCarSpeed(), maxSpeed);
        AddScore();
    }

    private void HandleInput()
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            if (!carUI.IsPaused)
            {
                if (Input.GetKey(KeyCode.W)) GoForward();
                if (Input.GetKey(KeyCode.S)) GoReverse();

                if (Input.GetKey(KeyCode.A)) Turn(-1);
                if (Input.GetKey(KeyCode.D)) Turn(1);

                if (Input.GetKey(KeyCode.Space)) Handbrake();
                if (Input.GetKeyUp(KeyCode.Space)) RecoverTraction();
            }

            if (!Input.anyKey) ThrottleOff();
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) ResetSteering();
        }
        else
        {
            if (!carUI.IsPaused)
            {
                if (isForwardButtonPressed) GoForward();
                if (isReverseButtonPressed) GoReverse();

                if (isLeftButtonPressed) Turn(-1);
                if (isRightButtonPressed) Turn(1);

                if (isHandbrakeButtonPressed) Handbrake();
                if (isHandbrakeButtonUp)
                {
                    RecoverTraction();
                    isHandbrakeButtonUp = false;
                }
            }

            if (!isForwardButtonPressed && !isReverseButtonPressed && !isLeftButtonPressed && !isRightButtonPressed && !isHandbrakeButtonPressed) ThrottleOff();
            if (!isLeftButtonPressed && !isRightButtonPressed) ResetSteering();
        }      
    }

    public void ForwardButtonDown()
    {
        isForwardButtonPressed = true;
    }

    public void ForwardButtonUp()
    {
        isForwardButtonPressed = false;
    }

    public void ReverseButtonDown()
    {
        isReverseButtonPressed = true;
    }

    public void ReverseButtonUp()
    {
        isReverseButtonPressed = false;
    }

    public void LeftButtonDown()
    {
        isLeftButtonPressed = true;
    }

    public void LeftButtonUp()
    {
        isLeftButtonPressed = false;
    }

    public void RightButtonDown()
    {
        isRightButtonPressed = true;
    }

    public void RightButtonUp()
    {
        isRightButtonPressed = false;
    }

    public void HandbrakeButtonDown()
    {
        isHandbrakeButtonPressed = true;
    }

    public void HandbrakeButtonUp()
    {
        isHandbrakeButtonPressed = false;
        isHandbrakeButtonUp = true;
    }

    private void UpdateCarSpeed()
    {
        localVelocityX = transform.InverseTransformDirection(carRigidbody.velocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.velocity).z;
    }

    private void AnimateWheelMeshesRPC()
    {
        photonView.RPC("AnimateWheelMeshes", RpcTarget.All);
    }

    [PunRPC]
    public void AnimateWheelMeshes()
    {
        foreach (var wheel in wheels)
        {
            wheel.collider.GetWorldPose(out var position, out var rotation);
            wheel.mesh.transform.position = position;
            wheel.mesh.transform.rotation = rotation;
        }
    }

    public float GetCarSpeed()
    {
        return carRigidbody.velocity.magnitude * 3.6f;
    }

    private void GoForward()
    {
        throttleAxis = Mathf.Clamp(throttleAxis + (Time.deltaTime * 3f), 0f, 1f);

        if (localVelocityZ < -1f)
        {
            ApplyBrakes();
        }
        else
        {
            ApplyMotorTorque(GetCarSpeed(), maxSpeed);
        }
    }

    private void GoReverse()
    {
        throttleAxis = Mathf.Clamp(throttleAxis - (Time.deltaTime * 3f), -1f, 0f);

        if (localVelocityZ > 1f)
        {
            ApplyBrakes();
        }
        else
        {
            ApplyMotorTorque(Mathf.Abs(GetCarSpeed()), maxReverseSpeed);
        }
    }

    private void HandleDrifting()
    {
        if(isGameOver)
            return;

        isDrifting = Mathf.Abs(localVelocityX) > minDriftAngle;

        if (isDrifting && !wasDrifting)
        {
            OnDriftStart();
        }
        else if (!isDrifting && wasDrifting)
        {
            OnDriftEnd();
        }

        wasDrifting = isDrifting;

        UpdateParticleEffects();
    }

    private void ApplyMotorTorque(float currentSpeed, float maxAllowedSpeed)
    {
        if (Mathf.RoundToInt(currentSpeed) < maxAllowedSpeed)
        {
            SetWheelTorque((accelerationMultiplier * 50f) * throttleAxis);
        }
        else
        {
            SetWheelTorque(0f);
        }
    }

    private void SetWheelTorque(float torque)
    {
        foreach (WheelData wheel in wheels)
        {
            wheel.collider.brakeTorque = 0;
            wheel.collider.motorTorque = torque;
        }
    }

    private void ThrottleOff()
    {
        foreach (var wheel in wheels)
        {
            wheel.collider.motorTorque = 0;
        }
    }

    private void Turn(float direction)
    {
        steeringAxis = Mathf.Clamp(steeringAxis + direction * Time.deltaTime * 10f * steeringSpeed, -1f, 1f);
        var steeringAngle = steeringAxis * maxSteeringAngle;

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, steeringAngle, steeringSpeed);
            }
        }
    }

    private void ResetSteering()
    {
        steeringAxis = Mathf.MoveTowards(steeringAxis, 0, Time.deltaTime * 10f * steeringSpeed);
        var steeringAngle = steeringAxis * maxSteeringAngle;

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                wheel.collider.steerAngle = Mathf.Lerp(wheel.collider.steerAngle, steeringAngle, steeringSpeed);
            }
        }
    }

    private void ApplyBrakes()
    {
        foreach (var wheel in wheels)
        {
            wheel.collider.brakeTorque = brakeForce;
        }
    }

    private void Handbrake()
    {
        driftingAxis = Mathf.Clamp01(driftingAxis + Time.deltaTime);

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear)
            {
                var friction = wheelFrictionCurves[wheel.collider];
                friction.extremumSlip *= handbrakeDriftMultiplier * driftingAxis;
                wheel.collider.sidewaysFriction = friction;
            }
        }
    }

    private IEnumerator RecoverTractionGradually()
    {
        while (driftingAxis > 0)
        {
            driftingAxis = Mathf.Clamp01(driftingAxis - Time.deltaTime / 1.5f);

            foreach (var wheel in wheels)
            {
                var originalFriction = wheelFrictionCurves[wheel.collider];
                var friction = wheel.collider.sidewaysFriction;

                friction.extremumSlip = Mathf.Lerp(originalFriction.extremumSlip, friction.extremumSlip, driftingAxis);
                wheel.collider.sidewaysFriction = friction;
            }

            yield return null;
        }
    }

    public void RecoverTraction()
    {
        StopAllCoroutines();
        StartCoroutine(RecoverTractionGradually());
    }

    private void UpdateParticleEffects()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.isUsingEffects)
            {
                if (isDrifting)
                {
                    wheel.particleSystem.Play();
                    wheel.trailRenderer.emitting = true;
                }
                else
                {
                    wheel.particleSystem.Stop();
                    wheel.trailRenderer.emitting = false;
                }
            }
        }
    }

    private void AddScore()
    {
        if (isDrifting)
        {
            currentScore += scoreIncreaser;
            carUI.UpdateCurrentScoreText(currentScore.ToString());
        }
    }

    private void OnDriftStart()
    {
        currentScore = 0;
        carUI.ChangeCurrentScoreTextColor(Color.white);
        hasCollided = false;
        carUI.FadeScoreText(false);
    }

    private void OnDriftEnd()
    {     
        if (!hasCollided)
        {
            carUI.ChangeCurrentScoreTextColor(Color.green);
            SmoothAddScore(currentScore);      
        }
        else
        {
            carUI.ChangeCurrentScoreTextColor(Color.red);
        }
        carUI.FadeScoreText(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        hasCollided = true;
    }

    private void SmoothAddScore(int addedScore)
    {
        int startScore = totalScore;
        int targetScore = totalScore + addedScore;

        DOTween.To(() => totalScore, x =>
        {
            totalScore = x;
            carUI.UpdateTotalScoreText(totalScore.ToString());
        }, targetScore, scoreAddTime);
    }

    private void StartTimer()
    {
        timerIsRunning = true;
        carUI.UpdateTimerText(currentTime);
    }

    private void StopTimer()
    {
        timerIsRunning = false;
    }

    private void ResetTimer()
    {
        timerIsRunning = false;
        currentTime = 0f;
        carUI.UpdateTimerText(currentTime);
    }

    private void GameOver()
    {
        isGameOver = true;

        isDrifting = false;
        UpdateParticleEffects();
        OnDriftEnd();

        ThrottleOff();
        ApplyBrakes();

        int bestScore = BestScore.GetBestScore();
        if (totalScore > bestScore)
            BestScore.SetBestScore(totalScore);

        int reward = totalScore / 10;
        Money.AddMoney(reward);

        carUI.ShowGameOverPanel(totalScore, BestScore.GetBestScore(), reward);
    }
}
