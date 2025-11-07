using Fusion;
using SpellFlinger.Enum;
using SpellSlinger.Networking;
using System.Collections;
using UnityEngine;

namespace SpellFlinger.PlayScene
{
    public class GameManager : NetworkBehaviour
    {
        private static GameManager _instance;
        [SerializeField] private int _teamKillsForWin = 0;
        [SerializeField] private int _soloKillsForWin = 0;
        [SerializeField] private int _gameEndTime = 0;

        public static GameManager Instance => _instance;
        [Networked] public int TeamAKills { get; private set;}
        [Networked] public int TeamBKills { get; private set;}
        [Networked, OnChangedRender(nameof(WinnerChanged))] public TeamType WinnerTeam { get; private set;}
        [Networked, OnChangedRender(nameof(WinnerChanged))] public NetworkString<_32> WinnerPlayerName { get; set; }
        [Networked] public int RemainingGameEndTime { get; private set; }
        public int TeamKillsForWin => _teamKillsForWin;
        public int SoloKillsForWin => _soloKillsForWin;

        public override void Spawned()
        {
            _instance = this;
            if (RemainingGameEndTime > 0)
            {
                UiManager.Instance.UpdateEndGameText();
                FusionConnection.Instance.LocalCharacterController.GameEnd();
            }
            UiManager.Instance.UpdateTeamScore();
        }

        public void AddTeamKill(TeamType team)
        {
            if(team == TeamType.TeamA) TeamAKills++;
            else TeamBKills++;
        }

        public int GetTeamKills(TeamType team)
        {
            if (team == TeamType.TeamA) return TeamAKills;
            else return TeamBKills;
        }

        public void GameEnd(string playerName)
        {
            WinnerPlayerName = playerName;
            StartCoroutine(GameEndCountdown());
        }

        public void GameEnd(TeamType winnerTeam)
        {
            WinnerTeam = winnerTeam;
            StartCoroutine(GameEndCountdown());
        }

        private IEnumerator GameEndCountdown()
        {
            RemainingGameEndTime = _gameEndTime;

            // Zaustavi rundu kod svih igrača
            PlayerManager.Instance.SendGameEndRpc();

            while (RemainingGameEndTime > 0)
            {
                yield return new WaitForSeconds(1f);
                RemainingGameEndTime--;
            }

            // Pokreni novu rundu
            PlayerManager.Instance.SendGameStartRpc();
        }

        private void WinnerChanged() => UiManager.Instance.UpdateEndGameText();
    }
}