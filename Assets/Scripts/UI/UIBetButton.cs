using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBetButton : MonoBehaviour
{
    [SerializeField]
    private GameManager m_gameManager;
    [SerializeField]
    private UIIncrementers m_uiIncrementers;

    private Button m_button;

    private bool m_betDeclared = false;

    private void Awake()
    {
        m_button = GetComponent<Button>();
        m_gameManager.BetManager.OnBetConfirmed += OnBetConfirmed;
        m_gameManager.BetManager.OnBetDeclared += OnBetDeclared;
        m_gameManager.BetManager.OnBetColorChanged += OnBetColorChanged;
        m_gameManager.BetManager.OnBetChipQuantityChanged += OnBetChipQuantityChanged;
        UpdateBetButton();
    }

    private void OnDestroy()
    {
        if(m_gameManager != null)
        {
            m_gameManager.BetManager.OnBetConfirmed -= OnBetConfirmed;
            m_gameManager.BetManager.OnBetDeclared -= OnBetDeclared;
            m_gameManager.BetManager.OnBetColorChanged -= OnBetColorChanged;
            m_gameManager.BetManager.OnBetChipQuantityChanged -= OnBetChipQuantityChanged;
        }
    }
    private void OnBetConfirmed(bool isBetWon, BetManager.EColor color)
    {
        m_betDeclared = false;
        UpdateBetButton();
    }

    private void OnBetColorChanged()
    {
        UpdateBetButton();
    }

    private void OnBetChipQuantityChanged(string userId, string tokenId, int totalBetIncrements)
    {
        UpdateBetButton();
    }

    private void OnBetDeclared(string userId)
    {
        if(m_gameManager.GetLocalPlayerId() == userId)
        {
            m_betDeclared = true;
            UpdateBetButton();
        }
    }

    public void UpdateBetButton()
    {
        if (m_button == null)
        {
            return;
        }

        bool toggleButton = m_gameManager.BetManager.IsColorSelected && m_uiIncrementers.HasAddedChips && !m_betDeclared;
        m_button.interactable = toggleButton;
    }
}
