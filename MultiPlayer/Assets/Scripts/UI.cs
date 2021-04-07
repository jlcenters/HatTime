using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class UI : MonoBehaviour
{
    public PlayerUIContainer[] playerContainers;
    public TextMeshProUGUI winTxt;

    public static UI instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitializePlayerUI();
    }

    private void Update()
    {
        UpdatePlayerUI();
    }

    void InitializePlayerUI()
    {
        //only enable the containers that will be used
        for(int i = 0; i < playerContainers.Length; i++)
        {
            PlayerUIContainer container = playerContainers[i];

            if(i < PhotonNetwork.PlayerList.Length)
            {
                container.obj.SetActive(true);
                container.nameTxt.text = PhotonNetwork.PlayerList[i].NickName;
                container.hatTime.maxValue = GameManager.instance.timeToWin;
            }
            else
            {
                container.obj.SetActive(false);
            }
        }
    }

    void UpdatePlayerUI()
    {
        //slider displays hat win progress
        for(int i = 0; i < GameManager.instance.players.Length; i++)
        {
            if(GameManager.instance.players[i] != null)
            {
                playerContainers[i].hatTime.value = GameManager.instance.players[i].hatTime;
            }
        }
    }

    //display the winning text, using the name of the winner
    public void DisplayWinTxt(string winner)
    {
        winTxt.gameObject.SetActive(true);
        winTxt.text = winner + " wins! \nReturning to Main Menu...";
    }
}




//a class rather than a component
[System.Serializable]
public class PlayerUIContainer
{
    public GameObject obj;
    public TextMeshProUGUI nameTxt;
    public Slider hatTime;
}
