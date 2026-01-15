using System;
using UnityEngine;
using GlobalEnums;

namespace Metroidvania.SceneManagement
{
    [System.Serializable]
    public class SceneSpawnPoints
    {
        [System.Serializable]
        public struct SceneSpawnPoint
        {
            public string key;
            public Transform position;
            public bool facingRight;
            public bool doFadeWalk;
            public GravityDirection gravityDirection;
        }

        public SceneSpawnPoint defaultSpawnPoint = new() { key = "default" };

        public SceneSpawnPoint[] spawnPoints;

        private SceneSpawnPoint FindSpawnPoint(string key)
        {
            foreach (SceneSpawnPoint spawnPoint in spawnPoints)
                if (spawnPoint.key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return spawnPoint;
            return defaultSpawnPoint;
        }

        public void TryGetSpawnPoint(string key, ref SceneSpawnPoint spawnPoint)
        {
            SceneSpawnPoint sp = FindSpawnPoint(key);
            spawnPoint = sp; // luôn gán
        }
    }
}
