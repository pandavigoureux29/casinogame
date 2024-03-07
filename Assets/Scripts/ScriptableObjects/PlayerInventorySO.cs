using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerInventory", order = 1)]
public class PlayerInventorySO : ScriptableObject
{
    public List<ChipData> Chips;

    public ChipData GetChipData(string chipId)
    {
        return Chips.FirstOrDefault(x=>x.Id == chipId);
    }
}
