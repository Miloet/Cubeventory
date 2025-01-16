using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;


public class DisableOnClient : NetworkBehaviour
{
    private void Start()
    {
        if (!IsServer)
        {
            Renderer[] all = GetComponentsInChildren<Renderer>();

            foreach(var obj in all)
            {
                obj.enabled = false;
            }
            print("REMOVED ON CLIENT");
            //gameObject.SetActive(false);
        }
    }
}
