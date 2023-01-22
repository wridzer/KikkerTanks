using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public struct InputUpdate : INetworkSerializable
{
	public float horizontal, vertical, mouseX, mouseY;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref horizontal);
        serializer.SerializeValue(ref vertical);
        serializer.SerializeValue(ref mouseX);
        serializer.SerializeValue(ref mouseY);
    }
}

public class NetworkedPlayer : NetworkBehaviour, IDamageble
{
    [HideInInspector] public bool isTurn;

    [Header("Player Settings")]
    [SerializeField] private float health;
    [SerializeField] private float minPower;
    [SerializeField] private float maxPower;
    [SerializeField] private float lineAlphaModifier;
    [SerializeField] private float moveSpeed;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject launchPoint;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpawnPointsInfo spawnPoints;

    private LineRenderer lineRenderer;
    private Vector2 aimPos;
    private float lineAlphaBegin = 0;
    private float lineAlphaEnd = 0;
    private float power;
    private bool alphaGoingUp = true;
    private int gameEnded = 0;

    public float Health { get; set; }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Health = health;
        HealthClientRpc();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 10f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, launchPoint.transform.position);

        transform.position = spawnPoints.spawnPointList[(int)OwnerClientId];
        if ((int)OwnerClientId > (spawnPoints.spawnPointList.Count * 0.5f) - 1f)
        {
            transform.GetComponent<SpriteRenderer>().flipX = true;
            launchPoint.transform.localPosition = new Vector3(
                launchPoint.transform.localPosition.x * -1,
                launchPoint.transform.localPosition.y,
                launchPoint.transform.localPosition.z);
        }
        if (IsOwner)
        {
            CheckNumberOfPlayersServerRpc();
        }
    }



    // Update is called once per frame
    void Update()
    {
        HealthClientRpc();

        // Check if client is object owner and is their turn
        if (!IsOwner || !isTurn) return;

        aimPos = Input.mousePosition;

        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        lineRenderer.SetPosition(0, launchPoint.transform.position);

        if (Input.GetMouseButtonDown(0))
        {
            lineRenderer.enabled = true;
        }
        if (Input.GetMouseButton(0))
        {
            lineRenderer.SetPosition(1, aimPos);
            power = GetPower();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Shoot(power);
        }

        NetworkVariable<InputUpdate> inputUpdate = new NetworkVariable<InputUpdate>(
            new InputUpdate
            {
                horizontal = moveDir.x,
                vertical = moveDir.y,
                mouseX = aimPos.x,
                mouseY = aimPos.y,
            }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    }

    private float GetPower()
    {
        if (alphaGoingUp)
        {
            if (lineAlphaBegin < 1)
            {
                lineAlphaBegin += lineAlphaModifier;
            }
            else
            {
                lineAlphaEnd += lineAlphaModifier;
                if (lineAlphaEnd > 1) { alphaGoingUp = false; }
            }
        }
        if (!alphaGoingUp)
        {
            if (lineAlphaEnd > 0)
            {
                lineAlphaEnd -= lineAlphaModifier;
            }
            else
            {
                lineAlphaBegin -= lineAlphaModifier;
                if (lineAlphaBegin < 0) { alphaGoingUp = true; }
            }
        }
        lineRenderer.startColor = new Color(255, 255, 0, lineAlphaBegin);
        lineRenderer.endColor = new Color(255, 0, 0, lineAlphaEnd);

        return (lineAlphaBegin + lineAlphaEnd) / 2 * (maxPower - minPower);
    }

    void Shoot(float _power)
    {
        lineRenderer.enabled = false;
        lineAlphaBegin = 0;
        lineAlphaEnd = 0;
        Vector2 newRot = (Input.mousePosition - launchPoint.transform.position).normalized;
        SpawnServerRpc(newRot.x, newRot.y, _power);
        NextPlayerServerRpc(NetworkManager.LocalClientId);
    }

    [ClientRpc]
    public void TakeDamageClientRpc(float _Damage)
    {
        Health -= _Damage;
        HealthClientRpc();
        if (Health <= 0)
        {
            if (IsOwner) GetScoresServerRpc();
        }
    }

    private void EndGame(float _score)
    {
        FindObjectOfType<NetworkUI>().EndGame(_score);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnServerRpc(float rotX, float rotY, float _power)
    {
        Vector2 newRot = new Vector2(rotX, rotY);
        GameObject rocket = Instantiate(
            bulletPrefab,
            launchPoint.transform.position,
            Quaternion.identity,
            this.transform
            );
        rocket.GetComponent<Rigidbody2D>().AddForce(newRot * _power);
        rocket.GetComponent<NetworkObject>().Spawn(true);
    }

    [ClientRpc]
    private void HealthClientRpc()
    {
        healthText.text = Health.ToString();
    }

    //GameManager

    [SerializeField] private int numberOfPlayers;

    [ServerRpc(RequireOwnership = false)]
    private void CheckNumberOfPlayersServerRpc()
    {
        if (NetworkManager.ConnectedClients.Count != numberOfPlayers) { Debug.Log("not enough players yet, number of player is: " + NetworkManager.ConnectedClients.Count); return; }

        // StartGame
        ulong startPlayerI = NetworkManager.ConnectedClientsIds[0];
        NetworkManager.ConnectedClients[startPlayerI].PlayerObject.GetComponent<NetworkedPlayer>().isTurn = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void NextPlayerServerRpc(ulong clientId)
    {
        NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<NetworkedPlayer>().isTurn = false;
        int playerIndex = ((int)clientId + 1) % numberOfPlayers;
        ulong nextPlayerI = NetworkManager.ConnectedClientsIds[playerIndex];
        SetPlayersActiveClientRpc(nextPlayerI);

    }
    [ClientRpc]
    private void SetPlayersActiveClientRpc(ulong _id)
    {
        if (_id == NetworkManager.LocalClientId)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<NetworkedPlayer>().isTurn = true;
        } else { NetworkManager.LocalClient.PlayerObject.GetComponent<NetworkedPlayer>().isTurn = false; }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetScoresServerRpc()
    {
        foreach (var clientId in NetworkManager.ConnectedClientsIds)
        {
            Debug.Log(clientId);
            float score = NetworkManager.ConnectedClients[clientId].PlayerObject.GetComponent<NetworkedPlayer>().health;
            foreach (var otherClientId in NetworkManager.ConnectedClientsIds)
            {
                if (otherClientId != clientId)
                {
                    score += (100f - NetworkManager.ConnectedClients[otherClientId].PlayerObject.GetComponent<NetworkedPlayer>().health);
                }
            }
            SetScoreClientRpc(clientId, score);
        }

        StartCoroutine(WaitForEndGame(3)); // need to wait for clientRPC to finish
    }

    [ClientRpc]
    private void SetScoreClientRpc(ulong _id, float _score)
    {
        if (_id == NetworkManager.LocalClientId)
        {
            if (_score == 0) { _score = -1; } // Minimum score of 1
            Debug.Log(_score + " : " + _id);
            NetworkManager.LocalClient.PlayerObject.GetComponent<NetworkedPlayer>().EndGame(_score);
        }
    }

    [ClientRpc]
    private void ShutdownClientRpc()
    {
        NetworkManager.Singleton.Shutdown();
    }

    private IEnumerator WaitForEndGame(int sec)
    {
        yield return new WaitForSeconds(sec);
        ShutdownClientRpc();
    }
}