using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby instance;

    public TextMeshProUGUI onlinePlayers;

    private Coroutine displayPlayersOnline;
    
    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    private IEnumerator GetPlayersOnline()
    {
        while (true)
        {
            onlinePlayers.text = PhotonNetwork.CountOfPlayers.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    public override void OnConnectedToMaster()
    {
        print("Player has connected to the Photon Server");
        OnServerStatusChange.Raise(true);
        PhotonNetwork.AutomaticallySyncScene = true;

        if(displayPlayersOnline == null)
            displayPlayersOnline = StartCoroutine(GetPlayersOnline());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        OnServerStatusChange.Raise(false);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("Tried to join a random game but failed. There must be no open games available");
        CreateRoom();
    }

    private void CreateRoom()
    {
        print("Trying to create a new Room");
        int randomRoomName = UnityEngine.Random.Range(0, 10);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte) MatchmakingController.instance.maxPlayers };
        PhotonNetwork.CreateRoom("Room: " + randomRoomName, roomOps);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("Tried to create a new room but failed, there must already be a room with the same name");

        CreateRoom();
    }

    public void OnBattleButtonClicked()
    {
        print("Battle Button was click");

        OnQueueStatusChange.Raise(true);

        PhotonNetwork.JoinRandomRoom();
    }

    public void OnCancelButtonClicked()
    {
        OnQueueStatusChange.Raise(false);

        PhotonNetwork.LeaveRoom();
    }
}
