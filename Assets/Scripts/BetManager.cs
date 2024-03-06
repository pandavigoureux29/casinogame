using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BetManager : MonoBehaviour, IPunObservable
{
    //probably best in a config 
    public static int S_BET_INCREMENTS = 10;

    [SerializeField]
    private GameManager m_gameManager;


    public Action<string, string> OnAddTokenToBet;
    public Action<string, string> OnRemoveTokenFromBet;
    public Action<bool> OnBetConfirmed;

    private PhotonView myPhotonView;

    private ReversableChip.EColor m_currentColor;
    private Dictionary<string, int> m_betTokensCount = new Dictionary<string, int>();

    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
    }

    #region BET

    public void AddTokenToBet(PlayerInventory playerInventory, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AddTokenOnMaster(tokenId);
        }

        myPhotonView?.RPC("RPC_AddTokenToBet", RpcTarget.Others, playerInventory.UserId, tokenId);
    }

    private void AddTokenOnMaster(string tokenId)
    {
        if (!m_betTokensCount.ContainsKey(tokenId))
        {
            m_betTokensCount[tokenId] = 0;
        }

        var currentBet = m_betTokensCount[tokenId] * S_BET_INCREMENTS;
        if (m_gameManager.GetInventory(m_gameManager.GetCurrentInventory().UserId).CanBetMore(tokenId, currentBet))
        {
            m_betTokensCount[tokenId]++;
        }
    }

    [PunRPC]
    private void RPC_AddTokenToBet(string userId, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AddTokenOnMaster(tokenId);
        }

        OnAddTokenToBet?.Invoke(userId, tokenId);
    }

    private void RemoveTokenOnMaster(string tokenId)
    {
        if (!m_betTokensCount.ContainsKey(tokenId) || m_betTokensCount[tokenId] == 0)
            return;

        m_betTokensCount[tokenId]--;
    }

    public void RemoveTokenFromBet(PlayerInventory playerInventory, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RemoveTokenOnMaster(tokenId);   
        }

        myPhotonView?.RPC("RPC_RemoveTokenFromBet", RpcTarget.Others, playerInventory.UserId, tokenId);
    }

    [PunRPC]
    private void RPC_RemoveTokenFromBet(string userId, string tokenId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RemoveTokenOnMaster(tokenId);
        }

        OnRemoveTokenFromBet?.Invoke(userId, tokenId);
    }

    #endregion

    #region BET_COLOR

    public void ChangeBetColor(ReversableChip.EColor eColor)
    {
        m_currentColor = eColor;
    }

    #endregion


    #region ConfirmBet

    public void OnDeclareBet()
    {
        if (m_gameManager.IsChipSelected)
        {
            DeclareBet(m_currentColor);
        }
    }

    //on master : confirm the bet and move on to next turn
    //on client : ask the master for confirmation
    private void DeclareBet(ReversableChip.EColor color)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!IsBetValid())
            {
                Debug.LogError("There's no token bet");
                return;
            }
            bool isBetWon = m_gameManager.CheckChip(color);

            m_gameManager.ConfirmBet(m_betTokensCount, isBetWon);

            //notify client that the bet is done
            myPhotonView?.RPC("RPC_ConfirmBet", RpcTarget.Others, isBetWon, m_gameManager.CurrentPlayer.ActorNumber);

            OnBetConfirmed?.Invoke(isBetWon);

            m_betTokensCount.Clear();

        }
        else
        {
            //notify master that the bet is set and confirmed
            myPhotonView?.RPC("RPC_DeclareBet", RpcTarget.Others, (int)m_currentColor);
        }
    }

    //From client to master to declare the bet
    [PunRPC]
    private void RPC_DeclareBet(int colorInt)
    {
        ReversableChip.EColor sentColor = (ReversableChip.EColor)colorInt;
        DeclareBet(sentColor);
    }

    //From master to client to confirm the bet and update values
    [PunRPC]
    public void RPC_ConfirmBet(bool isBetWon, int turnPlayerId)
    {
        m_gameManager.ConfirmBet(null, isBetWon, turnPlayerId);
        OnBetConfirmed?.Invoke(isBetWon);
    }

    #endregion

    private bool IsBetValid()
    {
        foreach ( var token in m_betTokensCount)
        {
            if (token.Value > 0)
                return true;
        }
        return false;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
