using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetStack : ChipStack
{
    public override void Refresh(string chipId, int quantity)
    {
        var chipData = DatabaseManager.Instance.GetChipData(chipId);
        if(m_chips.Count <= 0)
        {
            AddChips(chipId, quantity);
            return;
        }

        int currentQuantity = 0;
        //remove exceeding quantity
        for(int i = m_chips.Count-1; i >= 0; i--)
        {
            if (m_chips[i].ChipId == chipData.Id)
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

        AddChips(chipId, quantity - currentQuantity);
        ReplaceAll();
    }
}
