using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class GameManager : MonoBehaviour, IPunObservable
{

    private PhotonView myPhotonView;

    [SerializeField]
    GridManager m_gridManager;

    // Start is called before the first frame update
    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        //TODO : debug only, remove
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        Debug.Log("Start Game");


        if (PhotonNetwork.IsMasterClient)
        {
            m_gridManager.InstantiateGrid();
            myPhotonView?.RPC("RPC_StartGame", RpcTarget.Others, m_gridManager.GreenIndexes.ToArray());
        }
    }

    [PunRPC]
    public void RPC_StartGame(int[] greenIndexes)
    {
        m_gridManager.InstantiateGrid(greenIndexes.ToList());
        StartGame();
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
