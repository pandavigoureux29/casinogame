using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;

public class NetworkController : MonoBehaviourPunCallbacks
{

	public PhotonView playerPrefab;

	[SerializeField]
	private GameManager m_gameManager;

	// Start is called before the first frame update
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinRandomOrCreateRoom();
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined a room.");
		PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
	}

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("Other Player Joined this room.");
		if (PhotonNetwork.IsMasterClient)
		{
			m_gameManager.StartGame();

        }
    }

}