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

    public PlayerInventory(string userId, PlayerInventorySO playerInventorySO)
    {
        m_userId = userId;
        m_inventory = playerInventorySO;
    }

    public int GetQuantity(string tokenId)
    {
        var token = m_inventory.tokens.FirstOrDefault(x => x.Id == tokenId);
        if (token == null)
            return 0;
        return token.Quantity;
    }

    public bool CanBetMore(string tokenId, int currentBet)
    {
        return GetQuantity(tokenId) - currentBet >= BetManager.S_BET_INCREMENTS;
    }

    public void UpdateQuantities(Dictionary<string, int> m_betTokensCount, bool win)
    {
        foreach (var tokenStackCount in m_betTokensCount)
        {
            var token = m_inventory.tokens.FirstOrDefault(x=>x.Id == tokenStackCount.Key);
            if(token == null)
                continue;

            int earnings = tokenStackCount.Value * BetManager.S_BET_INCREMENTS;
            if(!win)
                earnings *= -1;

            token.Quantity += earnings;
        }
    }

    public void GetTokensForNetwork(out List<string> keys, out List<int> values)
    {
        keys = new List<string>();
        values = new List<int>();

        foreach (var token in m_inventory.tokens)
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
            var token = m_inventory.tokens.FirstOrDefault(x => x.Id == tokenkey);
            if(token != null)
            {
                token.Quantity = values[i];
            }
        }
    }
}
