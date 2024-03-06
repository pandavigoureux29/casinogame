using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITokenInventory : MonoBehaviour
{
    List<UIToken> m_tokens;

    [SerializeField]
    private GameManager m_gameManager;
    [SerializeField]
    private UIToken m_tokenPrefab;

    [SerializeField]
    private bool m_isLocal;

    [SerializeField]
    private UITokenBetStack m_tokenBetStack;

    [SerializeField]
    private Button m_betButton;

    private PlayerInventory m_inventory;

    private List<UIToken> m_uiTokens = new List<UIToken>();
    private Dictionary<string, int> m_betTokensCount = new Dictionary<string, int>();

    private void Awake()
    {
        m_gameManager.OnInventoriesInitialized += OnInventoriesInitialized;
        m_gameManager.BetManager.OnAddTokenToBet += OnAddBetEvent;
        m_gameManager.BetManager.OnRemoveTokenFromBet += OnRemoveBetEvent;
        m_gameManager.BetManager.OnBetConfirmed += OnBetConfirmed;
        m_gameManager.OnInventoryUpdated += OnInventoryUpdated;
        m_gameManager.OnChipSelected += OnChipSelected;
        m_gameManager.OnTurnChanged += OnTurnChanged;
    }

    private void OnDestroy()
    {
        if (m_gameManager != null)
        {
            m_gameManager.BetManager.OnAddTokenToBet -= OnAddBetEvent;
            m_gameManager.BetManager.OnRemoveTokenFromBet -= OnRemoveBetEvent;
            m_gameManager.BetManager.OnBetConfirmed -= OnBetConfirmed;
            m_gameManager.OnInventoryUpdated -= OnInventoryUpdated;
            m_gameManager.OnChipSelected -= OnChipSelected;
            m_gameManager.OnTurnChanged -= OnTurnChanged;
        }
    }

    private void OnInventoriesInitialized(PlayerInventory localInventory, PlayerInventory otherInventory)
    {
        m_inventory = m_isLocal ? localInventory : otherInventory;    

        if(m_inventory == null || m_inventory.Inventory == null)
        {
            return;
        }

        foreach (var token in m_inventory.Inventory.tokens)
        {
            var go = Instantiate(m_tokenPrefab, transform);
            var uiToken = go.GetComponent<UIToken>();
            uiToken.InitializeToken(this, token);
            m_uiTokens.Add(uiToken);
        }

        m_gameManager.OnInventoriesInitialized -= OnInventoriesInitialized;
    }

    public void OnAddBetStacked(UIToken token, bool sendEvent=true)
    {
        if(!m_betTokensCount.ContainsKey(token.Id))
        {
            m_betTokensCount[token.Id] = 0;
        }
        m_betTokensCount[token.Id]++;

        if(m_tokenBetStack != null)
        {
            m_tokenBetStack.AddToken(token);
        }

        //send to network
        if(sendEvent)
            m_gameManager.BetManager.AddTokenToBet(m_inventory, token.Id);

        UpdateBetButton();
    }

    public void OnRemoveBetStacked(UIToken token, bool sendEvent = true)
    {
        if (m_betTokensCount.ContainsKey(token.Id))
        {
            m_betTokensCount[token.Id]--;
            if (m_betTokensCount[token.Id] == 0)
            {
                m_betTokensCount.Remove(token.Id);
            }

            if (m_tokenBetStack != null)
            {
                m_tokenBetStack.RemoveToken(token);
            }
        }

        //send to network
        if (sendEvent)
            m_gameManager.BetManager.RemoveTokenFromBet(m_inventory, token.Id);

        UpdateBetButton();
    }

    //from rpc
    private void OnAddBetEvent(string userId, string tokenId)
    {
        if(m_inventory.UserId != userId)
        {
            return;
        }

        var uiToken = m_uiTokens.Find(x=>x.Id == tokenId);
        if(uiToken != null)
        {
            uiToken.UpdateAddBet();
            OnAddBetStacked(uiToken, false);
        }
    }

    private void OnRemoveBetEvent(string userId, string tokenId)
    {
        if (m_inventory.UserId != userId)
        {
            return;
        }

        var uiToken = m_uiTokens.Find(x => x.Id == tokenId);
        if (uiToken != null)
        {
            uiToken.UpdateRemoveBet();
            OnRemoveBetStacked(uiToken, false);
        }
    }

    private void OnBetConfirmed(bool isBetWon)
    {
        m_betTokensCount.Clear();
        m_tokenBetStack.ClearStack();
        UpdateBetButton();
    }

    public void OnInventoryUpdated(PlayerInventory inventory)
    {
        if(inventory == m_inventory)
        {
            foreach (var uiToken in m_uiTokens)
            {
                uiToken.ClearBetStacks();
                uiToken.UpdateQuantity();
            }
        }
    }

    public void OnTurnChanged(string userId)
    {
        bool toggle = m_inventory.UserId == userId;

        foreach (var uiToken in m_uiTokens)
        {
            uiToken.Toggle(toggle);
        }

        UpdateBetButton();
    }

    private void OnChipSelected(int chipIndex)
    {
        UpdateBetButton();
    }

    private void UpdateBetButton()
    {
        if(m_betButton == null)
        {
            return;
        }

        bool toggleButton = m_gameManager.IsLocalPlayerTurn();
        toggleButton &= m_gameManager.IsChipSelected;
        toggleButton &= m_betTokensCount.Count > 0;

        m_betButton.interactable = toggleButton;
    }

}
