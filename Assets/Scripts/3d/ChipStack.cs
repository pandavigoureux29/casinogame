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

    private Dictionary<string, List<Chip>> m_chips = new Dictionary<string, List<Chip>>();

    private List<Chip> m_chipsPool = new List<Chip>();

    public int ActiveChips => m_totalChipsCount;

    private int m_totalChipsCount = 0;

    public void AddChips(ChipData chipData, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            Chip chip = TakeChipFromPool();
            chip.Refresh(chipData);
            chip.gameObject.SetActive(true);


            if (!m_chips.ContainsKey(chipData.Id))
            {
                m_chips[chipData.Id] = new List<Chip>();
            }

            //place chip
            chip.transform.localPosition = new Vector3(0, m_totalChipsCount * m_spacing, 0);
            m_chips[chipData.Id].Add(chip);

            m_totalChipsCount++;
        }
    }

    public void RemoveChips(ChipData chipData, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            var list = m_chips[chipData.Id];
            var tokenImage = list.LastOrDefault();
            if(tokenImage != null)
            {
                list.Remove(tokenImage);
                ReleaseToPool(tokenImage);

                m_totalChipsCount--;
            }
        }
    }

    public void Refresh(ChipData chipData, int quantity)
    {
        ClearStack();
        AddChips(chipData, quantity);
    }

    public void ClearStack()
    {
        foreach (string key in m_chips.Keys)
        {
            var list = m_chips[key];
            foreach (Chip image in list)
            {
                ReleaseToPool(image);
            }
            list.Clear();
        }
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
}
