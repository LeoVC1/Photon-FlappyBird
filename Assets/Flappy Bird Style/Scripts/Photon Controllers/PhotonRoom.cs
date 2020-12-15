using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.Events;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonRoom instance;

    public int PlayersInRoom
    {
        get 
        {
            return _playersInRoom;
        }
        set 
        {
            _playersInRoom = value;
            OnPlayersCountChange.Invoke(_playersInRoom);
        }
    }

    public static event UnityAction<int> OnPlayersCountChange; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if ((instance != this))
            {
                Destroy(this.gameObject);
            }
        }

        DontDestroyOnLoad(gameObject);

        PV = GetComponent<PhotonView>();

        OnPlayersCountChange = new UnityAction<int>(EventOnPlayersCountChange);
    }

    private void EventOnPlayersCountChange(int arg0) { }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Has joined room");

        UpdatePlayersInRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        UpdatePlayersInRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        Debug.Log("A new players has joined the room");

        UpdatePlayersInRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        Debug.Log("A players has left the room");

        UpdatePlayersInRoom();
    }

    private void UpdatePlayersInRoom()
    {
        PlayersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;

        if (PlayersInRoom == MatchmakingController.instance.maxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
    }

    private int _playersInRoom;
    private PhotonView PV;
}