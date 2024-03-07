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

    private PlayerInventory m_inventory;

    public void Initialize(PlayerInventorySO playerInventorySO)
    {
        foreach (var chip in playerInventorySO.Chips)
        {
            GetStack(chip.Id).Refresh(chip, 0);
        }
    }

    public void Initialize(PlayerInventory inventory)
    {
        m_inventory = inventory;
        RefreshFromInventory();
    }

    public void AddChips(ChipData chipData, int quantity)
    {
        ChipStack stack = GetStack(chipData.Id);
        stack.AddChips(chipData, quantity);
    }

    public void RemoveChips(ChipData chipData, int quantity)
    {
        ChipStack stack = GetStack(chipData.Id);
        stack.RemoveChips(chipData, quantity);
    }

    public void RefreshChips(ChipData chipData, int quantity)
    {
        ChipStack stack = GetStack(chipData.Id);
        stack.Refresh(chipData, quantity);
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
            GetStack(chip.Id).Refresh(chip,chip.Quantity);
        }
    }

    public ChipStack GetStack(string id)
    {
        ChipStack stack;
        if (!m_stacks.ContainsKey(id))
        {
            stack = Instantiate(m_chipStackPrefab, transform);
            m_stacks[id] = stack;
            PlaceStack(stack);
        }
        else
        {
            stack = m_stacks[id];
        }
        return stack;
    }
}
