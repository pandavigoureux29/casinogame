using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public bool HasAddedChips => TotalBetCount > 0;
    public int TotalBetCount => m_betChipsCount.Sum(x=>x.Value);

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
            var uiChip = go.GetComponent<UIChipBetIncrementer>();
            uiChip.InitializeChip(this, stack.Value);
            m_uiIncrementers.Add(uiChip);
        }
        m_hubManager.OnInitialized -= OnStacksInitialized;
    }

    public void OnAddBetStacked(UIChipBetIncrementer chip)
    {
        if(!m_betChipsCount.ContainsKey(chip.Id))
        {
            m_betChipsCount[chip.Id] = 0;
        }
        m_betChipsCount[chip.Id]++;

        m_gameManager.BetManager.AddChipToBet(m_stacksManager.Inventory, chip.Id);
    }

    public void OnRemoveBetStacked(UIChipBetIncrementer chip)
    {
        if (!m_betChipsCount.ContainsKey(chip.Id))
        {
            return;
        }

        m_betChipsCount[chip.Id]--;
        if (m_betChipsCount[chip.Id] == 0)
        {
            m_betChipsCount.Remove(chip.Id);
        }

        m_gameManager.BetManager.RemoveChipFromBet(m_stacksManager.Inventory, chip.Id);
    }

    //update values from server
    private void OnBetChipQuantityChanged(string userId, string chipId, int totalBetIncrements)
    {
        if (m_stacksManager.Inventory == null || m_stacksManager.Inventory.UserId != userId)
        {
            return;
        }

        var uiChip = m_uiIncrementers.Find(x => x.Id == chipId);
        if (uiChip != null)
        {
            uiChip.UpdateIncrement(totalBetIncrements);
        }

        if (m_betChipsCount.ContainsKey(chipId))
        {
            m_betChipsCount[chipId] = totalBetIncrements;
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
            foreach (var uiChipIncrementer in m_uiIncrementers)
            {
                uiChipIncrementer.ClearBetStacks();
                uiChipIncrementer.UpdateQuantity(true);
            }
        }
    }    

}
