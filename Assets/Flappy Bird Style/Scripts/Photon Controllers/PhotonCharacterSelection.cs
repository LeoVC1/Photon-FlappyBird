using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LigaHelpers.Variables;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using LigaHelpers.Events;

public class PhotonCharacterSelection : MonoBehaviourPunCallbacks
{
    public static PhotonCharacterSelection instance;
    private PhotonView PV;

    public int startTime;

    public bool inGame;

    private float remainingTimeCounter;

    private List<int> playersReady = new List<int>();

    [HideInInspector]
    public bool inCharacterSelection;

    private bool isGameLoading;

    private int playersInGame;

    private void Awake()
    {
        if (instance)
        {
            Destroy(instance.gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        PV = GetComponent<PhotonView>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneFinisedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneFinisedLoading;
    }

    public void StartCharacterSelection()
    {
        remainingTime.Value = startTime;
        remainingTimeCounter = startTime;
        inCharacterSelection = true;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }
    }

    private void Update()
    {
        if(inCharacterSelection)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            UpdateTimerVariables();
        }
    }

    private void UpdateTimerVariables()
    {
        remainingTimeCounter -= Time.deltaTime;

        int auxiliar = System.Convert.ToInt32(remainingTimeCounter);
        remainingTime.Value = auxiliar;

        PV.RPC("UpdateTimer", RpcTarget.All, auxiliar);
    }

    [PunRPC]
    private void UpdateTimer(int remainingTime)
    {
        if (this.remainingTime <= 0)
        {
            StartGame();

            this.remainingTime.Value = 0;
        }
        else
        {
            this.remainingTime.Value = remainingTime;
        }
    }


    #region Ready Methods
    public void GetReady()
    {
        PV.RPC("OnPlayerIsReady", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    private void GetUnready()
    {
        PV.RPC("OnPlayerUnready", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void OnPlayerIsReady(int playerID)
    {
        if (!playersReady.Contains(playerID))
        {
            playersReady.Add(playerID);
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if(playersReady.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            if(remainingTime > 5)
                remainingTimeCounter = 5;
        }
    }

    [PunRPC]
    private void OnPlayerUnready(int playerID)
    {
        if (playersReady.Contains(playerID))
        {
            playersReady.Remove(playerID);
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (playersReady.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            StartGame();
        }
    }

    #endregion

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            if (instance.inGame)
            {
                PhotonNetwork.LeaveRoom();

                StartCoroutine(DisconnectPlayer());
            }
            else if(inCharacterSelection)
            {
                stopCharacterSelection.Raise();
                StopCharacterSelection();
            }
        }
    }

    IEnumerator DisconnectPlayer()
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        SceneManager.LoadScene(MatchmakingController.instance.menuScene);
    }

    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (isGameLoading)
            return;

        isGameLoading = true;

        PhotonNetwork.LoadLevel(MatchmakingController.instance.multiplayerScene);
    }

    public void StopCharacterSelection()
    {
        GetUnready();

        inCharacterSelection = false;

        onQueueStatusChange.Raise(false);

        PhotonNetwork.LeaveRoom();

        playersReady.Clear();
    }

    private void OnSceneFinisedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MatchmakingController.instance.multiplayerScene)
        {
            inGame = true;

            if (MatchmakingController.instance.waitForPlayers)
            {
                PV.RPC("OnLoadGameScene", RpcTarget.MasterClient);
            }
            else
            {
                CreatePlayer();
            }
        }
    }

    [PunRPC]
    private void OnLoadGameScene()
    {
        playersInGame++;
        print(playersInGame);

        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            PV.RPC("CreatePlayer", RpcTarget.All);
        }
    }

    [PunRPC]
    private void CreatePlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), transform.position, Quaternion.identity, 0);
    }
}
