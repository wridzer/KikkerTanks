using System.Collections;
using Unity.Netcode;
using UnityEngine;


public interface IDamageble
{
    float Health { get; set; }

    [ClientRpc]
    void TakeDamageClientRpc(float _Damage);
}