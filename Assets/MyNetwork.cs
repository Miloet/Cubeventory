using Unity.Netcode;
using UnityEngine;

public class MyNetwork : MonoBehaviour
{
    public void Client()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.StartClient();
    }
    public void Host()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.StartHost();
    }
    public void Server()
    {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.StartServer();
    }
}
