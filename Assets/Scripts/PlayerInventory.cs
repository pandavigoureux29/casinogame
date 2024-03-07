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

    private int m_totalTokensCount = 0;
    public int TotalTokensCount => m_totalTokensCount;

    public PlayerInventory(string userId, PlayerInventorySO playerInventorySO)
    {
        m_userId = userId;
        m_inventory = playerInventorySO;
        m_totalTokensCount = playerInventorySO.Chips.Sum(x=> x.Quantity);
    }

    public int GetQuantity(string chipId)
    {
        var token = m_inventory.Chips.FirstOrDefault(x => x.Id == chipId);
        if (token == null)
            return 0;
        return token.Quantity;
    }

    public bool CanBetMore(string chipId, int currentBet)
    {
        return GetQuantity(chipId) - currentBet >= BetManager.S_BET_INCREMENTS;
    }

    public void UpdateQuantities(Dictionary<string, int> m_betTokensCount, bool win)
    {
        foreach (var tokenStackCount in m_betTokensCount)
        {
            var token = m_inventory.Chips.FirstOrDefault(x=>x.Id == tokenStackCount.Key);
            if(token == null)
                continue;

            int earnings = tokenStackCount.Value * BetManager.S_BET_INCREMENTS;
            if(!win)
                earnings *= -1;

            token.Quantity += earnings;
            m_totalTokensCount += earnings;
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
            var tokenkey = keys[i];
            var token = m_inventory.Chips.FirstOrDefault(x => x.Id == tokenkey);
            if(token != null)
            {
                int lastvalue = token.Quantity;
                token.Quantity = values[i];

                m_totalTokensCount += token.Quantity - lastvalue;
            }
        }
    }

    public void Reset(PlayerInventorySO playerInventorySO)
    {
        m_totalTokensCount = 0;
        for(int i=0; i < playerInventorySO.Chips.Count; i++)
        {
            m_inventory.Chips[i].Quantity = playerInventorySO.Chips[i].Quantity;
            m_totalTokensCount += m_inventory.Chips[i].Quantity;
        }
    }
}
