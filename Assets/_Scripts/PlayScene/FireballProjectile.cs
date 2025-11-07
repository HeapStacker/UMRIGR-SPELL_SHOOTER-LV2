using Fusion;
using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class FireballProjectile : Projectile
    {
        [SerializeField] private float _range = 0f;
        [SerializeField] private float _explosionRange = 0f;
        [SerializeField] private float _explosionDuration = 0f;
        [SerializeField] private GameObject _projectileEffect = null;
        [SerializeField] private GameObject _explosionEffect = null;
        private bool _exploded = false;
        private PlayerStats _hitPlayer = null;

        public override void Throw(Vector3 direction, PlayerStats ownerPlayerStats)
        {
            Direction = direction.normalized * _movementSpeed;
            OwnerPlayerStats = ownerPlayerStats;
            transform.rotation = Quaternion.FromToRotation(transform.forward, Direction.normalized);
        }

        public override void FixedUpdateNetwork()
        {
            if (_exploded) return;

            transform.position += (Direction * Runner.DeltaTime);

            if (!HasStateAuthority) return;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _range);

            foreach (Collider collider in hitColliders)
            {
                if (collider.tag != "Player") continue;

                PlayerStats player = collider.GetComponent<PlayerStats>();

                if (player.Object.InputAuthority == OwnerPlayerStats.Object.InputAuthority) continue;
                if (FusionConnection.GameModeType == GameModeType.TDM && player.Team == OwnerPlayerStats.Team) continue;

                player.DealDamage(_damage, OwnerPlayerStats);
                _hitPlayer = player;
                Explode();

                break;
            }

            if (!_exploded && hitColliders.Any((collider) => collider.tag == "Ground")) Explode();
        }

        private void Explode()
        {
            _exploded = true;
            Debug.Log("Exploded");

            // Ugasiti projektil efekt, upaliti eksploziju i obavijestiti sve klijente
            if (_projectileEffect) _projectileEffect.SetActive(false);
            if (_explosionEffect) _explosionEffect.SetActive(true);
            RPC_ExplodeEffect();

            if (HasStateAuthority)
            {
                // AOE šteta bez primarnog targeta (_hitPlayer) i bez friendly fire-a
                Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRange);
                foreach (var col in hits)
                {
                    if (col.tag != "Player") continue;
                    var p = col.GetComponent<PlayerStats>();
                    if (p == null) continue;
                    if (_hitPlayer != null && p == _hitPlayer) continue;
                    if (p.Object.InputAuthority == OwnerPlayerStats.Object.InputAuthority) continue;
                    if (FusionConnection.GameModeType == GameModeType.TDM && p.Team == OwnerPlayerStats.Team) continue;

                    p.DealDamage(_damage, OwnerPlayerStats);
                }
            }

            Destroy(gameObject, _explosionDuration);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
        public void RPC_ExplodeEffect()
        {
            if (_projectileEffect) _projectileEffect.SetActive(false);
            if (_explosionEffect) _explosionEffect.SetActive(true);
        }


        //This code can be used for testing hit range
        //private void Update()
        //{
        //    Collider[] hitColliders = Physics.OverlapSphere(transform.position, _explosionRange);
        //    if (hitColliders.Any((collider) => collider.tag == "Ground")) Debug.Log("In range");
        //}
    }
}
