using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private ReversableChip m_chipPrefab;
    [SerializeField]
    private Transform m_container;
    [SerializeField]
    private GameObject m_selectorObject;

    [SerializeField]
    private int m_gridSize = 8;

    [SerializeField]
    private float m_chipSpacing = 2;

    private List<ReversableChip> m_chips;

    private List<int> m_greenIndexes;
    public List<int> GreenIndexes => m_greenIndexes;

    public void InstantiateGrid(List<int> greenIndexes = null)
    {
        if(greenIndexes == null)
        {
            GenerateGrid();
        }
        else
        {
            m_greenIndexes = greenIndexes.ToList();
        }

        m_chips = new List<ReversableChip> (); 

        var currentGreenIndex = 0;
        Vector3 chipPosition = Vector3.zero;

        for (int i=0; i < m_gridSize * m_gridSize; i++)
        {
            //instantiate chip
            var go = Instantiate(m_chipPrefab, m_container);
            ReversableChip chip = go.GetComponent<ReversableChip>();
            chip.Index = i;

            //set its color
            if (currentGreenIndex< m_greenIndexes.Count && m_greenIndexes[currentGreenIndex] == i)
            {
                currentGreenIndex++;
                chip.SetColor(ReversableChip.EColor.GREEN);
            }
            else
            {
                chip.SetColor(ReversableChip.EColor.RED);
            }

            //place it on the grid
            int row = i / m_gridSize;
            int column = i % m_gridSize;

            chipPosition.x = column * m_chipSpacing;
            chipPosition.z = row * m_chipSpacing;
            go.transform.localPosition = chipPosition;

            m_chips.Add( chip );
        }
    }

    private void GenerateGrid()
    {
        m_greenIndexes = new List<int>();
        for (int i = 0; i < m_gridSize * m_gridSize; i++)
        {
            var r = Random.Range(0, 99);
            if(r < 50)
            {
                m_greenIndexes.Add(i);
            }
        }
    }

    public ReversableChip GetClickedChip()
    {
        Ray ray;
        RaycastHit hit;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit))
        {
            var go = hit.collider.gameObject;
            var chip = go.GetComponent<ReversableChip>();
            if(chip != null && !chip.IsFlipped)
                return chip;
        }
        return null;
    }

    public bool SelectChip(int index)
    {
        if(index >= m_chips.Count) 
            return false;

        var chip = m_chips[index];

        m_selectorObject.SetActive(true);
        m_selectorObject.transform.position = chip.transform.position;

        return !chip.IsFlipped;
    }

    public bool CheckChip(int index, ReversableChip.EColor color)
    {
        if (index < 0 || index >= m_chips.Count)
            return false;

        var chip = m_chips[index];

        return !chip.IsFlipped && chip.Color == color;
    }

    public void FlipChip(int index)
    {
        m_selectorObject.SetActive(false);

        if (index < m_chips.Count)
        {
            m_chips[index].Flip();
        }
    }
}
