using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour, IPunObservable
{

    private PhotonView myPhotonView;

    // Start is called before the first frame update
    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        Debug.Log("Start Game");

        if (PhotonNetwork.IsMasterClient)
        {
            myPhotonView?.RPC("RPC_StartGame", RpcTarget.Others);
        }
    }

    [PunRPC]
    public void RPC_StartGame()
    {
        StartGame();
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
