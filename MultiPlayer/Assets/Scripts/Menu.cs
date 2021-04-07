using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;

    [Header("Main Screen")]
    public Button createRoom;
    public Button joinRoom;
    public Button quit;

    [Header("Lobby Screen")]
    public TextMeshProUGUI playerList;
    public Button startGame;

    //disable buttons while disconnected with master server
    private void Start()
    {
        createRoom.interactable = false;
        joinRoom.interactable = false;
    }


    //once connected, activate buttons
    public override void OnConnectedToMaster()
    {
        createRoom.interactable = true;
        joinRoom.interactable = true;
    }

    void SetScreen(GameObject screen)
    {
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        //enable requested screen
        screen.SetActive(true);
    }

    public void OnCreate(TMP_InputField roomName)
    {
        NetManager.instance.CreateRoom(roomName.text);
    }

    public void OnJoin(TMP_InputField roomName)
    {
        NetManager.instance.JoinRoom(roomName.text);
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    public void OnPlayerNameUpdate(TMP_InputField name)
    {
        PhotonNetwork.NickName = name.text;
    }

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        
        //call rpc, string name of method, to every player
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    //has to update for every player when someone joins/leaves lobby
    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerList.text = "";

        //for every player in room, add name to list
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerList.text += player.NickName + "\n";
        }

        //if host, show start game button
        if (PhotonNetwork.IsMasterClient)
        {
            startGame.interactable = true;
        }
        else
        {
            startGame.interactable = false;
        }
    }

    public void OnLeave()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    public void OnStartGame()
    {
        NetManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
}
