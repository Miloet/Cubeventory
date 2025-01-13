using Unity.Netcode;
using UnityEngine;

public class MyNetwork : MonoBehaviour
{
    public void StartClient()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.StartClient();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.StartHost();
    }
    public void StartServer()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.StartServer();
    }
}
