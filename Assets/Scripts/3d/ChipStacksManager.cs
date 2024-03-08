using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipStacksManager : MonoBehaviour
{
    [SerializeField] 
    ChipStack m_chipStackPrefab;
    [SerializeField]
    private float m_spacing = 1;

    private Dictionary<string,ChipStack> m_stacks = new Dictionary<string, ChipStack>();
    public Dictionary<string,ChipStack> Stacks => m_stacks;

    private PlayerInventory m_inventory;
    public PlayerInventory Inventory => m_inventory;

    public void Initialize(PlayerInventory inventory)
    {
        m_inventory = inventory;
        if(m_inventory == null)
        {
            return;
        }

        foreach (var chip in m_inventory.Inventory.Chips)
        {
            var stack = Instantiate(m_chipStackPrefab, transform);
            m_stacks[chip.Id] = stack;
            PlaceStack(stack);
            stack.Initialize(chip);
        }

        RefreshFromInventory();
    }

    public void RefreshChips(string chipId, int quantity)
    {
        m_stacks[chipId].Refresh(chipId, quantity);
    }

    public void Clear()
    {
        foreach(var stack in m_stacks.Values)
        {
            stack.ClearStack();
        }
    }

    private void PlaceStack(ChipStack stack)
    {
        var index = Mathf.Floor( m_stacks.Count / 2 );
        float x = index * m_spacing;
        if (m_stacks.Count % 2 == 0)
        {
            x *= -1;
        }

        Vector3 pos = new Vector3(x, 0, 0);
        stack.transform.localPosition = pos;
    }

    public void RefreshFromInventory()
    {
        if(m_inventory == null)
        {
            return;
        }

        foreach(var chip in m_inventory.Inventory.Chips)
        {
            m_stacks[chip.Id].Refresh(chip.Id,chip.Quantity);
        }
    }

}
