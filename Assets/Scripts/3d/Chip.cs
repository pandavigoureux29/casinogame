using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer m_renderer;

    private string m_tokenId;
    public string TokenId => m_tokenId;

    public void Refresh(ChipData chipData)
    {
        m_tokenId = chipData.Id;
        m_renderer.material = chipData.Material;
    }
}
