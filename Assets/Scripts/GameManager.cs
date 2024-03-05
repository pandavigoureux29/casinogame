using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using Newtonsoft.Json.Linq;

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

    private Player m_currentPlayer;

    public Action<PlayerInventory, PlayerInventory> OnInventoriesInitialized;
    public Action<string, string> OnAddTokenToBet;
    public Action<string, string> OnRemoveTokenFromBet;
    public Action<string> OnTurnChanged;
    public Action OnBetConfirmed;

    private int m_currentSelectedChip = -1;
    private Dictionary<string, int> m_betTokensCount = new Dictionary<string, int>();

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

    #region BET

    public void AddTokenToBet(PlayerInventory playerInventory, string tokenId)
    {   
        myPhotonView?.RPC("RPC_AddTokenToBet", RpcTarget.Others, playerInventory.UserId, tokenId);
    }

    [PunRPC]
    private void RPC_AddTokenToBet(string userId, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!m_betTokensCount.ContainsKey(tokenId))
            {
                m_betTokensCount[tokenId] = 0;
            }

            var currentBet = m_betTokensCount[tokenId] * S_BET_INCREMENTS;
            if (GetInventory(userId).CanBetMore(tokenId,currentBet))
            {
                m_betTokensCount[tokenId]++;
            }
            else
            {
                return;
            }
        }
        OnAddTokenToBet?.Invoke(userId, tokenId);
    }
    public void RemoveTokenFromBet(PlayerInventory playerInventory, string tokenId)
    {
        
        myPhotonView?.RPC("RPC_RemoveTokenFromBet", RpcTarget.Others, playerInventory.UserId, tokenId);           
    }

    [PunRPC]
    private void RPC_RemoveTokenFromBet(string userId, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!m_betTokensCount.ContainsKey(tokenId) || m_betTokensCount[tokenId] == 0)
                return;

            m_betTokensCount[tokenId]--;
        }

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

    #region ConfirmBet

    public void OnDeclareBet()
    {
        DeclareBet();
    }

    //on master : confirm the bet and move on to next turn
    //on client : ask the master for confirmation
    private void DeclareBet()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            bool isBetWon = m_gridManager.CheckChip(m_currentSelectedChip, ReversableChip.EColor.GREEN);

            ChangeTurn();
            OnTurnChanged?.Invoke(m_currentPlayer.ActorNumber.ToString());

            //notify client that the bet is done
            myPhotonView?.RPC("RPC_ConfirmBet", RpcTarget.Others, isBetWon, m_currentPlayer.ActorNumber);

            m_gridManager.FlipChip(m_currentSelectedChip);

            GetCurrentInventory().UpdateQuantities(m_betTokensCount, isBetWon);
            OnBetConfirmed?.Invoke();

            m_currentSelectedChip = -1;
            m_betTokensCount.Clear();

        }
        else
        {
            //notify master that the bet is set and confirmed
            myPhotonView?.RPC("RPC_DeclareBet", RpcTarget.Others);
        }
    }

    //From client to master to declare the bet
    [PunRPC]
    private void RPC_DeclareBet()
    {
        DeclareBet();
    }

    //From master to client to confirm the bet and update values
    [PunRPC]
    public void RPC_ConfirmBet(bool isBetWon, int turnPlayerId)
    {
        m_gridManager.FlipChip(m_currentSelectedChip);
        m_currentPlayer = PhotonNetwork.PlayerList.First(x => x.ActorNumber == turnPlayerId);
        OnTurnChanged?.Invoke(m_currentPlayer.ActorNumber.ToString());
        OnBetConfirmed?.Invoke();
    }

    #endregion

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //We could send other player's mouse position in here
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

    private PlayerInventory GetInventory(string userId)
    {
        return m_localPlayerInventory.UserId == userId ? m_localPlayerInventory : m_otherPlayerInventory;
    }

    private PlayerInventory GetCurrentInventory()
    {
        return m_localPlayerInventory.UserId == m_currentPlayer.ActorNumber.ToString() ? m_localPlayerInventory : m_otherPlayerInventory;
    }
}
