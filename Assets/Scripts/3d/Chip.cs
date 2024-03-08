using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer m_renderer;

    private string m_chipId;
    public string ChipId => m_chipId;

    public void Refresh(ChipData chipData)
    {
        m_chipId = chipData.Id;
        //all chips can share the same material as there nothing in the design
        //that tells us that a single chip will need a unique instantiated material
        m_renderer.sharedMaterial = chipData.Material;
    }
}
