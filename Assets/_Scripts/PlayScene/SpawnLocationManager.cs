using System.Collections.Generic;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class SpawnLocationManager : Singleton<SpawnLocationManager>
    {
        [SerializeField] private List<Transform> _spawnLocations = null;


        public Vector3 GetRandomSpawnLocation()
        {
            int index = Random.Range(0, _spawnLocations.Count);
            return _spawnLocations[index].position;
        }
    }
}
