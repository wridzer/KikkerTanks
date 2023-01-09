using System.Collections;
using UnityEngine;


public interface IDamageble
{
    float Health { get; set; }

    void TakeDamage(float _Damage);
}