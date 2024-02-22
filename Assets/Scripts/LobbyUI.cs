using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button changeTeamButton;

    private void Start()
    {

        // Verificar si eres el host y mostrar u ocultar los botones de comenzar y cambiar equipo
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsHost)
        {
            if (startButton) startButton.gameObject.SetActive(true);
            if (changeTeamButton) changeTeamButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            if (startButton) startButton.gameObject.SetActive(false);
            if (changeTeamButton) changeTeamButton.gameObject.SetActive(true);
        }
        changeTeamButton.onClick.AddListener(ChangePlayerTeam);
    }
    void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Main", UnityEngine.SceneManagement.LoadSceneMode.Single);
        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (player.PlayerObject != null)
                player.PlayerObject.GetComponent<PlayerNetwork>().CanShoot.Value=true;
        }
    }






    private void ChangePlayerTeam()
    {
        // Obtener la referencia al jugador local
        PlayerNetwork player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>();

        // Cambiar el equipo del jugador
        if (player.playerTeam.Value == PlayerNetwork.TeamColor.red)
            player.ChangePlayerTeam(PlayerNetwork.TeamColor.blue);
        else
            player.ChangePlayerTeam(PlayerNetwork.TeamColor.red);
    }

}
