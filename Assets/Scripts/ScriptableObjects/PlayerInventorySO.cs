using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerInventory", order = 1)]
public class PlayerInventorySO : ScriptableObject
{
    public List<ChipInventoryData> Chips;

    public ChipInventoryData GetChipData(string chipId)
    {
        return Chips.FirstOrDefault(x=>x.Id == chipId);
    }
}
