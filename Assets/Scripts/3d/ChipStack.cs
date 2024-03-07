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

    private string m_Id;
    public string Id => m_Id;

    private ChipData m_chipData;
    public ChipData ChipData => m_chipData;

    private List<Chip> m_chips = new List<Chip>();
    private List<Chip> m_chipsPool = new List<Chip>();

    public int ActiveChips => m_totalChipsCount;

    private int m_totalChipsCount = 0;

    public void AddChips(ChipData chipData, int quantity)
    {
        m_Id = chipData.Id;
        m_chipData = chipData;

        for (int i = 0; i < quantity; i++)
        {
            Chip chip = TakeChipFromPool();
            chip.Refresh(chipData);
            chip.gameObject.SetActive(true);

            //place chip
            chip.transform.localPosition = new Vector3(0, m_totalChipsCount * m_spacing, 0);
            m_chips.Add(chip);

            m_totalChipsCount++;
        }
    }

    public void RemoveChips(ChipData chipData, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            var chip = m_chips[m_chips.Count - 1];
            m_chips.RemoveAt(m_chips.Count-1);
            ReleaseToPool(chip);

            m_totalChipsCount--;
        }
    }

    public void Refresh(ChipData chipData, int quantity)
    {
        ClearStack();
        AddChips(chipData, quantity);
    }

    public void ClearStack()
    {
        foreach (var chip in m_chips)
        {
            ReleaseToPool(chip);
        }
        m_chips.Clear();
        m_totalChipsCount = 0;
    }

    private Chip TakeChipFromPool()
    {
        if (m_chipsPool.Count == 0)
        {
            var go = Instantiate(m_chipPrefab, m_chipContainer);
            return go.GetComponent<Chip>();
        }

        var chip = m_chipsPool[0];
        m_chipsPool.RemoveAt(0);
        return chip;
    }

    private void ReleaseToPool(Chip chip)
    {
        chip.gameObject.SetActive(false);
        chip.transform.SetAsLastSibling();
        m_chipsPool.Add(chip);
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
