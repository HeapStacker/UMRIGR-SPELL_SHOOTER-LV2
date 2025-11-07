using SpellFlinger.Enum;
using TMPro;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class PlayerScoreboardData : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerName = null;
        [SerializeField] private TextMeshProUGUI _kills = null;
        [SerializeField] private TextMeshProUGUI _deaths = null;
        private TeamType _teamType;

        public void Init(string playerName)
        {
            _playerName.text = playerName;
            _kills.text = "0";
            _deaths.text = "0";
        }

        public void SetTeamType(TeamType teamType)
        {
            _teamType = teamType;
            if (PlayerManager.Instance.FriendlyTeam != TeamType.None) UpdateTeamColors();
            else PlayerManager.Instance.OnPlayerTeamTypeSet += UpdateTeamColors;
        }

        public void UpdateScore(int kills, int deaths)
        {
            _kills.text = kills.ToString();
            _deaths.text = deaths.ToString();
        }

        private void UpdateTeamColors()
        {
            if (PlayerManager.Instance.FriendlyTeam == _teamType)
            {
                _playerName.color = PlayerManager.Instance.FriendlyColor;
                _kills.color = PlayerManager.Instance.FriendlyColor;
                _deaths.color = PlayerManager.Instance.FriendlyColor;
            }
            else
            {
                _playerName.color = PlayerManager.Instance.EnemyColor;
                _kills.color = PlayerManager.Instance.EnemyColor;
                _deaths.color = PlayerManager.Instance.EnemyColor;
            }
        }
    }
}
