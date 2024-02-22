using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    [SerializeField] Button serverButton;

    private void Start()
    {
        if (hostButton) hostButton.onClick.AddListener(ToHost);
        if (clientButton) clientButton.onClick.AddListener(ToClient);
        if (serverButton) serverButton.onClick.AddListener(ToServer);
    }
    void ToHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby",UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    void ToClient()
    {
        NetworkManager.Singleton.StartClient();
        //NetworkManager.Singleton.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    void ToServer()
    {
        NetworkManager.Singleton.StartServer();
    }

}
