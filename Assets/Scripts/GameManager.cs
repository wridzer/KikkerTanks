using ChatClientExample;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private int numberOfPlayers;
    [SerializeField]  private NetworkManager netManager;
    private int playerIndex = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CheckNumberOfPlayers();
    }

    private void CheckNumberOfPlayers()
    {
        if(netManager.ConnectedClients.Count != numberOfPlayers) { return; }

        // StartGame
        ulong startPlayerI = netManager.ConnectedClientsIds[playerIndex];
        netManager.ConnectedClients[startPlayerI].PlayerObject.GetComponent<NetworkedPlayer>().isTurn = true;
        Debug.Log(netManager.ConnectedClients[startPlayerI].PlayerObject);
        Debug.Log(playerIndex);
    }

    public void NextPlayer(ulong clientId)
    {
        netManager.ConnectedClients[clientId].PlayerObject.GetComponent<NetworkedPlayer>().isTurn = false;
        ulong nextPlayerI = netManager.ConnectedClientsIds[playerIndex++];
        netManager.ConnectedClients[nextPlayerI].PlayerObject.GetComponent<NetworkedPlayer>().isTurn = true;
    }
}
