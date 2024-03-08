using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChipStacksHubManager : MonoBehaviour
{
    [SerializeField] 
    private GameManager m_gameManager;
    [SerializeField]
    private ChipStacksManager m_localInventoryStacks;
    [SerializeField]
    private ChipStacksManager m_otherInventoryStacks;

    [SerializeField]
    private BetStack m_localBetStack;
    [SerializeField]
    private BetStack m_otherBetStack;

    private Dictionary<string, ChipStacksManager> m_inventoryStacksMap;
    private Dictionary<string, BetStack> m_betStacksMap;

    public Action<ChipStacksManager> OnInitialized;

    private void Awake()
    {
        m_gameManager.OnInventoriesInitialized += OnInventoriesInitialized;
        m_gameManager.OnInventoryUpdated += OnInventoryUpdated;
        m_gameManager.BetManager.OnBetChipQuantityChanged += OnBetQuantityChanged;
        m_gameManager.BetManager.OnBetConfirmed += OnBetConfirmed;
    }

    private void OnDestroy()
    {
        if(m_gameManager != null)
        {
            m_gameManager.OnInventoriesInitialized -= OnInventoriesInitialized;
            m_gameManager.OnInventoryUpdated -= OnInventoryUpdated;
            if(m_gameManager.BetManager != null)
            {
                m_gameManager.BetManager.OnBetChipQuantityChanged -= OnBetQuantityChanged;
                m_gameManager.BetManager.OnBetConfirmed -= OnBetConfirmed;
            }
        }
    }

    private void OnInventoriesInitialized(PlayerInventory localInventory, PlayerInventory otherInventory)
    {
        m_localInventoryStacks.Initialize(localInventory);
        m_otherInventoryStacks.Initialize(otherInventory);

        //keep stacks in dicos
        m_betStacksMap = new Dictionary<string, BetStack>();
        m_inventoryStacksMap = new Dictionary<string, ChipStacksManager>();

        m_betStacksMap[localInventory.UserId] = m_localBetStack;
        m_inventoryStacksMap[localInventory.UserId] = m_localInventoryStacks;
        if (otherInventory != null)
        {
            m_betStacksMap[otherInventory.UserId] = m_otherBetStack;
            m_inventoryStacksMap[otherInventory.UserId] = m_otherInventoryStacks;
        }

        OnInitialized?.Invoke(m_localInventoryStacks);
    }

    private void OnInventoryUpdated(PlayerInventory inventory)
    {
        m_inventoryStacksMap[inventory.UserId].RefreshFromInventory();
    }

    private void OnBetQuantityChanged(string userId, string chipId, int totalBetCount)
    {
        var chipData = DatabaseManager.Instance.GetChipData(chipId);
        var inventory = m_gameManager.GetInventory(userId);
        int currentQuantity = inventory.GetQuantity(chipId);

        m_betStacksMap[userId].Refresh(chipId, totalBetCount);
        m_inventoryStacksMap[userId].RefreshChips(chipId, currentQuantity - totalBetCount);
    }

    private void OnBetConfirmed(bool win, BetManager.EColor betcolor)
    {
        foreach(var betStack in m_betStacksMap.Values)
        {
            betStack.ClearStack();
        }
    }
}
