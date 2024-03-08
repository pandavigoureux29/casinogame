using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIncrementers : MonoBehaviour
{
    [SerializeField]
    ChipStacksHubManager m_hubManager;
    [SerializeField]
    private GameManager m_gameManager;
    [SerializeField]
    private UIChipBetIncrementer m_uiIncrementerPrefab;

    [SerializeField]
    private bool m_isLocal;

    private ChipStacksManager m_stacksManager;

    private List<UIChipBetIncrementer> m_uiIncrementers = new List<UIChipBetIncrementer>();
    private Dictionary<string, int> m_betChipsCount = new Dictionary<string, int>();

    public bool HasAddedChips => m_betChipsCount.Count > 0;

    public Action OnBetQuantityChanged;

    private void Awake()
    {
        m_hubManager.OnInitialized += OnStacksInitialized;
        m_gameManager.BetManager.OnBetConfirmed += OnBetConfirmed;
        m_gameManager.OnInventoryUpdated += OnInventoryUpdated;
        m_gameManager.BetManager.OnBetChipQuantityChanged += OnBetChipQuantityChanged;
    }

    private void OnDestroy()
    {
        if (m_gameManager != null)
        {
            m_gameManager.BetManager.OnBetConfirmed -= OnBetConfirmed;
            m_gameManager.OnInventoryUpdated -= OnInventoryUpdated;
            m_gameManager.BetManager.OnBetChipQuantityChanged -= OnBetChipQuantityChanged;
        }
    }

    private void OnStacksInitialized(ChipStacksManager manager)
    {
        m_stacksManager = manager;
        foreach (var stack in m_stacksManager.Stacks)
        {
            var go = Instantiate(m_uiIncrementerPrefab, transform);
            var uiToken = go.GetComponent<UIChipBetIncrementer>();
            uiToken.InitializeToken(this, stack.Value);
            m_uiIncrementers.Add(uiToken);
        }
        m_hubManager.OnInitialized -= OnStacksInitialized;
    }

    public void OnAddBetStacked(UIChipBetIncrementer token, bool sendEvent=true)
    {
        if(!m_betChipsCount.ContainsKey(token.Id))
        {
            m_betChipsCount[token.Id] = 0;
        }
        m_betChipsCount[token.Id]++;
        OnBetQuantityChanged?.Invoke();

        //send to network
        if(sendEvent)
            m_gameManager.BetManager.AddChipToBet(m_stacksManager.Inventory, token.Id);
    }

    public void OnRemoveBetStacked(UIChipBetIncrementer token, bool sendEvent = true)
    {
        if (m_betChipsCount.ContainsKey(token.Id))
        {
            m_betChipsCount[token.Id]--;
            OnBetQuantityChanged?.Invoke();
            if (m_betChipsCount[token.Id] == 0)
            {
                m_betChipsCount.Remove(token.Id);
            }
        }

        //send to network
        if (sendEvent)
            m_gameManager.BetManager.RemoveChipFromBet(m_stacksManager.Inventory, token.Id);
    }

    private void OnBetChipQuantityChanged(string userId, string tokenId, int totalBetIncrements)
    {
        if (m_stacksManager.Inventory == null || m_stacksManager.Inventory.UserId != userId)
        {
            return;
        }

        var uiToken = m_uiIncrementers.Find(x => x.Id == tokenId);
        if (uiToken != null)
        {
            uiToken.UpdateIncrement(totalBetIncrements);
        }
    }

    private void OnBetConfirmed(bool isBetWon, BetManager.EColor color)
    {
        m_betChipsCount.Clear();
    }

    public void OnInventoryUpdated(PlayerInventory inventory)
    {
        if(inventory == m_stacksManager.Inventory)
        {
            foreach (var uiToken in m_uiIncrementers)
            {
                uiToken.ClearBetStacks();
                uiToken.UpdateQuantity();
            }
        }
    }    

}
