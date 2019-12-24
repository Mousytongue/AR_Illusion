using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Mps
{
    public class CSS385_Mp2_EggSpawnSystem : MonoBehaviour
    {
        // UI Support
        public RectTransform mCoolDownTimeBar = null;

        private float kEggInterval = 0.2f;
        private const float kCoolDownBarSize = 100f;

        // handle correct cool off time
        private float mSpawnEggAt = 0f;

        // Count
        private int mEggCount = 0;

        private static CSS385_Mp2_EggSpawnSystem m_Instance;

        public void Start()
        {
            //Do to network latency, this start function will fire before this user has the correct name of this gameObject. Therefore,
            //This function was made public and the moment the name gets updated via the multi-purpose SendFloat4 function that is attached
            //to this player object upon its creation, calls this start function to ensure m_Instance is assigned properly.

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                //If player name matches with our peer id, then set this manager to follow that player
                if (Regex.IsMatch(player.name, ASL.GameSparksManager.Instance().GetPlayerPeerId().ToString()))
                {
                    m_Instance = this;
                    break;
                }
            }
            mSpawnEggAt = Time.realtimeSinceStartup - kEggInterval; // assume one was shot
        }

        void Update()
        {
            UpdateCoolDownUI();
        }

        #region Spawning support
        public bool CanSpawn()
        {
            return TimeTillNext() <= 0f;
        }

        public float TimeTillNext()
        {
            float sinceLastEgg = Time.realtimeSinceStartup - mSpawnEggAt;
            return kEggInterval - sinceLastEgg;
        }

        public void SpawnAnEgg(Vector3 p, Vector3 dir)
        {
            Debug.Assert(CanSpawn());
            ASL.ASLHelper.InstanitateASLObject("Egg", p, Quaternion.LookRotation(Vector3.forward, dir), "", "", AfterEggSpawn);
        }

        public static void AfterEggSpawn(GameObject _gameObject)
        {
            m_Instance.IncEggCount();
            m_Instance.mSpawnEggAt = Time.realtimeSinceStartup;
        }


        #endregion

        #region UI Support
        private void UpdateCoolDownUI()
        {
            float percentageT = TimeTillNext() / kEggInterval;

            if (mCoolDownTimeBar)
            {
                Vector2 s = mCoolDownTimeBar.sizeDelta;  // This is the WidthxHeight [in pixel units]
                s.x = percentageT * kCoolDownBarSize;
                mCoolDownTimeBar.sizeDelta = s;
            }

        }
        #endregion

        // Count support
        private void IncEggCount() { mEggCount++; }
        public void DecEggCount() { mEggCount--; }
        public int GetEggCount() { return mEggCount; }
    }
}