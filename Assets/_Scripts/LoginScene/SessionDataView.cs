using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.LoginScene
{
    public class SessionDataView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _sessionName = null;
        [SerializeField] private TextMeshProUGUI _gameMode = null;
        [SerializeField] private TextMeshProUGUI _playerCount = null;
        [SerializeField] private Image _levelImage = null;
        [SerializeField] private Toggle _toggle = null;

        public void ShowSession(string sessionName,  int playerCount, int maxPlayerCount, LevelType levelType, GameModeType gameModeType, Action<bool, (string, GameModeType, LevelType)> onToggle, ToggleGroup toggleGroup)
        {
            _sessionName.text = sessionName;
            _gameMode.text = gameModeType.ToString();
            _playerCount.text = "" + playerCount + "/" + maxPlayerCount; 
            _levelImage.sprite = LevelDataScriptable.Instance.GetLevelSprite(levelType);
            _toggle.onValueChanged.AddListener((isOn) => onToggle.Invoke(isOn, (sessionName, gameModeType, levelType)));
            _toggle.group = toggleGroup;
        }
    }
}
