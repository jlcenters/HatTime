using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    //will not be displayed, even though it is public
    [HideInInspector]
    public int id;

    [Header("Info")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject hat;

    [HideInInspector]
    public float hatTime;

    [Header("Components")]
    public Rigidbody rb;
    public Player photonPlayer;

    [PunRPC]
    public void Initialize(Player player)
    {
        photonPlayer = player;
        id = player.ActorNumber;

        //id num starts at 1, so we need to subtract 1 to add to the list
        GameManager.instance.players[id - 1] = this;

        //first player in the game receives hat
        if(id == 1)
        {
            GameManager.instance.GiveHat(id, true);
        }

        //you can only control the character you spawned in with
        if(!photonView.IsMine)
        {
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        //runs on only master client to avoid overlap
        if (PhotonNetwork.IsMasterClient)
        {
            //
            if(hatTime >= GameManager.instance.timeToWin && !(GameManager.instance.hasEnded))
            {
                GameManager.instance.hasEnded = true;
                GameManager.instance.photonView.RPC("WinGame", RpcTarget.All, id);
            }
        }

        //check if master client
        if (photonView.IsMine)
        {
            Move();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            //track time wearing hat
            if (hat.activeInHierarchy)
            {
                hatTime += Time.deltaTime;
            }
        }
    }

    void Move()
    {
        //get key that is pressed
        float x = Input.GetAxis("Horizontal") * moveSpeed;
        float z = Input.GetAxis("Vertical") * moveSpeed;

        rb.velocity = new Vector3(x, rb.velocity.y, z);
    }

    void Jump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        //if raycast collides with anything, aka is on land,
        //jump on impulse which is instantaneous
        if(Physics.Raycast(ray, 0.7f))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    public void WearHat(bool hasHat)
    {
        hat.SetActive(hasHat);
    }

    public void OnCollisionEnter(Collision collision)
    {
        //if you did not collide, ignore function call
        if (!photonView.IsMine)
        {
            return;
        }

        //check if collision was with another player
        if (collision.gameObject.CompareTag("Player"))
        {
            //check if player id matches the id of the hat wearer
            if(GameManager.instance.GetPlayer(collision.gameObject).id == GameManager.instance.hatPlayer)
            {
                //check if we can wear hat
                if (GameManager.instance.CanWearHat())
                {
                    //swap hats, show everyone
                    GameManager.instance.photonView.RPC("GiveHat", RpcTarget.All, id, false);
                }
            }
        }
    }

    //used to efficiently sync players
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if information is being sent, send over hat time
        if (stream.IsWriting)
        {
            stream.SendNext(hatTime);
        }
        //if information is receiving information, receive hat time
        else if (stream.IsReading)
        {
            hatTime = (float)stream.ReceiveNext();
        }
    }
}
