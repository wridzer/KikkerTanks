using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "My Assets/SpawnPointsInfo")]
public class SpawnPointsInfo : ScriptableObject
{
    public List<Vector2> spawnPointList = new List<Vector2>();
}
