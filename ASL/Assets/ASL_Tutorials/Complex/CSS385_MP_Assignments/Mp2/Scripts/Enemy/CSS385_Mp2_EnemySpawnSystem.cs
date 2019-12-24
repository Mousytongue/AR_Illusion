using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mps
{
    public class CSS385_Mp2_EnemySpawnSystem : MonoBehaviour
    {
        private const int kMaxEnemy = 25;

        private int mTotalEnemy = 0;
        public GameObject mPatrolEnemy = null;
        private Vector2 mSpawnRegionMin, mSpawnRegionMax;

        private int mEnemyDestroyed = 0;

        void Start()
        {
            Random.InitState(722018); //Ensures all users are in sync for non-networked objects as their randomness will be the same randomness
            GenerateEnemy();
        }

        void Update()
        {
        }

        public void SetSpawnRegion(Vector2 min, Vector2 max)
        {
            mSpawnRegionMin = min;
            mSpawnRegionMax = max;
        }

        private void GenerateEnemy()
        {
            for (int i = mTotalEnemy; i < kMaxEnemy; i++)
            {
                GameObject p = Instantiate(mPatrolEnemy);
                float x = Random.Range(mSpawnRegionMin.x, mSpawnRegionMax.x);
                float y = Random.Range(mSpawnRegionMin.y, mSpawnRegionMax.y);
                p.transform.position = new Vector3(x, y, 0f);
                mTotalEnemy++;
            }
        }

        public void OneEnemyDestroyed() { mEnemyDestroyed++; ReplaceOneEnemy(); }
        public void ReplaceOneEnemy() { mTotalEnemy--; GenerateEnemy(); }
        public string GetEnemyState() { return "   Enemy: Count(" + mTotalEnemy + ") Destroyed(" + mEnemyDestroyed + ")"; }
    }
}