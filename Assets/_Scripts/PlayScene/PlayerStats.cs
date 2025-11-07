using Fusion;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.PlayScene
{
    public class PlayerStats : NetworkBehaviour
    {
        [SerializeField] private TextMeshPro _playerNameText = null;
        [SerializeField] private Slider _healthBar = null;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private GameObject _playerModel = null;
        private bool _init = false;
        private PlayerScoreboardData _playerScoreboardData = null;
        private PlayerCharacterController _playerCharacterController = null;

        public Action OnSpawnedCallback;

        public PlayerCharacterController PlayerCharacterController => _playerCharacterController;
        public bool IsSlowed => SlowDuration > 0.001f;

        [Networked, OnChangedRender(nameof(PlayerNameChanged))] public NetworkString<_32> PlayerName { get; set; }
        [Networked, OnChangedRender(nameof(TeamChanged))] public TeamType Team { get; set; }
        [Networked, OnChangedRender(nameof(WeaponChanged))] public WeaponType SelectedWeapon { get; set; }
        [Networked, OnChangedRender(nameof(HealthChanged))] public int Health { get; set; }
        [Networked, OnChangedRender(nameof(KillsChanged))] public int Kills { get; set; }
        [Networked, OnChangedRender(nameof(DeathsChanged))] public int Deaths { get; set; }
        [Networked] public float SlowDuration { get; set; }

        private void LateUpdate()
        {
            _playerNameText.transform.LookAt(CameraController.Instance.transform);
            _playerNameText.transform.Rotate(0, 180, 0);
        }

        public override void FixedUpdateNetwork()
        {

            if (HasStateAuthority && SlowDuration > 0.001f) SlowDuration -= Runner.DeltaTime;
        }

        public override void Spawned()
        {
            _playerCharacterController = GetComponent<PlayerCharacterController>();
            _playerScoreboardData = UiManager.Instance.CreatePlayerScoarboardData();
            PlayerManager.Instance.RegisterPlayer(this);
            
            if (HasInputAuthority)
            {
                _playerNameText.gameObject.SetActive(false);
                RPC_InitializeData(FusionConnection.Instance.PlayerName, WeaponDataScriptable.SelectedWeaponType);
                if (FusionConnection.GameModeType == GameModeType.DM) UiManager.Instance.ShowSoloScore();
                else if (FusionConnection.GameModeType == GameModeType.TDM) UiManager.Instance.ShowTeamScore();
            }

            if(PlayerName.Value != default) PlayerNameChanged();
            if (Team != default) TeamChanged();
            if (SelectedWeapon != default) WeaponChanged();
            if(Health != default) HealthChanged();
            if(Kills != default) KillsChanged();
            if(Deaths != default) DeathsChanged();

            if(!HasInputAuthority && FusionConnection.GameModeType == GameModeType.DM) PlayerManager.Instance.SetPlayerColor(this);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RPC_InitializeData(string playerName, WeaponType selectedWeapon, RpcInfo info = default)
        {
            Kills = 0;
            Deaths = 0;
            Health = _maxHealth;
            PlayerName = playerName;
            SelectedWeapon = selectedWeapon;
        }

        private void PlayerNameChanged()
        {
            _playerNameText.text = PlayerName.ToString();
            _playerScoreboardData.Init(PlayerName.ToString());
        }

        private void TeamChanged()
        {
            if (HasInputAuthority) PlayerManager.Instance.SetFriendlyTeam(Team);
            else PlayerManager.Instance.SetPlayerColor(this);
            _playerScoreboardData.SetTeamType(Team);
        }

        private void WeaponChanged()
        {
            var weaponData = WeaponDataScriptable.Instance.GetWeaponData(SelectedWeapon);
            _playerCharacterController.SetWeapon(weaponData);
        }

        private void HealthChanged()
        {
            float percentage = (float)Health / _maxHealth;
            if (HasInputAuthority)
            {
                UiManager.Instance.UpdateHealthBar(Health, percentage);
            }
            else
            {
                if (_healthBar) _healthBar.value = percentage;
            }
        }

        private void KillsChanged()
        {
            _playerScoreboardData.UpdateScore(Kills, Deaths);
            if (FusionConnection.GameModeType == GameModeType.DM)
            {
                UiManager.Instance.UpdateSoloScore(Kills);
            }
            else
            {
                UiManager.Instance.UpdateTeamScore();
            }
        }

        private void DeathsChanged() => _playerScoreboardData.UpdateScore(Kills, Deaths);

        public void DealDamage(int damage, PlayerStats attacker)
        {
            bool wasAlive = Health > 0;
            Health -= damage;
            if (Health < 0) Health = 0;

            if (wasAlive && Health <= 0)
            {
                Deaths++;
                PlayerCharacterController.PlayerKilled();

                attacker.Kills++;

                if (FusionConnection.GameModeType == GameModeType.TDM)
                {
                    GameManager.Instance.AddTeamKill(attacker.Team);
                    if (GameManager.Instance.GetTeamKills(attacker.Team) >= GameManager.Instance.TeamKillsForWin)
                    {
                        GameManager.Instance.GameEnd(attacker.Team);
                    }
                }
                else // DM
                {
                    if (attacker.Kills >= GameManager.Instance.SoloKillsForWin)
                    {
                        GameManager.Instance.GameEnd(attacker.PlayerName.ToString());
                    }
                }
            }
        }

        public void Heal(int healAmount)
        {
            Health += healAmount;
            if(Health > _maxHealth) Health  = _maxHealth;
        }

        public void ApplySlow(float duration)
        {
            if (duration > SlowDuration) SlowDuration = duration;
        }

        public void ResetHealth() => Health = _maxHealth;

        public void SetTeamMaterial(Material material, Color color)
        {
            _playerNameText.color = color;
            if (!HasInputAuthority)
            {
                _playerModel.GetComponent<Renderer>().material = material;
            }
        }

        public void ResetGameInfo()
        {
            Kills = 0;
            Deaths = 0;
            Health = _maxHealth;
        }

        private void OnDestroy()
        {
            if(_playerScoreboardData != null && !_playerScoreboardData.gameObject.IsDestroyed()) Destroy(_playerScoreboardData.gameObject);
            PlayerManager.Instance?.UnregisterPlayer(this);
        }
    }
}