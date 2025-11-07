using SpellFlinger.Enum;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.LoginScene
{
    public class LevelSelectionToggle : MonoBehaviour
    {
        [SerializeField] private Image _levelImage = null;
        [SerializeField] private Toggle _toggle = null;
        private LevelType _levelType;

        public void ShowLevel(LevelType levelType, ToggleGroup toggleGroup, Sprite sprite, Action<LevelType> onSelected)
        {
            _levelType = levelType;
            _toggle.onValueChanged.AddListener((isOn) => 
            { 
                if (isOn) onSelected.Invoke(_levelType);
            });
            _levelImage.sprite = sprite;
            _toggle.group = toggleGroup;
        }
    }
}
