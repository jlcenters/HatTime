using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    public bool hasEnded = false;
    public float timeToWin;
    public float invincible;
    private float hatPickup;

    [Header("Players")]
    public string playerPrefab;
    public Transform[] spawnPts;
    public PlayerController[] players;
    public int hatPlayer;
    private int playersInGame;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("NewPlayer", RpcTarget.AllBuffered);
    }

    //this will happen for every player when a new player is added to the game
    [PunRPC]
    void NewPlayer()
    {
        playersInGame++;

        if(playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    
    void SpawnPlayer()
    {
        //player prefab is added randomly from one of the spawn points, with its default rotation
        GameObject player = PhotonNetwork.Instantiate(playerPrefab, spawnPts[Random.Range(0, spawnPts.Length)].position, Quaternion.identity);
        
        //grabs player script for rpc useage
        PlayerController playerScript = player.GetComponent<PlayerController>();

        //initializes new player by calling the init script for its class, which everyone will do. 
        //the player who just spawned is the one used in the params
        playerScript.photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }


    //get player controller who has with the same id as is requested
    public PlayerController GetPlayer(int playerId)
    {
        return players.First(x => x.id == playerId);
    }

    //get player controller who has the same game object as is requested
    public PlayerController GetPlayer(GameObject player)
    {
        return players.First(x => x.gameObject == player);
    }

    //when player takes hat wearing player
    [PunRPC]
    public void GiveHat(int playerId, bool first)
    {
        //if not the first player to wear hat, remove hat from previous player
        if (!first)
        {
            GetPlayer(hatPlayer).WearHat(false);
        }

        //new hat owner dons his crown
        hatPlayer = playerId;
        GetPlayer(playerId).WearHat(true);

        //timer starts
        hatPickup = Time.time;
    }

    //check if player can steal hat
    public bool CanWearHat()
    {
        if(Time.time > hatPickup + invincible)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //when timer has been reached, the game ends and a winner is announced
    [PunRPC]
    void WinGame(int playerId)
    {
        hasEnded = true;
        PlayerController player = GetPlayer(playerId);

        //set ui to show winner
        UI.instance.DisplayWinTxt(player.photonPlayer.NickName);
        //after 3 seconds, the players will be sent to the main menu
        Invoke("ReturnToMenu", 3.0f);
    }

    void ReturnToMenu()
    {
        PhotonNetwork.LeaveRoom();
        NetManager.instance.ChangeScene("Menu");
    }
}
