using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    private static DatabaseManager m_instance;
    public static DatabaseManager Instance
    {
        get { 
            if(m_instance == null)
            {
                var obj = Resources.Load("DatabaseManager");
                var go = Instantiate(obj);
                m_instance = go.GetComponent<DatabaseManager>();
            }
            return m_instance; 
        }
    }

    [SerializeField]
    private ChipsDataSO m_chipsData;

    private void Awake()
    {
        m_instance = this;
    }

    public ChipData GetChipData(string chipId)
    {
        return m_chipsData.GetChipData(chipId);
    }
}
