using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToken : MonoBehaviour
{
    [SerializeField]
    private Image m_image;

    [SerializeField]
    private TextMeshProUGUI m_quantityText;

    [SerializeField]
    private Button m_takeChipsButton;

    private UITokenInventory m_uiTokenInventory;
    private Token m_token;

    private string m_id;
    public string Id => m_id;

    private int m_betStacks = 0;

    public void InitializeToken(UITokenInventory inventory, Token token)
    {
        m_uiTokenInventory = inventory;
        m_id = token.Id;
        m_image.color = token.Color;
        UpdateQuantity(token.Quantity);
        m_token = token;
    }

    public void UpdateQuantity(int qty)
    {
        m_quantityText.text = "x" + qty;
    }

    public void OnAddBetClicked()
    {
        var currentBet = m_betStacks * GameManager.S_BET_INCREMENTS;
        if( m_token.Quantity - currentBet >= GameManager.S_BET_INCREMENTS)
        {
            m_betStacks++;
            UpdateQuantity(m_token.Quantity - m_betStacks * GameManager.S_BET_INCREMENTS);
            m_uiTokenInventory.OnAddBetStacked(this);
        }
    }

    public void OnRemoveBetClicked()
    {
        if(m_betStacks > 0)
        {
            m_betStacks--;
            UpdateQuantity(m_token.Quantity - m_betStacks * GameManager.S_BET_INCREMENTS);
            m_uiTokenInventory.OnRemoveBetStacked(this);
        }
    }
}
