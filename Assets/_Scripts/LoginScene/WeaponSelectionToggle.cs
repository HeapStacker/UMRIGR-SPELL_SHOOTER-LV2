using SpellFlinger.Enum;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.LoginScene
{
    public class WeaponSelectionToggle : MonoBehaviour
    {
        [SerializeField] private Image _weaponImage = null;
        [SerializeField] private Toggle _toggle = null;
        private WeaponType _weaponType;

        public void ShowWeapon(WeaponType weaponType, ToggleGroup toggleGroup, Sprite sprite, Action<WeaponType> onSelected)
        {
            _weaponType = weaponType;
            _toggle.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) onSelected.Invoke(_weaponType);
            });
            _weaponImage.sprite = sprite;
            _toggle.group = toggleGroup;
        }
    }
}
