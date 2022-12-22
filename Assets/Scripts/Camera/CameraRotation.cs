using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float mouseRotationMultiplier;
    [SerializeField] private int maxZoom;
    [SerializeField] private int minZoom;
    [SerializeField] private int maxXRotation;
    [SerializeField] private int minXRotation;

    private bool isRotating = false;

    private Vector3 pressedPosition;
    private float rotationXValue;
    private float rotationYValue;

    private float screenX;
    private float screenY;

    private int zoom = 0;

    private float wheelValue;

    [SerializeField] private CameraPosition posComponent;

    // Update is called once per frame
    void Update()
    {
        DoZoom();
        DoRotationCheck();
    }

    private void DoRotationCheck()
    {
        if (Input.GetMouseButtonDown(2))
        {
            StartRotation();
        }
        if (Input.GetMouseButtonUp(2))
        {
            StopRotation();
        }

        DoRotation();
    }

    private void StartRotation()
    {
        isRotating = true;
        pressedPosition = Input.mousePosition;
    }

    private void StopRotation()
    {
        isRotating = false;
    }

    private void DoRotation()
    {
        if (isRotating == true)
        {
            screenX = Input.mousePosition.x - pressedPosition.x;
            screenY = Input.mousePosition.y - pressedPosition.y;
            float screenXPos = 0;
            float screenYPos = 0;

            if (screenX > 6)
            {
                rotationYValue = 1;
                screenXPos = screenX - 6;
                pressedPosition = Input.mousePosition;
            }
            else if (screenX < -6)
            {
                rotationYValue = -1;
                screenXPos = -screenX - 6;
                pressedPosition = Input.mousePosition;
            }
            else
            {
                rotationYValue = 0;
            }
            if (screenY > 3)
            {
                rotationXValue = -1 + ((100 - screenY) / 100);
                screenYPos = screenY;
                pressedPosition = Input.mousePosition;
            }
            else if (screenY < -3)
            {
                rotationXValue = 1 - ((100 - -screenY) / 100);
                screenYPos = -screenY;
                pressedPosition = Input.mousePosition;
            }
            else
            {
                rotationXValue = 0;
            }

            if (rotationXValue != 0)
            {
                //Rotate this on the X
                Quaternion newRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x + (rotationXValue * (posComponent.RotationSpeed)), 0, 0);
                newRotation = Quaternion.RotateTowards(transform.localRotation, newRotation, posComponent.RotationSpeed  * Time.deltaTime);
                transform.localRotation = newRotation;

                float x = transform.localRotation.eulerAngles.x;

                if (x > maxXRotation)
                {
                    x = maxXRotation;
                }
                if (x < minXRotation)
                {
                    x = minXRotation;
                }

                transform.localEulerAngles = new Vector3(x, 0, 0);
            }

            if (rotationYValue != 0)
            {
                //Rotate our position component on the Y
                float rotValue = transform.rotation.eulerAngles.y + ((rotationYValue * posComponent.RotationSpeed));
                posComponent.RotateRig(rotValue, mouseRotationMultiplier * ((screenXPos / (Screen.width / 2))) * 10);
            }
        }
    }

    private void DoZoom()
    {
        wheelValue = Input.GetAxis("Mouse ScrollWheel");

        if (wheelValue != 0)
        {
            if (wheelValue > 0)
            {
                zoom = -1;
            }
            if (wheelValue < 0)
            {
                zoom = 1;
            }
        }
        else
        {
            zoom = 0;
        }

        if (zoom != 0)
        {
            if (transform.localPosition.y > maxZoom || transform.localPosition.y < minZoom)
            {
                Vector3 newZoom = new Vector3(0, zoom * zoomSpeed, -(zoom * zoomSpeed));
                transform.localPosition += newZoom;
            }
            CheckMouseTarget();
        }
        if (transform.localPosition.y <= maxZoom)
        {
            transform.localPosition = new Vector3(0, maxZoom + 1, -(maxZoom + 1));
        }
        if (transform.localPosition.y >= minZoom)
        {
            transform.localPosition = new Vector3(0, minZoom - 1, -(minZoom - 1));
        }

        AffectSpeed();
    }

    private void CheckMouseTarget()
    {
        RaycastHit hit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 300))
        {
            if (hit.collider.tag == "Collider") 
            {
                posComponent.SetTargetUnit(hit.collider.GetComponent<UnitCollider>().Unit);
            }
        }
    }

    private void AffectSpeed()
    {
        float multiplier = transform.localPosition.y / minZoom;

        posComponent.AdjustSpeed(multiplier);
    }
}
