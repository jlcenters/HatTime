using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//For this game, I did not include the Photon Demos and Live Chat feature
//users can create rooms which hold individual players who are on the same master server

public class NetManager : MonoBehaviourPunCallbacks
{

    //creating a global object

    public static NetManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            gameObject.SetActive(false);
        }

        //set instance, carry over when loaded in
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    //tagged so that all players will call the same function
    [PunRPC]
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
