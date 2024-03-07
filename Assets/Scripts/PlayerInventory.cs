using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory
{
    private string m_userId;
    public string UserId => m_userId;

    private PlayerInventorySO m_inventory;
    public PlayerInventorySO Inventory => m_inventory;

    private int m_totalChipsCount = 0;
    public int TotalTokensCount => m_totalChipsCount;

    public PlayerInventory(string userId, PlayerInventorySO playerInventorySO)
    {
        m_userId = userId;
        m_inventory = playerInventorySO;
        m_totalChipsCount = playerInventorySO.Chips.Sum(x=> x.Quantity);
    }

    public int GetQuantity(string chipId)
    {
        var chip = m_inventory.Chips.FirstOrDefault(x => x.Id == chipId);
        if (chip == null)
            return 0;
        return chip.Quantity;
    }

    public bool CanBetMore(string chipId, int currentBet)
    {
        return GetQuantity(chipId) - currentBet >= BetManager.S_BET_INCREMENTS;
    }

    public void UpdateQuantities(Dictionary<string, int> betChipIncrementsCount, bool win)
    {
        foreach (var chipIncrementsCount in betChipIncrementsCount)
        {
            var chip = m_inventory.Chips.FirstOrDefault(x=>x.Id == chipIncrementsCount.Key);
            if(chip == null)
                continue;

            int earnings = chipIncrementsCount.Value * BetManager.S_BET_INCREMENTS;
            if(!win)
                earnings *= -1;

            chip.Quantity += earnings;
            m_totalChipsCount += earnings;
        }
    }

    public void GetTokensForNetwork(out List<string> keys, out List<int> values)
    {
        keys = new List<string>();
        values = new List<int>();

        foreach (var token in m_inventory.Chips)
        {
            keys.Add(token.Id);
            values.Add(token.Quantity);
        }
    }

    public void UpdateTokensFromNetwork(string[] keys, int[] values)
    {
        for(int i = 0; i < keys.Length; i++)
        {
            var chipKey = keys[i];
            var chip = m_inventory.Chips.FirstOrDefault(x => x.Id == chipKey);
            if(chip != null)
            {
                int lastvalue = chip.Quantity;
                chip.Quantity = values[i];

                m_totalChipsCount += chip.Quantity - lastvalue;
            }
        }
    }

    public void Reset(PlayerInventorySO playerInventorySO)
    {
        m_totalChipsCount = 0;
        for(int i=0; i < playerInventorySO.Chips.Count; i++)
        {
            m_inventory.Chips[i].Quantity = playerInventorySO.Chips[i].Quantity;
            m_totalChipsCount += m_inventory.Chips[i].Quantity;
        }
    }
}
