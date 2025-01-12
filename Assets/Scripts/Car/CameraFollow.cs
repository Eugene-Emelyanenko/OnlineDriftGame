using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Smoothness")]
    [Space(5)]
    [SerializeField] private float moveSmoothness;
    [SerializeField] private float rotSmoothness;

    [Space(5)]
    [Header("Offset")]
    [SerializeField] private Vector3 moveOffset;
    [SerializeField] private Vector3 rotOffset;

    private Transform carTransform;

    private void Start()
    {
        carTransform = transform.parent;
        transform.SetParent(null);
    }

    void FixedUpdate()
    {
        if (carTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        Vector3 targetPos = new Vector3();
        targetPos = carTransform.transform.TransformPoint(moveOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
    }

    private void HandleRotation()
    {
        var direction = carTransform.transform.position - transform.position;
        var rotation = new Quaternion();

        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSmoothness * Time.deltaTime);
    }
}
