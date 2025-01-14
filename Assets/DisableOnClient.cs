using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;


public class DisableOnClient : NetworkBehaviour
{
    private void Start()
    {
        if(!IsServer) gameObject.SetActive(false);
    }
}
