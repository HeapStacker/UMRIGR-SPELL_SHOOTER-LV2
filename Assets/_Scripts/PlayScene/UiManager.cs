using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.PlayScene
{
    public class UiManager : Singleton<UiManager>
    {
        [SerializeField] private Button _pauseButton = null;
        [SerializeField] private GameObject _pauseMenu = null;
        [SerializeField] private Button _returnButton = null;
        [SerializeField] private Button _leaveGameButton = null;
        [SerializeField] private GameObject _scoreMenu = null;
        [SerializeField] private Transform _scoreBoardContainer = null;
        [SerializeField] private PlayerScoreboardData _scoreBoardDataPrefab = null;
        [SerializeField] private GameObject _deathScreen = null;
        [SerializeField] private TextMeshProUGUI _deathTimer = null;
        [SerializeField] private GameObject _aimCursor = null;
        [SerializeField] private GameObject _teamScore = null;
        [SerializeField] private GameObject _soloScore = null;
        [SerializeField] private TextMeshProUGUI _teamAScoreText = null;
        [SerializeField] private TextMeshProUGUI _teamBScoreText = null;
        [SerializeField] private TextMeshProUGUI _soloScoreText = null;
        [SerializeField] private TextMeshProUGUI _healthText = null;
        [SerializeField] private Slider _healthSlider = null;
        [SerializeField] private Slider _upDownSensitivity = null;
        [SerializeField] private Slider _leftRightSensitivity = null;
        [SerializeField] private GameObject _gameEndScreen = null;
        [SerializeField] private TextMeshProUGUI _winnerText = null;

        private void Start()
        {
            base.Awake();
            _pauseButton.onClick.AddListener(() => _pauseMenu.SetActive(true));
            _returnButton.onClick.AddListener(() =>
            {
                _pauseMenu.SetActive(false);
                CameraController.Instance.CameraEnabled = true;
            });
            _leaveGameButton.onClick.AddListener(() =>
            {
                CameraController.Instance.CameraEnabled = false;
                FusionConnection.Instance.LeaveSession();
            });

            _upDownSensitivity.value = SensitivitySettingsScriptable.Instance.UpDownMultiplier;
            _leftRightSensitivity.value = SensitivitySettingsScriptable.Instance.LeftRightMultiplier;

            _upDownSensitivity.onValueChanged.AddListener((mutliplier) => SensitivitySettingsScriptable.Instance.SetUpDownMultiplier(mutliplier));
            _leftRightSensitivity.onValueChanged.AddListener((mutliplier) => SensitivitySettingsScriptable.Instance.SetLeftRightMultiplier(mutliplier));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt)) _pauseMenu.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Tab)) _scoreMenu.SetActive(true);
            if (Input.GetKeyUp(KeyCode.Tab)) _scoreMenu.SetActive(false);
        }

        public PlayerScoreboardData CreatePlayerScoarboardData() => Instantiate(_scoreBoardDataPrefab, _scoreBoardContainer);

        public void ShowPlayerDeathScreen(int timer)
        {
            _deathScreen.SetActive(true);
            _deathTimer.text = timer.ToString();
            _aimCursor.SetActive(false);
        }

        public void UpdateDeathTimer(int time) => _deathTimer.text = time.ToString();

        public void HideDeathTimer()
        {
            _deathScreen.SetActive(false);
            _aimCursor.SetActive(true);
        }

        public void ShowTeamScore()
        {
            _teamScore.SetActive(true);

            if (PlayerManager.Instance.FriendlyTeam == TeamType.TeamA)
            {
                _teamAScoreText.color = PlayerManager.Instance.FriendlyColor;
                _teamBScoreText.color = PlayerManager.Instance.EnemyColor;
            }
            else 
            {
                _teamAScoreText.color = PlayerManager.Instance.EnemyColor;
                _teamBScoreText.color = PlayerManager.Instance.FriendlyColor;
            }
        }

        public void ShowSoloScore()
        {
            _soloScore.SetActive(true); 
            _soloScoreText.text = "Kills: 0";
        }

        public void UpdateTeamScore()
        {
            _teamAScoreText.text = "Team A: " + GameManager.Instance.TeamAKills;
            _teamBScoreText.text = "Team B: " + GameManager.Instance.TeamBKills;
        }

        public void UpdateSoloScore(int kills) => _soloScoreText.text = "Kills: " + kills.ToString();

        public void UpdateHealthBar(int health, float healthPercentage)
        {
            _healthText.text = health.ToString();
            _healthSlider.value = healthPercentage;
        }

        public void ShowEndGameScreen()
        {
            _gameEndScreen.SetActive(true);
            _aimCursor.SetActive(false);
        }

        public void UpdateEndGameText()
        {
            Color endGameTextColor = default;
            if (FusionConnection.GameModeType == GameModeType.TDM)
            {
                if (GameManager.Instance.WinnerTeam == TeamType.TeamA) _winnerText.text = "Team A wins!!!";
                else _winnerText.text = "Team B wins!!!";
                bool isFriendlyTeamWinner = GameManager.Instance.WinnerTeam == FusionConnection.Instance.LocalCharacterController.PlayerStats.Team;
                endGameTextColor = isFriendlyTeamWinner ? PlayerManager.Instance.FriendlyColor : PlayerManager.Instance.EnemyColor;
            }
            else if (FusionConnection.GameModeType == GameModeType.DM)
            {
                if (GameManager.Instance.WinnerPlayerName == FusionConnection.Instance.PlayerName)
                {
                    _winnerText.text = "You win!!!";
                    endGameTextColor = PlayerManager.Instance.FriendlyColor;
                }
                else
                {
                    _winnerText.text = $"{GameManager.Instance.WinnerPlayerName} wins!!!";
                    endGameTextColor = PlayerManager.Instance.EnemyColor;
                }
            }

            _winnerText.color = endGameTextColor;
        }

        public void HideEndGameScreen()
        {
            _gameEndScreen.SetActive(false);
            _aimCursor.SetActive(true);
        }
    }
}
