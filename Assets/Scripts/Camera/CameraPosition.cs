using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [SerializeField] private bool isMoving;
    [SerializeField] private Vector3 positionChange = Vector3.zero;
    [SerializeField] private float rotationChange;
    [SerializeField] private float defaultCameraSpeed = 1;
    [SerializeField] private float defaultRotationSpeed = 100;

    public float CameraSpeed = 1;
    public float RotationSpeed = 1;

    [SerializeField] private UnitObject targetUnit;
    [SerializeField] private LayerMask mask;

    private void Update()
    {
        positionChange = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.W))
            {
                positionChange += transform.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                positionChange += -transform.forward;
            }
            targetUnit = null;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.A))
            {
                positionChange += -transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                positionChange += transform.right;

            }
            targetUnit = null;
        }

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            if (Input.GetKey(KeyCode.Q))
            {
                rotationChange = -1;
            }
            if (Input.GetKey(KeyCode.E))
            {
                rotationChange = 1;

            }
        }
        else
        {
            rotationChange = 0;
        }

        float rotationValue = transform.rotation.eulerAngles.y + (rotationChange * RotationSpeed);
        RotateRig(rotationValue);

        if(targetUnit != null)
        {
            MoveToTargetUnitPosition();
            return;
        }

        //transform.rotation = Quaternion.LookRotation(new Vector3(0, transform.rotation.eulerAngles.y + (rotationChange * RotationSpeed), 0));

        if (positionChange == Vector3.zero )
        {
            isMoving = false;
            return;
        }

        isMoving = true;

        transform.localPosition += positionChange * CameraSpeed;

        RaycastHit hit;

        if (Physics.Raycast(transform.position + new Vector3(0,5,0), -transform.up, out hit))
        {
            transform.position = hit.point + new Vector3(0, 0.5f, 0);
        }
    } 

    public void RotateRig(float newValue, float multiplier = 1)
    {
        Quaternion newRotation = Quaternion.Euler(0, newValue, 0);
        newRotation = Quaternion.RotateTowards(transform.rotation, newRotation, (RotationSpeed * Time.deltaTime) * multiplier);
        transform.localRotation = newRotation;
    }

    public void SetTargetUnit(UnitObject newTarget)
    {
        targetUnit = newTarget;
    }

    public void AdjustSpeed(float Multiplier)
    {
        CameraSpeed = defaultCameraSpeed * Multiplier;

        if(CameraSpeed <  defaultCameraSpeed * 0.3f)
        {
            CameraSpeed = defaultCameraSpeed * 0.3f;
        }
    }

    private void MoveToTargetUnitPosition()
    {
        float newY = 0;

        RaycastHit hit;

        if (Physics.Raycast(transform.position + new Vector3(0, 5, 0), -transform.up, out hit, 100, mask))
        {
            newY = hit.point.y + 0.5f;
        }

        Vector3 newPos = new Vector3(targetUnit.transform.position.x, newY, targetUnit.transform.position.z);

        transform.position = newPos;
    }
}
