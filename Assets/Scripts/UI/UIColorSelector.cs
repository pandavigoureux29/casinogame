using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIColorSelector;

public class UIColorSelector : MonoBehaviour
{

    [SerializeField]
    private BetManager m_betManager;
    [SerializeField]
    private List<ColorData> m_colorsData;
    [SerializeField]
    private Button m_button;

    [SerializeField]
    private UIIncrementers m_tokenInventory;

    private int m_currentColorDataIndex;

    private void Awake()
    {
        m_currentColorDataIndex = 0;
        RefreshColor();
    }

    public void OnClicked()
    {
        m_currentColorDataIndex++;
        if(m_currentColorDataIndex >= m_colorsData.Count)
        {
            m_currentColorDataIndex = 0;
        }
        RefreshColor();
        m_betManager.ChangeBetColor(m_colorsData[m_currentColorDataIndex].ColorEnum);
    }

    private void RefreshColor()
    {
        var colorData = m_colorsData[m_currentColorDataIndex];
        var colors = m_button.colors;
        colors.normalColor = colorData.Color;
        colors.selectedColor = colorData.Color;
        m_button.colors = colors;
    }

    [Serializable]
    public class ColorData
    {
        public Color Color;
        public BetManager.EColor ColorEnum;
    }
}
