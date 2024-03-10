using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class UIChipBetIncrementer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_quantityText;

    [SerializeField]
    private Button m_buttonAdd;
    [SerializeField]
    private Button m_buttonRemove;

    [SerializeField]
    private Animator m_earningsTextAnimator;
    [SerializeField]
    private TextMeshProUGUI m_earningsText;

    private UIIncrementers m_incrementsManager;
    private ChipInventoryData m_chipInventoryData;
    public ChipInventoryData Chip => m_chipInventoryData;

    private string m_id;
    public string Id => m_id;

    private int m_betIncrements = 0;

    private int m_lastQuantity = 0;

    public void InitializeChip(UIIncrementers incrementsManager, ChipStack stack)
    {
        m_incrementsManager = incrementsManager;
        m_id = stack.ChipData.Id;
        m_chipInventoryData = stack.ChipData;
        UpdateQuantity();
        SetPosition(stack);
        SetButtonColor(m_buttonAdd);
        SetButtonColor(m_buttonRemove);
        m_lastQuantity = m_chipInventoryData.Quantity;
    }

    public void Toggle(bool toggle)
    {
        m_buttonAdd.gameObject.SetActive(toggle);
        m_buttonRemove.gameObject.SetActive(toggle);
    }

    private void UpdateQuantity(int qty)
    {
        m_quantityText.text = "x" + qty;
    }

    public void UpdateQuantity(bool isInventoryUpdate = false)
    {
        if (isInventoryUpdate)
        {
            var delta = m_chipInventoryData.Quantity - m_lastQuantity;
            if(delta != 0)
            {
                m_lastQuantity = m_chipInventoryData.Quantity;
                ShowEarnings(delta);
            }
        }

        var qty = m_chipInventoryData.Quantity - m_betIncrements;
        UpdateQuantity(qty);
    }

    public void OnAddBetClicked()
    {
        var currentBet = m_betIncrements;
        if( m_chipInventoryData.Quantity - currentBet > 0 && m_incrementsManager.TotalBetCount < BetManager.S_MAXIMUM_BET_COUNT)
        {
            m_betIncrements++;
            UpdateQuantity();
            m_incrementsManager.OnAddBetStacked(this);
        }
    }

    public void OnRemoveBetClicked()
    {
        if(m_betIncrements > 0)
        {
            m_betIncrements--;
            UpdateQuantity();
            m_incrementsManager.OnRemoveBetStacked(this);
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

    private void ShowEarnings(int qty)
    {
        string trigger = qty > 0 ? "Win" : "Lose";
        m_earningsTextAnimator.SetTrigger(trigger);
        m_earningsText.text = qty > 0 ? "+"+qty : "-"+qty; ;
    }

    private void SetButtonColor(Button button)
    {
        var chipData = DatabaseManager.Instance.GetChipData(m_chipInventoryData.Id);
        var colors = button.colors;
        colors.normalColor = chipData.Color;
        colors.selectedColor = chipData.Color;
        button.colors = colors;
    }

    private void SetPosition(ChipStack stack)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        Transform stackTransform = stack.GetBaseTransform();
        RectTransform rectTransform = GetComponent<RectTransform>();

        //var newPos = RectTransformUtility.WorldToScreenPoint(Camera.main, stackTransform.TransformPoint(m_worldOffset));
        var newPos = WorldToCanvasPosition(canvas.GetComponent<RectTransform>(), Camera.main, stack.GetBaseTransform().position);
        newPos.y = 0;

        rectTransform.anchoredPosition = newPos;
    }

    private Vector2 WorldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 position)
    {
        Vector2 temp = camera.WorldToViewportPoint(position);

        //Calculate position considering our percentage, using our canvas size
        temp.x *= canvas.sizeDelta.x;
        temp.y *= canvas.sizeDelta.y;

        temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
        temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

        return temp;
    }

}
