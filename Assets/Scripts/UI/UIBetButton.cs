using UnityEngine;
using UnityEngine.UI;
using static UIColorSelector;

public class UIBetButton : MonoBehaviour
{
    [SerializeField]
    private GameManager m_gameManager;
    [SerializeField]
    private UIIncrementers m_uiIncrementers;
    [SerializeField]
    private UIColorSelector m_colorSelector;

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
        m_colorSelector.RefreshColor(BetManager.EColor.NONE);
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
        m_colorSelector.RefreshColor(BetManager.EColor.NONE);
        m_betDeclared = false;
        UpdateBetButton();
    }

    private void OnBetColorChanged(BetManager.EColor color)
    {
        m_colorSelector.RefreshColor(color);
        UpdateBetButton();
    }

    private void OnBetChipQuantityChanged(string userId, string chipId, int totalBetIncrements)
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
        bool toggleButton = m_gameManager.BetManager.IsColorSelected && m_uiIncrementers.HasAddedChips && !m_betDeclared;
        m_button.interactable = toggleButton;
    }
}
