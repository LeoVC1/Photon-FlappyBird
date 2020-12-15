using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.Events;
using System;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static event UnityAction<bool> OnServerStatusChange;
    public static event UnityAction<bool> OnQueueStatusChange;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();

        OnServerStatusChange = new UnityAction<bool>(EventOnServerStatusChange);
        OnQueueStatusChange = new UnityAction<bool>(EventOnQueueStatusChange);
    }
    private void EventOnServerStatusChange(bool arg0) { }
    private void EventOnQueueStatusChange(bool arg0) { }

    public override void OnConnectedToMaster()
    {
        print("Player has connected to the Photon Server");
        OnServerStatusChange.Invoke(true);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        OnServerStatusChange.Invoke(false);
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
        OnQueueStatusChange.Invoke(true);

        PhotonNetwork.JoinRandomRoom();
    }

    public void OnCancelButtonClicked()
    {
        OnQueueStatusChange.Invoke(false);

        PhotonNetwork.LeaveRoom();
    }
}
