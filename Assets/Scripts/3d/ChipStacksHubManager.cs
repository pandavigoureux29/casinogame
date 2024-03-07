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
    private ChipStacksManager m_localBetStack;
    [SerializeField]
    private ChipStacksManager m_otherBetStack;

    [SerializeField]
    PlayerInventorySO m_inventorySO;

    private Dictionary<string, ChipStacksManager> m_inventoryStacksMap;
    private Dictionary<string, ChipStacksManager> m_betStacksMap;

    private void Awake()
    {
        m_gameManager.OnInventoriesInitialized += OnInventoriesInitialized;
        m_gameManager.OnInventoryUpdated += OnInventoryUpdated;
        m_gameManager.BetManager.OnBetQuantityChanged += OnBetQuantityChanged;
    }

    private void OnDestroy()
    {
        if(m_gameManager != null)
        {
            m_gameManager.OnInventoriesInitialized -= OnInventoriesInitialized;
            m_gameManager.OnInventoryUpdated -= OnInventoryUpdated;
            if(m_gameManager.BetManager != null)
            {
                m_gameManager.BetManager.OnBetQuantityChanged -= OnBetQuantityChanged;
            }
        }
    }

    private void OnInventoriesInitialized(PlayerInventory localInventory, PlayerInventory otherInventory)
    {
        m_localInventoryStacks.Initialize(localInventory);
        m_otherInventoryStacks.Initialize(otherInventory);

        m_localBetStack.Initialize(m_inventorySO);
        m_otherBetStack.Initialize(m_inventorySO);

        m_betStacksMap = new Dictionary<string, ChipStacksManager>();
        m_inventoryStacksMap = new Dictionary<string, ChipStacksManager>();


        m_betStacksMap[localInventory.UserId] = m_localBetStack;
        m_inventoryStacksMap[localInventory.UserId] = m_localInventoryStacks;
        if (otherInventory != null)
        {
            m_betStacksMap[otherInventory.UserId] = m_otherBetStack;
            m_inventoryStacksMap[otherInventory.UserId] = m_otherInventoryStacks;
        }
    }

    private void OnInventoryUpdated(PlayerInventory inventory)
    {
        m_inventoryStacksMap[inventory.UserId].RefreshFromInventory();
    }

    private void OnBetQuantityChanged(string userId, string chipId, int totalBetIncrements)
    {
        var chipData = m_inventorySO.GetChipData(chipId);
        var inventory = m_gameManager.GetInventory(userId);
        int currentQuantity = inventory.GetQuantity(chipId);
        int totalBetChipsCount = totalBetIncrements * BetManager.S_BET_INCREMENTS;

        m_betStacksMap[userId].RefreshChips(chipData, totalBetChipsCount);
        m_inventoryStacksMap[userId].RefreshChips(chipData, currentQuantity - totalBetChipsCount);
    }
}
