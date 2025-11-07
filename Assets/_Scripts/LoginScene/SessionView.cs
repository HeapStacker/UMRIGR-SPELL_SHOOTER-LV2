using Fusion;
using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.LoginScene
{
    public class SessionView : Singleton<SessionView>
    {
        [SerializeField] private Button _createRoomButton = null;
        [SerializeField] private Button _refreshButton = null;
        [SerializeField] private Button _joinButton = null;
        [SerializeField] private GameObject _roomCreationView = null;
        [SerializeField] private SessionDataView _sessionDataViewPrefab = null;
        [SerializeField] private ToggleGroup _sessionListContainer = null;
        [SerializeField] private WeaponSelectionToggle _weaponSelectionTogglePrefab = null;
        [SerializeField] private ToggleGroup _weaponSelectionContainer = null;
        private (string, GameModeType, LevelType) _sessionData;
        private List<SessionDataView> _sessions = new List<SessionDataView>();

        private void Awake()
        {
            base.Awake();
            _createRoomButton.onClick.AddListener(() =>
            {
                _roomCreationView.SetActive(true);
                gameObject.SetActive(false);
            });

            _joinButton.interactable = false;

            foreach (var data in WeaponDataScriptable.Instance.Weapons)
            {
                WeaponSelectionToggle weaponToggle = Instantiate(_weaponSelectionTogglePrefab, _weaponSelectionContainer.transform);
                weaponToggle.ShowWeapon(data.WeaponType, _weaponSelectionContainer, data.WeaponImage, (weaponType) => WeaponDataScriptable.SetSelectedWeaponType(weaponType));
            }

            UpdateSessionList();
            _refreshButton.onClick.AddListener(UpdateSessionList);
            _joinButton.onClick.AddListener(() => FusionConnection.Instance.JoinSession(_sessionData.Item1, _sessionData.Item2));
        }

        public void UpdateSessionList()
        {
            foreach (var item in _sessions)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _sessions.Clear();

            _joinButton.interactable = false;

            var sessions = FusionConnection.Instance.Sessions;
            if (sessions == null || sessions.Count == 0) return;

            foreach (var s in sessions)
            {
                if (!s) continue;

                GameModeType gameMode = GameModeType.DM;
                LevelType level = LevelType.Desert;

                if (s.Properties != null)
                {
                    if (s.Properties.TryGetValue("gm", out var gmProp))
                        gameMode = (GameModeType)(int)gmProp;

                    if (s.Properties.TryGetValue("lvl", out var lvlProp))
                        level = (LevelType)(int)lvlProp;
                }

                var view = Instantiate(_sessionDataViewPrefab, _sessionListContainer.transform);
                view.ShowSession(
                    s.Name,
                    s.PlayerCount,
                    s.MaxPlayers,
                    level,
                    gameMode,
                    SessionOnToggle,
                    _sessionListContainer
                );

                _sessions.Add(view);
            }
        }

        private void SessionOnToggle(bool isOn, (string, GameModeType, LevelType) sessionData)
        {
            if (isOn)
            {
                _sessionData = sessionData;
                _joinButton.interactable = true;
            }
            else if (sessionData == _sessionData) _joinButton.interactable = false;
        }
    }
}
