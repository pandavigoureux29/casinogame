using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipsPool
{
    private GameObject m_chipPrefab;
    private Transform m_chipContainer;

    private List<Chip> m_chipsPool = new List<Chip>();

    public void Initialize(GameObject chipPrefab, Transform container)
    {
        m_chipContainer = container;
        m_chipPrefab = chipPrefab;
    }

    public Chip TakeChipFromPool()
    {
        if (m_chipsPool.Count == 0)
        {
            var go = GameObject.Instantiate(m_chipPrefab, m_chipContainer);
            return go.GetComponent<Chip>();
        }

        var chip = m_chipsPool[0];
        m_chipsPool.RemoveAt(0);
        return chip;
    }

    public void ReleaseToPool(Chip chip)
    {
        chip.gameObject.SetActive(false);
        chip.transform.SetAsLastSibling();
        m_chipsPool.Add(chip);
    }
}
