using SpellFlinger.Enum;
using SpellFlinger.PlayScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.Scriptables
{
    [CreateAssetMenu(fileName = "Weapon Data Scriptable", menuName = "Weapon Data Scriptable")]
    public class WeaponDataScriptable : ScriptableObject
    {
        private static WeaponType _selectedWeaponType;
        private static WeaponDataScriptable _instance;

        public static WeaponDataScriptable Instance { get => _instance; private set => _instance = value; }
        public static WeaponType SelectedWeaponType => _selectedWeaponType;

        public void Init()
        {
            if (Instance == null) _instance = this;
        }

        [Serializable]
        public class WeaponData
        {
            public WeaponType WeaponType;
            public Sprite WeaponImage;
            public Projectile WeaponPrefab;
            public GameObject GlovePrefab;
            public Vector3 GloveLocation;
            public float FireRate;
        }

        [SerializeField] private List<WeaponData> _weapons;
        public List<WeaponData> Weapons => _weapons;

        public WeaponData GetWeaponData(WeaponType weaponType)
        {
            return _weapons.First(weapon => weapon.WeaponType == weaponType);
        }

        public static void SetSelectedWeaponType(WeaponType weaponType) => _selectedWeaponType = weaponType;
    }
}
