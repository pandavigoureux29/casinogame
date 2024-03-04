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

        if (Input.GetMouseButtonDown(0))
        {
            var chip = m_gridManager.GetClickedChip();
            if(chip != null)
            {
                FlipChip(chip.Index);
            }
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

    [PunRPC]
    private void FlipChip(int chipIndex)
    {
        m_gridManager.FlipChip(chipIndex);

        if (PhotonNetwork.IsMasterClient)
        {
            myPhotonView?.RPC("FlipChip", RpcTarget.Others, chipIndex);
        }
        else
        {
            myPhotonView?.RPC("FlipChip", RpcTarget.MasterClient, chipIndex);
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
