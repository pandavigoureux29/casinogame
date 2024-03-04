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

    private string m_id;
    public string Id => m_id;

    public void InitializeToken(Token token)
    {
        m_id = token.Id;
        m_image.color = token.Color;
        UpdateQuantity(token.Quantity);
    }

    public void UpdateQuantity(int qty)
    {
        m_quantityText.text = "x" + qty;
    }

    public void OnTakeChipsClicked()
    {

    }
}
