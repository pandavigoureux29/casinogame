using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITurnDisplay : MonoBehaviour
{
    [SerializeField]
    private GameManager m_gameManager;
    [SerializeField]
    private TextMeshProUGUI m_turnText;

    private void Awake()
    {
        m_gameManager.OnTurnChanged += OnTurnChanged;
    }

    private void OnDestroy()
    {
        m_gameManager.OnTurnChanged -= OnTurnChanged;
    }

    private void OnTurnChanged(string userId)
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber.ToString() == userId)
        {
            m_turnText.text = "Your Turn";
        }
        else
        {
            m_turnText.text = "Oppponent's Turn";
        }
    }
}
