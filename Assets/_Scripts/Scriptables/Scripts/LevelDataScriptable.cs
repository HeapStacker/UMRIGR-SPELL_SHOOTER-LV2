using SpellFlinger.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpellFlinger.Scriptables
{
    [CreateAssetMenu(fileName = "Level Data Scriptable", menuName = "Level Data Scriptable")]
    class LevelDataScriptable : ScriptableObject
    {
        private static LevelDataScriptable _instance;
        public static LevelDataScriptable Instance { get => _instance; private set => _instance = value; }

        public void Init()
        {
            if (Instance == null) _instance = this;
        }

        [Serializable]
        public class LevelData
        {
            public LevelType LevelType;
            public Sprite LevelImage;
            public int LevelBuildId;
        }

        [SerializeField] private List<LevelData> _levels;

        public List<LevelData> Levels => _levels;

        public LevelData GetLevelData(LevelType levelType)
        {
            return _levels.First(level => level.LevelType == levelType);
        }

        public Sprite GetLevelSprite(LevelType levelType)
        {
            return _levels.First(level => level.LevelType == levelType).LevelImage;
        }

        public int GetLevelBuildId(LevelType levelType)
        {
            return _levels.First(level => level.LevelType == levelType).LevelBuildId;
        }
    }
}
