using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.LoginScene
{
    public class RoomCreationView : MonoBehaviour
    {
        [SerializeField] private Toggle _teamDeathMatchToggle = null;
        [SerializeField] private Toggle _deathMatchToggle = null;
        [SerializeField] private TMP_InputField _roomNameInput = null;
        [SerializeField] private Button _returnButton = null;
        [SerializeField] private Button _createRoomButton = null;
        [SerializeField] private LevelSelectionToggle _levelSelectionTogglePrefab = null;
        [SerializeField] private ToggleGroup _levelSelectionContainer = null;
        [SerializeField] private GameObject _sessionView = null;
        private LevelType _selectedLevelType;

        private void Awake()
        {
            // Default odabir na prvi level (sigurnost ako nitko nije kliknut)
            if (LevelDataScriptable.Instance.Levels.Count > 0)
                _selectedLevelType = LevelDataScriptable.Instance.Levels[0].LevelType;

            foreach (var levelData in LevelDataScriptable.Instance.Levels)
            {
                var toggle = Instantiate(_levelSelectionTogglePrefab, _levelSelectionContainer.transform);
                toggle.ShowLevel(levelData.LevelType, _levelSelectionContainer, levelData.LevelImage, (lt) => _selectedLevelType = lt);
            }

            _returnButton.onClick.AddListener(() =>
            {
                _sessionView.SetActive(true);
                gameObject.SetActive(false);
            });

            _createRoomButton.onClick.AddListener(CreateRoom);
        }

        private void CreateRoom()
        {
            var roomName = _roomNameInput.text;
            if (string.IsNullOrWhiteSpace(roomName))
                roomName = $"Room-{UnityEngine.Random.Range(1000, 9999)}";

            var gameMode = _teamDeathMatchToggle.isOn ? GameModeType.TDM : GameModeType.DM;

            FusionConnection.Instance.CreateSession(roomName, gameMode, _selectedLevelType);
        }
    }
}
