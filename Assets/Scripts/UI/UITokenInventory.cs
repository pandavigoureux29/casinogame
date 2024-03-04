using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITokenInventory : MonoBehaviour
{
    List<UIToken> m_tokens;

    [SerializeField]
    private GameManager m_gameManager;
    [SerializeField]
    private UIToken m_tokenPrefab;

    [SerializeField]
    private bool m_isLocal;

    Dictionary<string, int> m_betTokens = new Dictionary<string, int>();

    private void Awake()
    {
        m_gameManager.OnInventoriesInitialized += OnInventoriesInitialized;
    }

    private void OnInventoriesInitialized(PlayerInventory localInventory, PlayerInventory otherInventory)
    {
        var inventory = m_isLocal ? localInventory : otherInventory;    

        if(inventory == null || inventory.Inventory == null)
        {
            return;
        }

        foreach (var token in inventory.Inventory.tokens)
        {
            var go = Instantiate(m_tokenPrefab, transform);
            var uiToken = go.GetComponent<UIToken>();
            uiToken.InitializeToken(this, token);
        }
    }

    public void OnAddBetStacked(UIToken token)
    {
        if(!m_betTokens.ContainsKey(token.Id))
        {
            m_betTokens[token.Id] = 0;
        }
        m_betTokens[token.Id]++;
    }

    public void OnRemoveBetStacked(UIToken token)
    {
        if (m_betTokens.ContainsKey(token.Id))
        {
            m_betTokens[token.Id]--;
            if (m_betTokens[token.Id] == 0)
            {
                m_betTokens.Remove(token.Id);
            }
        }
    }

    public void OnConfirmBet()
    {
        m_gameManager.ConfirmBet(m_betTokens);
    }
}
