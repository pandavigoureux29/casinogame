using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class UIToken : MonoBehaviour
{
    [SerializeField]
    private Image m_image;

    [SerializeField]
    private TextMeshProUGUI m_quantityText;

    [SerializeField]
    private GameObject m_buttonsContainer;

    private UITokenInventory m_uiTokenInventory;
    private ChipData m_token;
    public ChipData Token => m_token;

    private string m_id;
    public string Id => m_id;

    private int m_betIncrements = 0;

    public void InitializeToken(UITokenInventory inventory, ChipData token)
    {
        m_uiTokenInventory = inventory;
        m_id = token.Id;
        UpdateQuantity(token.Quantity);
        m_token = token;
    }

    public void Toggle(bool toggle)
    {
        m_buttonsContainer.SetActive(toggle);
    }

    public void UpdateQuantity(int qty)
    {
        m_quantityText.text = "x" + qty;
    }

    public void UpdateQuantity()
    {
        var qty = m_token.Quantity - m_betIncrements * BetManager.S_BET_INCREMENTS;
        UpdateQuantity(qty);
    }

    public void OnAddBetClicked()
    {
        var currentBet = m_betIncrements * BetManager.S_BET_INCREMENTS;
        if( m_token.Quantity - currentBet >= BetManager.S_BET_INCREMENTS)
        {
            m_betIncrements++;
            UpdateQuantity();
            m_uiTokenInventory.OnAddBetStacked(this);
        }
    }

    public void OnRemoveBetClicked()
    {
        if(m_betIncrements > 0)
        {
            m_betIncrements--;
            UpdateQuantity();
            m_uiTokenInventory.OnRemoveBetStacked(this);
        }
    }

    public void UpdateAddBet()
    {
        m_betIncrements++;
        UpdateQuantity();
    }
    public void UpdateRemoveBet()
    {
        m_betIncrements--;
        UpdateQuantity();
    }

    public void UpdateIncrement(int increments)
    {
        m_betIncrements = increments;
        UpdateQuantity();
    }

    public void ClearBetStacks()
    {
        m_betIncrements = 0;
    }
}
