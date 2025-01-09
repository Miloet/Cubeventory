using Cinemachine;
using StarterAssets;
using System.Globalization;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientCharacterMovement : NetworkBehaviour
{

    [SerializeField] private PlayerInput inputs;
    [SerializeField] private CharacterController controller;
    [SerializeField] private ThirdPersonController camera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        enabled = IsOwner;
        inputs.enabled = IsOwner;
        controller.enabled = IsOwner;
        camera.enabled = IsOwner;

        if(IsOwner)
        {
            var cam = (CinemachineVirtualCamera)FindFirstObjectByType(typeof(CinemachineVirtualCamera)); 
            cam.Follow = transform.Find("PlayerCameraRoot");
        }
    }

 }
