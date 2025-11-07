using Fusion;
using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class IceSpikeProjectile : Projectile
    {
        [SerializeField] private float _range = 0f;
        [SerializeField] private float _dissolveDelay = 0f;
        [SerializeField] private float _slowDuration = 0f;
        [SerializeField] private GameObject _projectileModel = null;
        
        [Networked] public bool ProjectileHit { get; private set; }

        public override void Throw(Vector3 direction, PlayerStats ownerPlayerStats)
        {
            Direction = direction.normalized * _movementSpeed;
            OwnerPlayerStats = ownerPlayerStats;
            transform.rotation = Quaternion.FromToRotation(transform.forward, Direction.normalized);
        }

        public override void FixedUpdateNetwork()
        {
            if (ProjectileHit) return;

            transform.position += (Direction * Runner.DeltaTime);

            if (!HasStateAuthority) return;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);

            foreach (Collider collider in hitColliders)
            {
                if (collider.tag == "Player")
                {
                    PlayerStats player = collider.GetComponent<PlayerStats>();

                    if (player.Object.InputAuthority == OwnerPlayerStats.Object.InputAuthority) continue;
                    if (FusionConnection.GameModeType == GameModeType.TDM && player.Team != OwnerPlayerStats.Team)
                    {

                        player.DealDamage(_damage, OwnerPlayerStats);
                        player.ApplySlow(_slowDuration);
                    }

                    ProjectileHit = true;
                    _projectileModel.transform.parent = player.transform;
                    RPC_LockModelToPlayer(_projectileModel.transform.localPosition, player);
                    Destroy(gameObject, _dissolveDelay);

                    break;
                }

                if (hitColliders.Any(collider => collider.tag == "Ground"))
                {
                    ProjectileHit = true;
                    Destroy(gameObject, _dissolveDelay);
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_LockModelToPlayer(Vector3 localPosition, PlayerStats player)
        {
            _projectileModel.transform.parent = player.transform;
            _projectileModel.transform.localPosition = localPosition;
            Destroy(_projectileModel.gameObject, _dissolveDelay);
        }


        //This code can be used for testing hit range
        //private void Update()
        //{
        //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);
        //    if (hitColliders.Any((collider) => collider.tag == "Ground")) Debug.Log("In range");
        //}
    }    
}
