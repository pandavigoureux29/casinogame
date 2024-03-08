using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UIColorSelector;

public class UIColorSelector : MonoBehaviour
{

    [SerializeField]
    private BetManager m_betManager;
    [SerializeField]
    private Image m_colorSelectedImage;
    [SerializeField]
    private List<ColorData> m_colorsData;

    public void OnGreenClicked()
    {
        m_betManager.ChangeBetColor(BetManager.EColor.GREEN);
    }

    public void OnRedClicked()
    {
        m_betManager.ChangeBetColor(BetManager.EColor.RED);
    }

    public void RefreshColor(BetManager.EColor eColor)
    {
        var colorData = m_colorsData.FirstOrDefault(x=>x.ColorEnum == eColor);
        m_colorSelectedImage.color = colorData.Color;
    }

    [Serializable]
    public class ColorData
    {
        public Color Color;
        public BetManager.EColor ColorEnum;
    }

}
