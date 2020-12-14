using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchmakingController : MonoBehaviour
{
    public static MatchmakingController instance;

    public bool waitForPlayers;
    public int minPlayers;
    public int maxPlayers;
    public string menuScene;
    public string multiplayerScene;

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
    }

}
