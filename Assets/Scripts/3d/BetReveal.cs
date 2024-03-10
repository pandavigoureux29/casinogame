using UnityEngine;

public class BetReveal : MonoBehaviour
{
    [SerializeField]
    private BetManager m_betManager;
    [SerializeField]
    private RevealObject m_revealObject;

    private void Awake()
    {

        m_betManager.OnBetConfirmed += OnBetConfirmed;
    }

    private void OnDestroy()
    {

        m_betManager.OnBetConfirmed += OnBetConfirmed;
    }

    private void OnBetConfirmed(bool win, BetManager.EColor betcolor)
    {
        m_revealObject.SetColor(betcolor);
        m_revealObject.Flip();
    }
}
