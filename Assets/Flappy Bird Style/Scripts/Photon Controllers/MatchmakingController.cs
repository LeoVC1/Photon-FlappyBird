using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchmakingController : MonoBehaviour
{
    public static MatchmakingController instance;

    public int minPlayers;
    public int maxPlayers;

    [SerializeField]
    private string menuScene;
    [SerializeField]
    private string gameplayerScene;

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

        DontDestroyOnLoad(this.gameObject);

        PV = GetComponent<PhotonView>();
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneFinisedLoading;
        PhotonRoom.OnPlayersCountChange += OnPlayersCountChange;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneFinisedLoading;
        PhotonRoom.OnPlayersCountChange -= OnPlayersCountChange;
    }

    private void OnPlayersCountChange(int playersCount)
    {
        if(playersCount >= minPlayers)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.LoadLevel(instance.gameplayerScene);
    }

    private void OnSceneFinisedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == instance.gameplayerScene)
        {
            PV.RPC("OnLoadGameScene", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void OnLoadGameScene()
    {
        _playersInGame++;

        if (_playersInGame == PhotonNetwork.PlayerList.Length)
        {
            PV.RPC("CreatePlayer", RpcTarget.All);
        }
    }

    [PunRPC]
    private void CreatePlayer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), Vector3.zero, Quaternion.identity, 0);
    }


    private PhotonView PV;
    private int _playersInGame;
}
