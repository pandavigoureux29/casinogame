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
    public int TotalChipsCount => m_totalChipsCount;

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
        return GetQuantity(chipId) - currentBet > 0;
    }

    public void UpdateQuantities(Dictionary<string, int> betChipIncrementsCount)
    {
        foreach (var chipIncrementsCount in betChipIncrementsCount)
        {
            var chip = m_inventory.Chips.FirstOrDefault(x=>x.Id == chipIncrementsCount.Key);
            if(chip == null)
                continue;

            chip.Quantity += chipIncrementsCount.Value;
            m_totalChipsCount += chipIncrementsCount.Value;
        }
    }

    public void GetChipsForNetwork(out List<string> keys, out List<int> values)
    {
        keys = new List<string>();
        values = new List<int>();

        foreach (var chip in m_inventory.Chips)
        {
            keys.Add(chip.Id);
            values.Add(chip.Quantity);
        }
    }

    public void UpdateChipsFromNetwork(string[] keys, int[] values)
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
