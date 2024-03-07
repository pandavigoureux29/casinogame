using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BetManager : MonoBehaviour, IPunObservable
{
    public enum EColor {NONE, GREEN, RED }

    //probably best in a config 
    public static int S_BET_INCREMENTS = 10;

    [SerializeField]
    private GameManager m_gameManager;


    public Action<string, string> OnAddTokenToBet;
    public Action<string, string> OnRemoveTokenFromBet;
    public Action<bool> OnBetConfirmed;

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

    public void AddTokenToBet(PlayerInventory playerInventory, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AddTokenOnMaster(playerInventory.UserId, tokenId);
        }

        myPhotonView?.RPC("RPC_AddTokenToBet", RpcTarget.Others, playerInventory.UserId, tokenId);
    }

    private void AddTokenOnMaster(string userId, string tokenId)
    {
        var playerBetData = GetPlayerBetData(userId);
        if (!playerBetData.BetTokensCount.ContainsKey(tokenId))
        {
            playerBetData.BetTokensCount[tokenId] = 0;
        }

        var currentBet = playerBetData.BetTokensCount[tokenId] * S_BET_INCREMENTS;
        if (m_gameManager.GetInventory(m_gameManager.GetCurrentInventory().UserId).CanBetMore(tokenId, currentBet))
        {
            playerBetData.BetTokensCount[tokenId]++;
        }
    }

    [PunRPC]
    private void RPC_AddTokenToBet(string userId, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AddTokenOnMaster(userId, tokenId);
        }

        OnAddTokenToBet?.Invoke(userId, tokenId);
    }

    private void RemoveTokenOnMaster(string userId, string tokenId)
    {
        var playerBetData = GetPlayerBetData(userId);
        if (!playerBetData.BetTokensCount.ContainsKey(tokenId) || playerBetData.BetTokensCount[tokenId] == 0)
            return;

        playerBetData.BetTokensCount[tokenId]--;
    }

    public void RemoveTokenFromBet(PlayerInventory playerInventory, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RemoveTokenOnMaster(playerInventory.UserId, tokenId);   
        }

        myPhotonView?.RPC("RPC_RemoveTokenFromBet", RpcTarget.Others, playerInventory.UserId, tokenId);
    }

    [PunRPC]
    private void RPC_RemoveTokenFromBet(string userId, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RemoveTokenOnMaster(userId, tokenId);
        }

        OnRemoveTokenFromBet?.Invoke(userId, tokenId);
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
            playerBetData.ColorBet = color;

            //add a new confirmed bet count
            if (playerBetData.IsBetValid())
            {
                m_confirmedBetsCount++;
                Debug.LogError("Bet is valid for " + userId);
            }

            //check if all bets have been set and declared
            if(m_confirmedBetsCount < m_playersBetData.Count)
            {
                Debug.LogError("Still waiting for more bets");
                return;
            }

            //check if all bets are valid
            foreach (var bet in m_playersBetData.Values)
            {
                bool win = m_currentBetResultColor == bet.ColorBet;

                m_gameManager.ConfirmBet_Master(bet.UserId, playerBetData.BetTokensCount, win);
                Debug.LogError("Bet " + bet.ColorBet + " for " + userId + " : " + win);

                if (bet.UserId == m_gameManager.GetLocalPlayerId())
                {
                    OnBetConfirmed?.Invoke(win);
                }
                else
                {
                    //notify client that the bet is done
                    myPhotonView?.RPC("RPC_ConfirmBet", RpcTarget.Others, bet.UserId, win);
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
    public void RPC_ConfirmBet(string userId, bool isBetWon)
    {
        //m_gameManager.ConfirmBet_Master(userId, isBetWon);
        if(userId == m_gameManager.GetLocalPlayerId())
        {
            OnBetConfirmed?.Invoke(isBetWon);
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
        public Dictionary<string, int> BetTokensCount = new Dictionary<string, int>();
        public bool BetConfirmed = false;

        public PlayerBetData(string userId)
        {
            UserId = userId;
        }

        public void Clear()
        {
            ColorBet = EColor.NONE;
            BetTokensCount.Clear();
            BetConfirmed = false;
        }

        public bool IsBetValid()
        {
            bool hasAtLeastOneBetToken = false;
            foreach (var token in BetTokensCount)
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
