using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering.LookDev;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour, IPunObservable
{

    private PhotonView myPhotonView;

    [SerializeField]
    BetManager m_betManager;
    public BetManager BetManager => m_betManager;

    [SerializeField]
    GridManager m_gridManager;
    [SerializeField]
    PlayerInventorySO m_inventorySO;

    PlayerInventory m_localPlayerInventory;
    PlayerInventory m_otherPlayerInventory;

    private Player m_currentPlayer;
    public Player CurrentPlayer => m_currentPlayer;

    public Action<PlayerInventory, PlayerInventory> OnInventoriesInitialized;
    public Action<string> OnTurnChanged;
    public Action<PlayerInventory> OnInventoryUpdated;
    public Action<int> OnChipSelected;

    private int m_currentSelectedChip = -1;
    public bool IsChipSelected => m_currentSelectedChip >= 0;

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

            m_currentPlayer = PhotonNetwork.LocalPlayer;
            OnTurnChanged?.Invoke(m_currentPlayer.ActorNumber.ToString());

            myPhotonView?.RPC("RPC_StartGame", RpcTarget.Others, 
                m_gridManager.GreenIndexes.ToArray(),
                userIds,
                m_currentPlayer.ActorNumber); //workaround since some UserId are null on client
        }
    }

    [PunRPC]
    public void RPC_StartGame(int[] greenIndexes, int[] userIds, int turnPlayerId)
    {
        InitializePlayers(userIds);
        m_gridManager.InstantiateGrid(greenIndexes.ToList());

        m_currentPlayer = PhotonNetwork.PlayerList.First(x=>x.ActorNumber==turnPlayerId);
        OnTurnChanged?.Invoke(m_currentPlayer.ActorNumber.ToString());
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

    #region SELECT_CHIP

    private void CheckSelectedChip()
    {
        if (m_currentPlayer == PhotonNetwork.LocalPlayer && Input.GetMouseButtonDown(0))
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
        OnChipSelected?.Invoke(chipIndex);
    }

    [PunRPC]
    private void RPC_SelectChip(int chipIndex)
    {
        bool success = m_gridManager.SelectChip(chipIndex);
        //this will check on MasterClient that we're not trying to select an already flipped chip
        if (success)
        {
            m_currentSelectedChip = chipIndex;
            OnChipSelected?.Invoke(chipIndex);
        }
    }

    public bool CheckChip(ReversableChip.EColor color)
    {
        return m_gridManager.CheckChip(m_currentSelectedChip, color);
    }

    #endregion

    public void ConfirmBet(Dictionary<string, int> betTokensCount, bool win, int turnPlayerId = -1)
    {
        m_gridManager.FlipChip(m_currentSelectedChip);

        if (PhotonNetwork.IsMasterClient)
        {
            var inventory = GetCurrentInventory();
            //apply the bet to inventory and update client's
            inventory.UpdateQuantities(betTokensCount,win);

            //reset condition
            if(inventory.TotalTokensCount <= 0)
            {
                inventory.Reset(m_inventorySO);
            }

            SendCurrentInventoryUpdate();
            OnInventoryUpdated?.Invoke(inventory);

            ChangeTurn();
        }
        else 
        { 
            //set the current player on client
            m_currentPlayer = PhotonNetwork.PlayerList.First(x => x.ActorNumber == turnPlayerId);
        }
        m_currentSelectedChip = -1;
        OnTurnChanged?.Invoke(m_currentPlayer.ActorNumber.ToString());
    }

    /// <summary>
    /// Send the current inventory from the server to the client so it can update
    /// </summary>
    public void SendCurrentInventoryUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            List<string> keys;
            List<int> values;
            GetCurrentInventory().GetTokensForNetwork(out keys, out values);
            myPhotonView?.RPC("RPC_UpdateInventory", RpcTarget.Others, CurrentPlayer.ActorNumber.ToString(), keys.ToArray(), values.ToArray());
        }
    }

    [PunRPC]
    public void RPC_UpdateInventory(string userId, string[] keys, int[] values)
    {
        var inventory = GetInventory(userId);
        inventory.UpdateTokensFromNetwork(keys,values);
        OnInventoryUpdated?.Invoke(inventory);
    }

    private void ChangeTurn()
    {
        if(m_currentPlayer == PhotonNetwork.LocalPlayer)
        {
            m_currentPlayer = PhotonNetwork.PlayerListOthers.FirstOrDefault();
        }
        else
        {
            m_currentPlayer = PhotonNetwork.LocalPlayer;
        }
    }

    public PlayerInventory GetInventory(string userId)
    {
        return m_localPlayerInventory.UserId == userId ? m_localPlayerInventory : m_otherPlayerInventory;
    }

    public PlayerInventory GetCurrentInventory()
    {
        return m_localPlayerInventory.UserId == m_currentPlayer.ActorNumber.ToString() ? m_localPlayerInventory : m_otherPlayerInventory;
    }

    public bool IsLocalPlayerTurn()
    {
        return m_currentPlayer == PhotonNetwork.LocalPlayer;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //We could send other player's mouse position in here
    }
}
