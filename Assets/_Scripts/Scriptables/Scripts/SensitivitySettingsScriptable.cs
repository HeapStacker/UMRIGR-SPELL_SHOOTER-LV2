using UnityEngine;

namespace SpellFlinger.Scriptables
{
    [CreateAssetMenu(fileName = "Sensitivity Settings Scriptable", menuName = "Sensitivitiy Settings Scriptable")]
    class SensitivitySettingsScriptable : ScriptableObject
    {
        private static SensitivitySettingsScriptable _instance;

        public static SensitivitySettingsScriptable Instance { get => _instance; private set => _instance = value; }

        public void Init()
        {
            if (Instance == null) _instance = this;
        }

        [SerializeField] private float _upDownSensitivity = 0f;
        [SerializeField] private float _leftRightSensitivity = 0f;
        private float _upDownMultiplier = 1f;
        private float _leftRightMultiplier = 1f;

        public float UpDownSensitivity => _upDownSensitivity * _upDownMultiplier;
        public float LeftRightSensitivity => _leftRightSensitivity * _leftRightMultiplier;
        public float UpDownMultiplier => _upDownMultiplier;
        public float LeftRightMultiplier => _leftRightMultiplier;

        public void SetUpDownMultiplier(float multiplier) => _upDownMultiplier = multiplier;
        public void SetLeftRightMultiplier(float multiplier) => _leftRightMultiplier = multiplier;
    }
}
