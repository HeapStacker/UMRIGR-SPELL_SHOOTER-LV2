using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;
using SpellFlinger.PlayScene;
using SpellFlinger.Scriptables;

namespace SpellSlinger.Networking
{
    public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
    {
        private NetworkInputData _accumulatedInput;
        private bool _reset = false;

        public void BeforeUpdate()
        {
            if (_reset)
            {
                _reset = false;
                _accumulatedInput = default;
            }

            if (CameraController.Instance && !CameraController.Instance.CameraEnabled)
            {
                return;
            }

            Vector2 direction = Vector2.zero;
            NetworkButtons buttons = default;

            // WASD / strelice
            direction.x = Input.GetAxisRaw("Horizontal");
            direction.y = Input.GetAxisRaw("Vertical");

            // Yaw rotacija lika (miš lijevo/desno), skalirano osjetljivošću
            float yaw = Input.GetAxis("Mouse X") * SensitivitySettingsScriptable.Instance.LeftRightSensitivity;

            // Skok i pucanje
            buttons.Set(NetworkInputData.JUMP, Input.GetKey(KeyCode.Space));
            buttons.Set(NetworkInputData.SHOOT, Input.GetMouseButton(0));

            buttons.Set(NetworkInputData.SHOOT, Input.GetMouseButton(0));

            // Zaštita: u Login sceni LocalCharacterController još ne postoji
            if (Input.GetMouseButton(0) 
                && FusionConnection.Instance != null 
                && FusionConnection.Instance.LocalCharacterController != null)
            {
                _accumulatedInput.ShootTarget = FusionConnection.Instance.LocalCharacterController.GetShootDirection();
            }

            // Akumuliraj do OnInput
            _accumulatedInput.Direction += direction;
            _accumulatedInput.YRotation += yaw;
            _accumulatedInput.Buttons = new NetworkButtons(_accumulatedInput.Buttons.Bits | buttons.Bits);

        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _accumulatedInput.Direction.Normalize();
            input.Set(_accumulatedInput);

            _reset = true;
            _accumulatedInput.YRotation = 0f;
        }

        #region UnusedCallbacks
        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
    #endregion
}