using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using SpellFlinger.PlayScene;
using SpellFlinger.Enum;
using SpellFlinger.LoginScene;
using SpellFlinger.Scriptables;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using WebSocketSharp;

namespace SpellSlinger.Networking
{
    public class FusionConnection : SingletonPersistent<FusionConnection>, INetworkRunnerCallbacks
    {
        private static string _playerName = null;
        [SerializeField] private PlayerCharacterController _playerPrefab = null;
        [SerializeField] private GameManager _gameManagerPrefab = null;
        [SerializeField] private NetworkRunner _networkRunnerPrefab = null;
        [SerializeField] private int _playerCount = 10;
        private NetworkRunner _runner = null;
        private NetworkSceneManagerDefault _networkSceneManager= null;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private List<SessionInfo> _sessions = new List<SessionInfo>();
        private static GameModeType _gameModeType;

        public PlayerCharacterController LocalCharacterController { get; set; }
        public List<SessionInfo> Sessions => _sessions;
        public static GameModeType GameModeType => _gameModeType;

        public string PlayerName => _playerName;

        private void Awake()
        {
            base.Awake();
            _networkSceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        public void ConnectToLobby(String playerName = null)
        {
            if(!playerName.IsNullOrEmpty()) _playerName = playerName;

            // Ako runner ne postoji (npr. nakon LeaveSession), kreiraj novi i registriraj callbackove
            if (_runner == null || _runner.IsDestroyed())
            {
                _runner = gameObject.AddComponent<NetworkRunner>();
                _runner.AddCallbacks(this);

                var inputManager = FindObjectOfType<InputManager>();
                if (inputManager != null)
                    _runner.AddCallbacks(inputManager);
            }

            _runner.JoinSessionLobby(SessionLobby.ClientServer);
        }

        public async void CreateSession(string sessionName, GameModeType gameMode, LevelType level)
        {
            // Osiguraj runner ako je očišćen prilikom prethodnog izlaska
            if (_runner == null || _runner.IsDestroyed())
            {
                _runner = gameObject.AddComponent<NetworkRunner>();
                _runner.AddCallbacks(this);

                var inputManager = FindObjectOfType<InputManager>();
                if (inputManager != null)
                    _runner.AddCallbacks(inputManager);
            }

            _gameModeType = gameMode;
            _runner.ProvideInput = true;

            var sessionProperties = new Dictionary<string, SessionProperty>
            {
                { "gm", (SessionProperty)(int)gameMode },
                { "lvl", (SessionProperty)(int)level }
            };

            var sceneRef = SceneRef.FromIndex(LevelDataScriptable.Instance.GetLevelBuildId(level));

            await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = sessionName,
                Scene = sceneRef,
                PlayerCount = _playerCount,
                SceneManager = _networkSceneManager,
                SessionProperties = sessionProperties
            });
        }

        public async void JoinSession(string sessionName, GameModeType gameMode)
        {
            // Osiguraj runner ako je očišćen prilikom prethodnog izlaska
            if (_runner == null || _runner.IsDestroyed())
            {
                _runner = gameObject.AddComponent<NetworkRunner>();
                _runner.AddCallbacks(this);

                var inputManager = FindObjectOfType<InputManager>();
                if (inputManager != null)
                    _runner.AddCallbacks(inputManager);
            }

            _runner.ProvideInput = true;
            _gameModeType = gameMode;

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = sessionName,
            });
        }

        public void LeaveSession()
        {
            if (_runner != null)
            {
                var runnerRef = _runner;
                _runner = null;

                if (!runnerRef.IsDestroyed())
                    runnerRef.Shutdown();

                if (runnerRef && runnerRef.gameObject)
                    Destroy(runnerRef.gameObject);
            }

            // Očisti cacheirane reference igrača
            _spawnedCharacters.Clear();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(0);
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _sessions = sessionList;
            SessionView.Instance.UpdateSessionList();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Joined");
            if (runner.IsServer)
            {
                if (player == runner.LocalPlayer)
                {
                    HealingPointSpawner.Instance.SpawnHealingPoints(runner);
                    runner.Spawn(_gameManagerPrefab);
                }

                NetworkObject playerObject = runner.Spawn(_playerPrefab.gameObject, inputAuthority: player);
                
                // Provjeri da li već postoji prije dodavanja (izbjegni ArgumentException)
                if (_spawnedCharacters.ContainsKey(player))
                {
                    _spawnedCharacters[player] = playerObject;
                }
                else
                {
                    _spawnedCharacters.Add(player, playerObject);
                }

                PlayerStats stats = playerObject.GetComponent<PlayerCharacterController>().PlayerStats;
                if (_gameModeType == GameModeType.TDM) stats.Team = PlayerManager.Instance.GetTeamWithLessPlayers();
            }
        }


        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("On Player Left");
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        private void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            if (_runner != null && !_runner.IsDestroyed()) _runner.Shutdown();
        }

        #region UnusedCallbacks
        // OnInput je implementiran u InputManager klasi, ne ovdje
        // Ova metoda mora postojati jer FusionConnection implementira INetworkRunnerCallbacks
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // Input se obrađuje u InputManager klasi
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("On Connected to server");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log("On Connect Failed");
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            Debug.Log("On Connect Request");
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            Debug.Log("On Custom Authentication Response");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log("On Disconnected From Server");
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.Log("On Host Migration");
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            Debug.Log("On Input Missing");
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log("On Object Enter AOI");
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            Debug.Log("OnO bject Exit AOI");
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            Debug.Log("On Reliable Data Progress");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            Debug.Log("On Reliable Data Received");
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("On Scene Load Done");
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("On Scene Load Start");
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("On Shut down, reason: " + shutdownReason.ToString());
            LeaveSession();
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            Debug.Log("On User Simulation Message");
        }
        #endregion
    }
}