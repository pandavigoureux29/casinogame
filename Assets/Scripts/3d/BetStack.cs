using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetStack : ChipStack
{
    [SerializeField]
    private PlayerInventorySO m_inventorySO;

    public override void Refresh(ChipData chipData, int quantity)
    {
        if(m_chips.Count <= 0)
        {
            AddChips(chipData, quantity);
            return;
        }

        int currentQuantity = 0;
        //remove exceeding quantity
        for(int i = m_chips.Count-1; i >= 0; i--)
        {
            if (m_chips[i].TokenId == chipData.Id)
            {
                currentQuantity++;
                if (currentQuantity > quantity)
                {
                    m_pool.ReleaseToPool(m_chips[i]);
                    m_chips.RemoveAt(i);
                    currentQuantity--;
                }
            }
        }

        AddChips(chipData, quantity - currentQuantity);
        ReplaceAll();
    }
}
