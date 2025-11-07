using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class HealingPointSpawner : Singleton<HealingPointSpawner>
    {
        [SerializeField] private HealingPoint _healingPointPrefab = null;
        [SerializeField] private List<Transform> _spawnLocations = null;

        public void SpawnHealingPoints(NetworkRunner runner)
        {
            foreach(Transform location in _spawnLocations)
            {
                HealingPoint healingPoint = runner.Spawn(_healingPointPrefab, location.position, inputAuthority: runner.LocalPlayer);
            }
        }
    }
}
