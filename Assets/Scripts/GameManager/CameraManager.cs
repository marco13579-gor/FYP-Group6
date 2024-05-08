using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraManager : NetworkBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera cinemachineVirtual;

    private bool isTrigger = false;

    private bool useEdgesScrolling = false;

    private float fieldOfViewMax = 50;
    private float fieldOfViewMin = 10;
    private float targetFieldOfView = 50;
    private void Update()
    {
        HandleCameraMovement();
        HandleCameraRotation();
        HandCameraZoom();
    }
    private void HandleCameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (transform.position.z >= -70 && transform.position.z <= 21)
        {
            if (Input.GetKey(KeyCode.W)) inputDir.z = +0.5f;
            if (Input.GetKey(KeyCode.S)) inputDir.z = -0.5f;
        }
        if (transform.position.x >= -45 && transform.position.x <= 54)
        {
            if (Input.GetKey(KeyCode.A)) inputDir.x = -0.5f;
            if (Input.GetKey(KeyCode.D)) inputDir.x = +0.5f;
        }

        if(transform.position.z <= -70)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -70f);
        }
        if (transform.position.z >= 21)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 21f);
        }
        if (transform.position.x <= -45)
        {
            transform.position = new Vector3(-45f, transform.position.y, transform.position.z);
        }
        if (transform.position.x >= 54)
        {
            transform.position = new Vector3(54f, transform.position.y, transform.position.z);
        }



        if (Input.GetKey(KeyCode.Y)) useEdgesScrolling = !useEdgesScrolling;
        if (Input.GetKey(KeyCode.Space))
        {
            switch (GameNetworkManager.Instance.GetPlayerID())
            {
                case 0:
                    transform.position = new Vector3(-22, 3, -3);
                    break;
                case 1:
                    transform.position = new Vector3(34, 3, -3);
                    break;
                case 2:
                    transform.position = new Vector3(-25, 3, -54);
                    break;
                case 3:
                    transform.position = new Vector3(34, 3, -54);
                    break;
            }
        }
        if (useEdgesScrolling)
        {
            int edgeScrollSize = 20;

            if (Input.mousePosition.x < edgeScrollSize)
            {
                inputDir.x = -1f;
            }

            if (Input.mousePosition.y < edgeScrollSize)
            {
                inputDir.z = -1f;
            }

            if (Input.mousePosition.x > Screen.width - edgeScrollSize)
            {
                inputDir.x = +1f;
            }

            if (Input.mousePosition.y > Screen.height - edgeScrollSize)
            {
                inputDir.z = +1f;
            }
        }

        Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;

        float moveSpeed = 50f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleCameraRotation()
    {
        float rotateDir = 0f;
        if (Input.GetKey(KeyCode.Q)) rotateDir = -1f;
        if (Input.GetKey(KeyCode.E)) rotateDir = +1f;

        float rotateSpeed = 100f;
        transform.eulerAngles += new Vector3(0, rotateDir * rotateSpeed * Time.deltaTime, 0);
    }

    private void HandCameraZoom()
    {
        if(Input.mouseScrollDelta.y > 0)
        {
            targetFieldOfView -= 5;
        }
        if(Input.mouseScrollDelta.y < 0)
        {
            targetFieldOfView += 5;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, fieldOfViewMin, fieldOfViewMax);

        float zoomSpeed = 20f;
        cinemachineVirtual.m_Lens.FieldOfView = targetFieldOfView = Mathf.Lerp(cinemachineVirtual.m_Lens.FieldOfView, targetFieldOfView, Time.deltaTime * zoomSpeed);
    }

    public void OnSwitchCameraView(int playerId)
    {
        switch (playerId)
        {
            case 0:
                transform.position = new Vector3(-22, 3, -3);
                break;
            case 1:
                transform.position = new Vector3(34, 3, -3);
                break;
            case 2:
                transform.position = new Vector3(-25, 3, -54);
                break;
            case 3:
                transform.position = new Vector3(34, 3, -54);
                break;
        }
    }
}
