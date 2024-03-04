using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using System;

public class GameManager : MonoBehaviour, IPunObservable
{
    //probably best in a config 
    public static int S_BET_INCREMENTS = 10;

    private PhotonView myPhotonView;

    [SerializeField]
    GridManager m_gridManager;
    [SerializeField]
    PlayerInventorySO m_inventorySO;

    PlayerInventory m_localPlayerInventory;
    PlayerInventory m_otherPlayerInventory;

    Player m_currentPlayer;

    public Action<PlayerInventory, PlayerInventory> OnInventoriesInitialized;

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
        InitializePlayers();

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

    private void InitializePlayers()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Count() ; i++)
        {
            var inventory = new PlayerInventory(PhotonNetwork.PlayerList[i].UserId, Instantiate(m_inventorySO));
            if (PhotonNetwork.PlayerList[i].UserId == PhotonNetwork.LocalPlayer.UserId)
            {
                m_localPlayerInventory = inventory;
            }
            else
            {
                m_otherPlayerInventory = inventory;
            }
        }

        OnInventoriesInitialized?.Invoke(m_localPlayerInventory, m_otherPlayerInventory);
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

    public void ConfirmBet(Dictionary<string, int> tokens)
    {

    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //We could send other player's mouse position in here
    }
}
