using SpellFlinger.Scriptables;
using UnityEngine;

namespace Scriptables
{
    class ScriptablesHolder : SingletonPersistent<ScriptablesHolder>
    {
        [SerializeField] private LevelDataScriptable _levelDataScriptable;
        [SerializeField] private WeaponDataScriptable _weaponDataScriptable;
        [SerializeField] private SensitivitySettingsScriptable _sensitivitySettingsScriptable;

        public LevelDataScriptable LevelDataScriptable => _levelDataScriptable;
        public WeaponDataScriptable WeaponDataScriptable => _weaponDataScriptable;
        public SensitivitySettingsScriptable SensitivitySettingsScriptable => _sensitivitySettingsScriptable;

        private void Awake()
        {
            base.Awake();
            _levelDataScriptable.Init();
            _weaponDataScriptable.Init();
            _sensitivitySettingsScriptable.Init();
        }
    }
}
