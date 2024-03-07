using Newtonsoft.Json.Bson;
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

    [SerializeField]
    private Button m_betButton;

    private ChipStacksManager m_stacksManager;

    private List<UIChipBetIncrementer> m_uiIncrementers = new List<UIChipBetIncrementer>();
    private Dictionary<string, int> m_betTokensCount = new Dictionary<string, int>();

    private void Awake()
    {
        m_hubManager.OnInitialized += OnStacksInitialized;
        m_gameManager.BetManager.OnBetConfirmed += OnBetConfirmed;
        m_gameManager.OnInventoryUpdated += OnInventoryUpdated;
        m_gameManager.BetManager.OnBetQuantityChanged += OnBetQuantityChanged;
    }

    private void OnDestroy()
    {
        if (m_gameManager != null)
        {
            m_gameManager.BetManager.OnBetConfirmed -= OnBetConfirmed;
            m_gameManager.OnInventoryUpdated -= OnInventoryUpdated;
            m_gameManager.BetManager.OnBetQuantityChanged -= OnBetQuantityChanged;
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
        if(!m_betTokensCount.ContainsKey(token.Id))
        {
            m_betTokensCount[token.Id] = 0;
        }
        m_betTokensCount[token.Id]++;

        //send to network
        if(sendEvent)
            m_gameManager.BetManager.AddChipsToBet(m_stacksManager.Inventory, token.Id);

        UpdateBetButton();
    }

    public void OnRemoveBetStacked(UIChipBetIncrementer token, bool sendEvent = true)
    {
        if (m_betTokensCount.ContainsKey(token.Id))
        {
            m_betTokensCount[token.Id]--;
            if (m_betTokensCount[token.Id] == 0)
            {
                m_betTokensCount.Remove(token.Id);
            }
        }

        //send to network
        if (sendEvent)
            m_gameManager.BetManager.RemoveChipsFromBet(m_stacksManager.Inventory, token.Id);

        UpdateBetButton();
    }

    private void OnBetQuantityChanged(string userId, string tokenId, int totalBetIncrements)
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
        m_betTokensCount.Clear();
        UpdateBetButton();
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

    public void OnTurnChanged(string userId)
    {
        bool toggle = m_stacksManager.Inventory.UserId == userId;

        foreach (var uiToken in m_uiIncrementers)
        {
            uiToken.Toggle(toggle);
        }

        UpdateBetButton();
    }

    public void UpdateBetButton()
    {
        if(m_betButton == null)
        {
            return;
        }

        bool toggleButton = m_gameManager.BetManager.IsColorSelected && m_betTokensCount.Count > 0;

        m_betButton.interactable = toggleButton;
    }

}
