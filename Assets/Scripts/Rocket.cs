using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Rocket : NetworkBehaviour
{
    [SerializeField] private float damage, damageModifier, range, lifeTime = 5;
    [SerializeField] private GameObject explosionPrefab;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 v2Pos = new Vector2(transform.position.x, transform.position.y);
        Vector2 dir = v2Pos + rb.velocity.normalized;
        float angle = Mathf.Atan2(dir.y - v2Pos.y, dir.x - v2Pos.x) * 180 / Mathf.PI;
        transform.localRotation = Quaternion.Euler(0, 0, angle + -90);
        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0)
        {
            DestroyServerRpc();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DestroyServerRpc();
        ExplodeServerRpc(collision.contacts[0].point);
    }

    [ServerRpc]
    private void ExplodeServerRpc(Vector3 _pos)
    {
        //player take damage (damge = damage - distmodifier^distance)
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = new Vector3(_pos.x, _pos.y, -1);
        explosion.transform.localScale = new Vector3(range, range, range);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        RaycastHit2D[] hit;

        hit = Physics2D.CircleCastAll(_pos, range, Vector3.zero);
        foreach (RaycastHit2D raycastHit in hit)
        {
            Debug.Log(raycastHit.transform.name);
            raycastHit.collider.GetComponent<IDamageble>()?.TakeDamageClientRpc(damage);
        }
    }

    [ServerRpc]
    private void DestroyServerRpc()
    {
        NetworkObject.Destroy(gameObject);
    }
}
