using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChipStack : MonoBehaviour
{
    [SerializeField]
    private GameObject m_chipPrefab;
    [SerializeField]
    private Transform m_chipContainer;
    [SerializeField]
    private float m_spacing = 0.05f;

    private ChipData m_chipData;
    public ChipData ChipData => m_chipData;

    protected List<Chip> m_chips = new List<Chip>();
    protected ChipsPool m_pool = new ChipsPool();

    public int ActiveChips => m_chips.Count;

    private void Awake()
    {
        m_pool.Initialize(m_chipPrefab, m_chipContainer);
    }

    public void AddChips(ChipData chipData, int quantity)
    {
        if(quantity <= 0)
        {
            return;
        }

        m_chipData = chipData;

        for (int i = 0; i < quantity; i++)
        {
            Chip chip = m_pool.TakeChipFromPool();
            chip.Refresh(chipData);
            chip.gameObject.SetActive(true);

            //place chip
            chip.transform.localPosition = new Vector3(0, m_chips.Count * m_spacing, 0);
            m_chips.Add(chip);
        }
    }

    public void RemoveChips(ChipData chipData, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            var chip = m_chips[m_chips.Count - 1];
            m_chips.RemoveAt(m_chips.Count-1);
            m_pool.ReleaseToPool(chip);
        }
    }

    public virtual void Refresh(ChipData chipData, int quantity)
    {
        ClearStack();
        AddChips(chipData, quantity);
    }

    public void ClearStack()
    {
        foreach (var chip in m_chips)
        {
            m_pool.ReleaseToPool(chip);
        }
        m_chips.Clear();
    }    

    public void ReplaceAll()
    {
        for(int i = 0; i < m_chips.Count; i++)
        {
            m_chips[i].transform.localPosition = new Vector3(0, i * m_spacing, 0);
        }
    }

    public Transform GetBaseTransform()
    {
        if(m_chips.Count > 0)
        {
            return m_chips[0].transform;
        }
        return transform;
    }
}
