using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MouseBehaviour : NetworkBehaviour
{

    public static ulong PlayerID;
    public static Canvas canvas;

    private void Start()
    {
        if(IsServer)
        {
            Canvas can = FindFirstObjectByType<Canvas>();
            if (can != null)
            {
                NetworkObject.TrySetParent(can.transform, true);
                canvas = can;
            }
            else
            {
                Debug.LogError("Canvas not found!");
            }
        }
        if(IsOwner)
        {
            PlayerID = NetworkObject.OwnerClientId;
            GetComponent<Image>().enabled = false;
        }
    }

    private Vector2 previousPosition;

    void Update()
    {

        //Perform visuals


        previousPosition = transform.position;
        if (!IsOwner) return;

        if(Application.isFocused)
            transform.position = Input.mousePosition;

    }
}
