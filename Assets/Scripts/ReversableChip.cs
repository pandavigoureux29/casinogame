using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversableChip : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer m_quadRenderer;

    [SerializeField]
    private Material m_redMaterial;
    [SerializeField]
    private Material m_greenMaterial;

    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private int m_index = -1;
    public int Index
    {
        get { return m_index; }
        set { m_index = value; }
    }

    private BetManager.EColor m_color;
    public BetManager.EColor Color => m_color;

    private bool m_isFlipped;
    public bool IsFlipped => m_isFlipped;

    public void SetColor(BetManager.EColor color)
    {
        m_color = color;
        if(color == BetManager.EColor.GREEN)
        {
            m_quadRenderer.material = m_greenMaterial;
        }
        else
        {
            m_quadRenderer.material = m_redMaterial;
        }
    }

    public void Flip()
    {
        m_isFlipped = true;
        m_animator.SetTrigger("Flip");
    }

    public void Unflip()
    {
        m_isFlipped = false;
        m_animator.SetTrigger("Unflip");
    }
}
