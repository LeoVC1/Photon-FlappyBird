using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    public Button btnJogar;
    public TextMeshProUGUI serverStatus;
    public TextMeshProUGUI queueStatus;

    private void Start()
    {
        PhotonLobby.OnServerStatusChange += PhotonLobby_OnServerStatusChange;
        PhotonLobby.OnQueueStatusChange += PhotonLobby_OnQueueStatusChange;
    }

    private void PhotonLobby_OnQueueStatusChange(bool arg0)
    {
        btnJogar.gameObject.SetActive(!arg0);
        queueStatus.text = arg0 ? "Na fila..." : "";
    }

    private void PhotonLobby_OnServerStatusChange(bool arg0)
    {
        btnJogar.interactable = arg0;
        serverStatus.text = arg0 ? "Conectado" : "Desconectado";
    }
}
