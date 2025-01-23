using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DisableOnServer : NetworkBehaviour
{
    private void Start()
    {
        if (IsServer)
        {
            var mask = gameObject.AddComponent<RectMask2D>();
            mask.padding = Vector4.one * 1000;
            //print("REMOVED ON CLIENT");
            gameObject.SetActive(false);
        }
    }
}
