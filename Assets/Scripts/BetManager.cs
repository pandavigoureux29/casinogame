using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BetManager;

public class BetManager : MonoBehaviour, IPunObservable
{
    public enum EColor {NONE, GREEN, RED }

    public static int S_MAXIMUM_BET_COUNT = 10;

    [SerializeField]
    private GameManager m_gameManager;

    public Action<string, string, int> OnBetChipQuantityChanged;
    public Action<bool,EColor> OnBetConfirmed;
    public Action<string> OnBetDeclared;
    public Action<EColor> OnBetColorChanged;

    private PhotonView myPhotonView;

    private Dictionary<string, PlayerBetData> m_playersBetData;
    /// <summary>
    /// Color the local player is betting on
    /// </summary>
    private EColor m_currentSelectedLocalColor = EColor.NONE;
    public bool IsColorSelected => m_currentSelectedLocalColor != EColor.NONE;

    /// <summary>
    /// Color generated on Master that is the result to guess
    /// </summary>
    private EColor m_currentBetResultColor = EColor.NONE;

    private int m_confirmedBetsCount = 0;

    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
    }

    public void Initialize(int[] userIds)
    {
        m_playersBetData = new Dictionary<string, PlayerBetData>(m_confirmedBetsCount);
        foreach(int userId in userIds)
        {
            m_playersBetData.Add(userId.ToString(),new PlayerBetData(userId.ToString()));
        }
        GenerateColorResult();
    }

    private void GenerateColorResult()
    {
        var r = UnityEngine.Random.Range(1, 100);
        m_currentBetResultColor = r <= 50 ? EColor.GREEN : EColor.RED;
        //Debug.LogError("Color to guess " + m_currentBetResultColor.ToString());
    }

    #region BET_CHIPS

    public void AddChipToBet(PlayerInventory playerInventory, string chipId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var playerBetData = GetPlayerBetData(playerInventory.UserId);

            if(playerBetData.TotalBetChipsCount >= S_MAXIMUM_BET_COUNT)
            {
                return;
            }

            var currentBet = playerBetData.GetChipCount(chipId);
            if (m_gameManager.GetInventory(m_gameManager.GetCurrentInventory().UserId).CanBetMore(chipId, currentBet))
            {
                playerBetData.AddChip(chipId);
            }

            var totalBetCount = playerBetData.GetChipCount(chipId);
            OnBetChipQuantityChanged?.Invoke(playerInventory.UserId, chipId, totalBetCount);

            myPhotonView?.RPC("RPC_OnBetQuantityChanged_Client", RpcTarget.Others, playerInventory.UserId, chipId, totalBetCount);
        }
        else
        {
            myPhotonView?.RPC("RPC_AddChipToBet_Master", RpcTarget.MasterClient, playerInventory.UserId, chipId);
        }

    }

    [PunRPC]
    private void RPC_AddChipToBet_Master(string userId, string chipId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var inventory = m_gameManager.GetInventory(userId);
            AddChipToBet(inventory, chipId);
        }
    }


    public void RemoveChipFromBet(PlayerInventory playerInventory, string chipId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //remove chips and notify client of new quantity
            int totalBetCount = RemoveChipOnMaster(playerInventory.UserId, chipId);
            OnBetChipQuantityChanged?.Invoke(playerInventory.UserId, chipId, totalBetCount);
            myPhotonView?.RPC("RPC_OnBetQuantityChanged_Client", RpcTarget.Others, playerInventory.UserId, chipId, totalBetCount);
        }
        else
        {
            myPhotonView?.RPC("RPC_RemoveChipFromBet_Master", RpcTarget.MasterClient, playerInventory.UserId, chipId);
        }
    }

    private int RemoveChipOnMaster(string userId, string chipId)
    {
        var playerBetData = GetPlayerBetData(userId);
        playerBetData.RemoveChip(chipId);
        return playerBetData.GetChipCount(chipId);
    }

    [PunRPC]
    private void RPC_RemoveChipFromBet_Master(string userId, string chipId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var inventory = m_gameManager.GetInventory(userId);
            RemoveChipFromBet(inventory, chipId);
        }
    }

    //from Master to client when the bet quantity has changed
    [PunRPC]
    private void RPC_OnBetQuantityChanged_Client(string userId, string chipId, int totalBet)
    {
        OnBetChipQuantityChanged?.Invoke(userId, chipId, totalBet);
    }

    #endregion

    #region BET_COLOR

    public void ChangeBetColor(EColor eColor)
    {
        m_currentSelectedLocalColor = eColor;
        OnBetColorChanged?.Invoke(eColor);
    }

    #endregion

    #region ConfirmBet

    public void OnDeclareBet()
    {
        DeclareBet(m_gameManager.GetLocalPlayerId(), m_currentSelectedLocalColor);
    }

    //on master : confirm the bet and move on to next turn
    //on client : ask the master for confirmation
    private void DeclareBet(string userId, EColor color)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var playerBetData = GetPlayerBetData(userId);
            if (playerBetData.BetConfirmed)
            {
                return;
            }
            playerBetData.ColorBet = color;

            //add a new confirmed bet count
            if (playerBetData.IsBetValid())
            {
                m_confirmedBetsCount++;
                playerBetData.BetConfirmed = true;
            }
            else
            {
                return;
            }

            //check if all bets have been set and declared
            if(m_confirmedBetsCount < m_playersBetData.Count)
            {
                //we have one bet declared but missing one
                
                //if the bet is from the local player, just send the event for the UI
                if(playerBetData.UserId == m_gameManager.GetLocalPlayerId())
                {
                    OnBetDeclared?.Invoke(playerBetData.UserId);
                }
                else
                {
                    //else, notify the client the bet is validated but waiting for another player
                    myPhotonView?.RPC("RPC_DeclareBet_Validated", RpcTarget.Others, playerBetData.UserId);
                }
                return;
            }

            //check if all bets are valid
            foreach (var bet in m_playersBetData.Values)
            {
                bool win = m_currentBetResultColor == bet.ColorBet;

                m_gameManager.ConfirmBet_Master(bet.UserId, bet.BetChipsCount, win);

                if (bet.UserId == m_gameManager.GetLocalPlayerId())
                {
                    OnBetConfirmed?.Invoke(win, m_currentBetResultColor);
                }
                else
                {
                    //notify client that the bet is done
                    myPhotonView?.RPC("RPC_ConfirmBet", RpcTarget.Others, bet.UserId, win, (int)m_currentBetResultColor);
                }

                bet.Clear();
            }

            GenerateColorResult();
            m_confirmedBetsCount = 0;
            m_currentSelectedLocalColor = EColor.NONE;
        }
        else
        {
            //notify master that the bet is set and confirmed
            myPhotonView?.RPC("RPC_DeclareBet", RpcTarget.Others, userId, (int)m_currentSelectedLocalColor);
        }
    }

    //From client to master to declare the bet
    [PunRPC]
    private void RPC_DeclareBet(string userId, int colorInt)
    {
        EColor sentColor = (EColor)colorInt;
        DeclareBet(userId, sentColor);
    }

    //From Master to client to notify the bet has been validated
    [PunRPC]
    private void RPC_DeclareBet_Validated(string userId)
    {
        OnBetDeclared?.Invoke(userId);
    }

    //From master to client to confirm the bet and update values
    [PunRPC]
    public void RPC_ConfirmBet(string userId, bool isBetWon, int colorInt)
    {
        //m_gameManager.ConfirmBet_Master(userId, isBetWon);
        if(userId == m_gameManager.GetLocalPlayerId())
        {
            m_currentSelectedLocalColor = EColor.NONE;
            EColor betColor = (EColor)colorInt;
            OnBetConfirmed?.Invoke(isBetWon, betColor);
        }
    }

    #endregion

    private PlayerBetData GetPlayerBetData(string userId)
    {
        if(!m_playersBetData.ContainsKey(userId))
        {
            return null;
        }
        return m_playersBetData[userId];
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

    public class PlayerBetData
    {
        public string UserId;
        public EColor ColorBet;
        public Dictionary<string, int> BetChipsCount = new Dictionary<string, int>();
        public bool BetConfirmed = false;

        private int m_totalBetChipsCount = 0;
        public int TotalBetChipsCount => m_totalBetChipsCount;

        public PlayerBetData(string userId)
        {
            UserId = userId;
        }

        public int GetChipCount(string chipId)
        {
            if (!BetChipsCount.ContainsKey(chipId))
            {
                return 0;
            }
            return BetChipsCount[chipId];
        }

        public void AddChip(string chipId)
        {
            if (!BetChipsCount.ContainsKey(chipId))
            {
                BetChipsCount[chipId] = 0;
            }

            BetChipsCount[chipId]++;
            m_totalBetChipsCount++;
        }

        public void RemoveChip(string chipId)
        {
            if (!BetChipsCount.ContainsKey(chipId) || BetChipsCount[chipId] == 0)
                return;

            BetChipsCount[chipId]--;
            m_totalBetChipsCount--;
        }

        public void Clear()
        {
            ColorBet = EColor.NONE;
            BetChipsCount.Clear();
            BetConfirmed = false;
            m_totalBetChipsCount = 0;
        }

        public bool IsBetValid()
        {
            bool hasAtLeastOneBetChip = false;
            foreach (var chip in BetChipsCount)
            {
                if (chip.Value > 0)
                {
                    hasAtLeastOneBetChip = true;
                }
            }

            return hasAtLeastOneBetChip && ColorBet != EColor.NONE;
        }
    }
}
