using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITokenInventory : MonoBehaviour
{
    List<UIToken> m_tokens;

    [SerializeField]
    private GameManager m_gameManager;
    [SerializeField]
    private UIToken m_tokenPrefab;

    private void Awake()
    {
        m_gameManager.OnInventoriesInitialized += OnInventoriesInitialized;
    }

    private void OnInventoriesInitialized(PlayerInventory localInventory, PlayerInventory otherInventory)
    {
        foreach (var token in localInventory.Inventory.tokens)
        {
            var go = Instantiate(m_tokenPrefab, transform);
            var uiToken = go.GetComponent<UIToken>();
            uiToken.InitializeToken(token);
        }
    }
}
