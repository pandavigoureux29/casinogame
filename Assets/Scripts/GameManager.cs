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
    PlayerInventorySO m_inventorySO;

    PlayerInventory m_localPlayerInventory;
    PlayerInventory m_otherPlayerInventory;

    List<PlayerInventory> m_playerInventories;

    public Action<PlayerInventory, PlayerInventory> OnInventoriesInitialized;
    public Action<PlayerInventory> OnInventoryUpdated;

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
    }

    public void StartGame()
    {
        Debug.Log("Start Game");

        if (PhotonNetwork.IsMasterClient)
        {
            var userIds = PhotonNetwork.PlayerList.Select(x => x.ActorNumber).ToArray();
            InitializePlayers(userIds);

            m_betManager.Initialize(userIds);

            myPhotonView?.RPC("RPC_StartGame", RpcTarget.Others, userIds); //workaround since some UserId are null on client
        }
    }

    [PunRPC]
    public void RPC_StartGame(int[] userIds)
    {
        InitializePlayers(userIds);
    }

    private void InitializePlayers(int[] userIds)
    {
        m_playerInventories = new List<PlayerInventory>();
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
            m_playerInventories.Add(inventory);
        }

        OnInventoriesInitialized?.Invoke(m_localPlayerInventory, m_otherPlayerInventory);
    }       

    public void ConfirmBet_Master(string userId, Dictionary<string, int> betTokensCount, bool win)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var inventory = GetInventory(userId);
            //apply the bet to inventory and update client's
            inventory.UpdateQuantities(betTokensCount,win);

            //reset condition
            if(inventory.TotalTokensCount <= 0)
            {
                inventory.Reset(m_inventorySO);
            }

            SendCurrentInventoryUpdate(userId);
            OnInventoryUpdated?.Invoke(inventory);        }
    }

    /// <summary>
    /// Send the current inventory from the server to the client so it can update
    /// </summary>
    public void SendCurrentInventoryUpdate(string userId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            List<string> keys;
            List<int> values;
            GetInventory(userId).GetTokensForNetwork(out keys, out values);
            myPhotonView?.RPC("RPC_UpdateInventory", RpcTarget.Others, userId, keys.ToArray(), values.ToArray());
        }
    }

    [PunRPC]
    public void RPC_UpdateInventory(string userId, string[] keys, int[] values)
    {
        var inventory = GetInventory(userId);
        inventory.UpdateTokensFromNetwork(keys,values);
        OnInventoryUpdated?.Invoke(inventory);
    }

    public string GetLocalPlayerId()
    {
        return PhotonNetwork.LocalPlayer.ActorNumber.ToString();
    }

    public PlayerInventory GetInventory(string userId)
    {
        return m_localPlayerInventory.UserId == userId ? m_localPlayerInventory : m_otherPlayerInventory;
    }

    public PlayerInventory GetCurrentInventory()
    {
        return m_localPlayerInventory.UserId == PhotonNetwork.LocalPlayer.ActorNumber.ToString() ? m_localPlayerInventory : m_otherPlayerInventory;
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //We could send other player's mouse position in here
    }
}
