using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private int numberOfPlayers;
    private int playerIndex = 0;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    private void CheckNumberOfPlayersServerRpc()
    {
        if(NetworkManager.ConnectedClients.Count != numberOfPlayers) { Debug.Log("not enough players yet, number of player is: " + NetworkManager.ConnectedClients.Count); return; }

        // StartGame
        ulong startPlayerI = NetworkManager.ConnectedClientsIds[playerIndex];
        NetworkManager.ConnectedClients[startPlayerI].PlayerObject.GetComponent<NetworkedPlayer>().isTurn = true;
        Debug.Log(NetworkManager.ConnectedClients[startPlayerI].PlayerObject);
        Debug.Log(playerIndex);
    }

    [ServerRpc]
    public void NextPlayerServerRpc(ulong clientId)
    {
        NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<NetworkedPlayer>().isTurn = false;
        ulong nextPlayerI = NetworkManager.ConnectedClientsIds[playerIndex++];
        SetPlayersActive(NetworkManager.ConnectedClients[nextPlayerI].PlayerObject.transform);
    }

    private void SetPlayersActive(Transform _selectedPlayer)
    {
        //loop players and set selected active
        foreach(var player in NetworkManager.ConnectedClients)
        {
            if(player.Value.PlayerObject.transform ==  _selectedPlayer) { player.Value.PlayerObject.GetComponent<NetworkedPlayer>().isTurn = true; }
            else { player.Value.PlayerObject.GetComponent<NetworkedPlayer>().isTurn = false; }
        }
    }
}
