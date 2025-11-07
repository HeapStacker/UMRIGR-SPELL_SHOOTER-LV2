using Fusion;
using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
	public class EarthProjectile : Projectile
	{
		[SerializeField] private float _range = 0.4f;
		[SerializeField] private float _explosionRange = 2.25f;
		[SerializeField] private float _explosionDuration = 0.8f;
		[SerializeField] private float _slowOnHitDuration = 1.25f;
		[SerializeField] private GameObject _projectileEffect = null;
		[SerializeField] private GameObject _explosionEffect = null;
		[SerializeField] private GameObject _impactFxPrefab = null;
		[SerializeField] private float _knockbackForce = 16f;
		[SerializeField] private float _knockUpForce = 6f;

		private bool _exploded = false;
		private PlayerStats _primaryHit = null;

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
				if (player == null) continue;
				if (player.Object.InputAuthority == OwnerPlayerStats.Object.InputAuthority) continue;
				if (FusionConnection.GameModeType == GameModeType.TDM && player.Team == OwnerPlayerStats.Team) continue;

				player.DealDamage(_damage, OwnerPlayerStats);
				_primaryHit = player;
				Explode();
				return;
			}

			if (hitColliders.Any((collider) => collider.tag == "Ground"))
			{
				Explode();
			}
		}

		private void Explode()
		{
			_exploded = true;

			if (_projectileEffect) _projectileEffect.SetActive(false);
			if (_explosionEffect) _explosionEffect.SetActive(true);
			RPC_ExplodeEffect();

			if (HasStateAuthority)
			{
				Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRange);
				foreach (var col in hits)
				{
					if (col.tag != "Player") continue;
					var p = col.GetComponent<PlayerStats>();
					if (p == null) continue;
					if (_primaryHit != null && p == _primaryHit) continue;
					if (p.Object.InputAuthority == OwnerPlayerStats.Object.InputAuthority) continue;
					if (FusionConnection.GameModeType == GameModeType.TDM && p.Team == OwnerPlayerStats.Team) continue;

					p.DealDamage(_damage, OwnerPlayerStats);
					p.ApplySlow(_slowOnHitDuration);

					// Knockback – od točke eksplozije prema igraču, uz blagi skok
					var dir = (p.transform.position - transform.position);
					dir.y = 0f; // čisto horizontalno
					p.PlayerCharacterController.ApplyKnockback(dir, _knockbackForce, _knockUpForce);
				}
			}

			Destroy(gameObject, _explosionDuration);
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
		public void RPC_ExplodeEffect()
		{
			if (_projectileEffect) _projectileEffect.SetActive(false);
			if (_explosionEffect) _explosionEffect.SetActive(true);

			// Instanciraj EarthImpactFX na svim klijentima
			if (_impactFxPrefab)
			{
				var pos = transform.position;
				Quaternion rot = Quaternion.identity;

				// Poravnaj efekt s normalom tla
				if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 2f))
				{
					pos = hit.point;
					rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
				}

				Instantiate(_impactFxPrefab, pos, rot);
			}
		}
	}
}


