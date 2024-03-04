using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private ReversableChip m_chipPrefab;

    [SerializeField]
    private int m_gridSize = 8;

    [SerializeField]
    private float m_chipSpacing = 2;

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

        var currentGreenIndex = 0;
        Vector3 chipPosition = Vector3.zero;

        for (int i=0; i < m_gridSize * m_gridSize; i++)
        {
            //instantiate chip
            var go = Instantiate(m_chipPrefab,transform);
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
}