using UnityEngine;
using TMPro;

namespace SpellFlinger.LoginScene
{
    public class TestAutofillNames :MonoBehaviour
    {
        [SerializeField] private bool _autofillActive = false;
        [SerializeField] private TMP_InputField _playerName = null;
        [SerializeField] private TMP_InputField _roomName = null;
        private const string _chars = "abcdefghijklmnoprstuzv";

        private void Awake()
        {
            if (!_autofillActive) return;

            string randWord = "";
            for (int i = 0; i < 8; i++) randWord += _chars[Random.Range(0, _chars.Length - 1)];
            _playerName.text = randWord;
            for (int i = 0; i < 8; i++) randWord += _chars[Random.Range(0, _chars.Length - 1)];
            _roomName.text = randWord;
        }
    }
}