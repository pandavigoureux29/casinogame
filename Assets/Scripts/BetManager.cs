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

    private Dictionary<string, int> m_betTokensCount = new Dictionary<string, int>();

    void Start()
    {
        myPhotonView = GetComponent<PhotonView>();
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
            if (m_gameManager.GetInventory(userId).CanBetMore(tokenId, currentBet))
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
            bool isBetWon = m_gameManager.CheckChip(ReversableChip.EColor.GREEN);

            m_gameManager.ConfirmBet();

            //notify client that the bet is done
            myPhotonView?.RPC("RPC_ConfirmBet", RpcTarget.Others, isBetWon, m_gameManager.CurrentPlayer.ActorNumber);


            m_gameManager.GetCurrentInventory().UpdateQuantities(m_betTokensCount, isBetWon);
            OnBetConfirmed?.Invoke(isBetWon);

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
        m_gameManager.ConfirmBet(turnPlayerId);
        OnBetConfirmed?.Invoke(isBetWon);
    }

    #endregion

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
