using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversableChip : MonoBehaviour
{

    public enum EColor { GREEN, RED }

    [SerializeField]
    private MeshRenderer m_quadRenderer;

    [SerializeField]
    private int m_index = -1;
    public int Index
    {
        get { return m_index; }
        set { m_index = value; }
    }

    private EColor m_color;
    public EColor Color => m_color;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(EColor color)
    {
        m_color = color;
    }
}
