using Fusion;
using System.Collections;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class HealingPoint : NetworkBehaviour
    {
        [SerializeField] private Collider _trigger = null;
        [SerializeField] private Transform _model = null;
        [SerializeField] private int _healAmount = 0;
        [SerializeField] private int _respawnTime = 0;

        [Networked, OnChangedRender(nameof(RespawnHealIteamSequence))] private int RespawnTimeLeft {  get; set; }

        public override void Spawned()
        {
            if (HasStateAuthority) _trigger.enabled = true;
            else if (RespawnTimeLeft > 0) _model.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
            {
                playerStats.Heal(_healAmount);
                _trigger.enabled = false;
                RespawnTimeLeft = _respawnTime;
            }
        }

        private void RespawnHealIteamSequence()
        {
            if (!_model.gameObject.activeSelf && RespawnTimeLeft > 0) return;
            else if (!_model.gameObject.activeSelf && RespawnTimeLeft == 0) _model.gameObject.SetActive(true);
            else if (_model.gameObject.activeSelf && RespawnTimeLeft > 0)
            {
                _model.gameObject.SetActive(false);
                if (HasStateAuthority) StartCoroutine(Respawn());
            }
        }

        private IEnumerator Respawn()
        {
            for(int i = RespawnTimeLeft; i >= 0; i--)
            {
                yield return new WaitForSeconds(1);
                RespawnTimeLeft--;
            }
            _trigger.enabled = true;
        }
    }
}
