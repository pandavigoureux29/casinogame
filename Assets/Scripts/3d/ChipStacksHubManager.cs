using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChipStacksHubManager : MonoBehaviour
{
    [SerializeField] 
    private GameManager m_gameManager;
    [SerializeField]
    private ChipStacksManager m_localInventoryStacks;
    [SerializeField]
    private ChipStacksManager m_otherInventoryStacks;

    private void Awake()
    {
        m_gameManager.OnInventoriesInitialized += OnInventoriesInitialized;
        m_gameManager.OnInventoryUpdated += OnInventoryUpdated;
    }

    private void OnDestroy()
    {
        if(m_gameManager != null)
        {
            m_gameManager.OnInventoriesInitialized -= OnInventoriesInitialized;
            m_gameManager.OnInventoryUpdated -= OnInventoryUpdated;
        }
    }

    private void OnInventoriesInitialized(PlayerInventory localInventory, PlayerInventory otherInventory)
    {
        m_localInventoryStacks.Initialize(localInventory);
        m_otherInventoryStacks.Initialize(otherInventory);
    }

    private void OnInventoryUpdated(PlayerInventory inventory)
    {
        m_localInventoryStacks.RefreshFromInventory();
        m_otherInventoryStacks.RefreshFromInventory();
    }
}
