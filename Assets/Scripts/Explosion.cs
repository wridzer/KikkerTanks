using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class Explosion : NetworkBehaviour
{
    [SerializeField] private float explosionTime = 0.5f;
    private float size;

    private void OnEnable()
    {
        size = transform.localScale.x;
    }

    private void Update()
    {
        size = transform.localScale.x;
        size -= explosionTime * size * Time.deltaTime;
        transform.localScale = new Vector3(size, size, size);

        if (size <= 0.1)
        {
            DestroyServerRpc();
        }
    }

    [ServerRpc]
    private void DestroyServerRpc()
    {
        NetworkObject.Destroy(gameObject);
    }
}