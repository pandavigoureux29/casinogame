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
    public Action<string, string> OnAddTokenToBet;
    public Action<string, string> OnRemoveTokenFromBet;

    private int m_currentSelectedChip = -1;

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

        CheckSelectedChip();
    }

    public void StartGame()
    {
        Debug.Log("Start Game");

        if (PhotonNetwork.IsMasterClient)
        {
            var userIds = PhotonNetwork.PlayerList.Select(x => x.ActorNumber).ToArray();
            InitializePlayers(userIds);
            m_gridManager.InstantiateGrid();
            myPhotonView?.RPC("RPC_StartGame", RpcTarget.Others, 
                m_gridManager.GreenIndexes.ToArray(),
                userIds); //workaround since some UserId are null on client
        }
    }

    [PunRPC]
    public void RPC_StartGame(int[] greenIndexes, int[] userIds)
    {
        InitializePlayers(userIds);
        m_gridManager.InstantiateGrid(greenIndexes.ToList());

        StartGame();
    }

    private void InitializePlayers(int[] userIds)
    {
        for (int i = 0; i < userIds.Count(); i++)
        {
            var inventory = new PlayerInventory(userIds[i].ToString(), Instantiate(m_inventorySO));
            if (userIds[i] == PhotonNetwork.LocalPlayer.ActorNumber)
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

    #region BET

    public void AddTokenToBet(PlayerInventory playerInventory, string tokenId)
    {
        myPhotonView?.RPC("RPC_AddTokenToBet", RpcTarget.Others, playerInventory.UserId, tokenId);
    }

    [PunRPC]
    private void RPC_AddTokenToBet(string userId, string tokenId)
    {
        OnAddTokenToBet?.Invoke(userId, tokenId);
    }
    public void RemoveTokenFromBet(PlayerInventory playerInventory, string tokenId)
    {
        myPhotonView?.RPC("RPC_RemoveTokenFromBet", RpcTarget.Others, playerInventory.UserId, tokenId);
    }

    [PunRPC]
    private void RPC_RemoveTokenFromBet(string userId, string tokenId)
    {
        OnRemoveTokenFromBet?.Invoke(userId, tokenId);
    }

    #endregion

    #region SELECT_CHIP

    private void CheckSelectedChip()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var chip = m_gridManager.GetClickedChip();
            if (chip != null)
            {
                SelectChip(chip.Index);
            }
        }
    }
    
    private void SelectChip(int chipIndex)
    {
        m_currentSelectedChip = chipIndex;
        m_gridManager.SelectChip(chipIndex);
        myPhotonView?.RPC("RPC_SelectChip", RpcTarget.Others, chipIndex);
    }

    [PunRPC]
    private void RPC_SelectChip(int chipIndex)
    {
        bool success = m_gridManager.SelectChip(chipIndex);
        //this will check on MasterClient that we're not trying to select an already flipped chip
        if (success)
        {
            m_currentSelectedChip = chipIndex;
        }
    }

    #endregion

    public void ConfirmBet(Dictionary<string, int> tokens)
    {

    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //We could send other player's mouse position in here
    }

    private PlayerInventory GetInventory(string userId)
    {
        return m_localPlayerInventory.UserId == userId ? m_localPlayerInventory : m_otherPlayerInventory;
    }
}
