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

    [SerializeField]
    private GameManager m_gameManager;


    public Action<string, string, int> OnBetQuantityChanged;
    public Action<bool,EColor> OnBetConfirmed;

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
        Debug.LogError("Color to guess " + m_currentBetResultColor.ToString());
    }

    #region BET

    public void AddChipToBet(PlayerInventory playerInventory, string chipId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var totalBetIncrements = AddChipOnMaster(playerInventory.UserId, chipId);
            OnBetQuantityChanged?.Invoke(playerInventory.UserId, chipId, totalBetIncrements);

            myPhotonView?.RPC("RPC_OnBetQuantityChanged_Client", RpcTarget.Others, playerInventory.UserId, chipId, totalBetIncrements);
        }
        else
        {
            myPhotonView?.RPC("RPC_AddChipToBet_Master", RpcTarget.MasterClient, playerInventory.UserId, chipId);
        }

    }

    private int AddChipOnMaster(string userId, string chipId)
    {
        var playerBetData = GetPlayerBetData(userId);

        var currentBet = playerBetData.GetChipCount(chipId);
        if (m_gameManager.GetInventory(m_gameManager.GetCurrentInventory().UserId).CanBetMore(chipId, currentBet))
        {
            playerBetData.AddChip(chipId);
        }

        return playerBetData.GetChipCount(chipId);
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
            OnBetQuantityChanged?.Invoke(playerInventory.UserId, chipId, totalBetCount);
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
        OnBetQuantityChanged?.Invoke(userId, chipId, totalBet);
    }

    #endregion

    #region BET_COLOR

    public void ChangeBetColor(EColor eColor)
    {
        m_currentSelectedLocalColor = eColor;
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

            //check if all bets have been set and declared
            if(m_confirmedBetsCount < m_playersBetData.Count)
            {
                return;
            }

            //check if all bets are valid
            foreach (var bet in m_playersBetData.Values)
            {
                bool win = m_currentBetResultColor == bet.ColorBet;

                m_gameManager.ConfirmBet_Master(bet.UserId, playerBetData.BetChipsCount, win);
                Debug.LogError("Bet " + bet.ColorBet + " for " + userId + " : " + win);

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
        }
        else
        {
            //notify master that the bet is set and confirmed
            myPhotonView?.RPC("RPC_DeclareBet", RpcTarget.Others, m_gameManager.GetLocalPlayerId(), (int)m_currentSelectedLocalColor);
        }
    }

    //From client to master to declare the bet
    [PunRPC]
    private void RPC_DeclareBet(string userId, int colorInt)
    {
        EColor sentColor = (EColor)colorInt;
        DeclareBet(userId, sentColor);
    }

    //From master to client to confirm the bet and update values
    [PunRPC]
    public void RPC_ConfirmBet(string userId, bool isBetWon, int colorInt)
    {
        //m_gameManager.ConfirmBet_Master(userId, isBetWon);
        if(userId == m_gameManager.GetLocalPlayerId())
        {
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
            bool hasAtLeastOneBetToken = false;
            foreach (var token in BetChipsCount)
            {
                if (token.Value > 0)
                {
                    hasAtLeastOneBetToken = true;
                }
            }

            return hasAtLeastOneBetToken && ColorBet != EColor.NONE;
        }
    }
}
