using Fusion;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public abstract class Projectile : NetworkBehaviour
    {
        [SerializeField] protected float _movementSpeed;
        [SerializeField] protected int _damage;

        [Networked] public Vector3 Direction { get; set; }
        [Networked] public PlayerStats OwnerPlayerStats {get;set;}

        public abstract void Throw(Vector3 direction, PlayerStats ownerPlayerStats);
    }
}
