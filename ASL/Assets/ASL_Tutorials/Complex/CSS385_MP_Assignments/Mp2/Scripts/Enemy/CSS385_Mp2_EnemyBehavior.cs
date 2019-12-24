using UnityEngine;
using System.Collections;

namespace Mps
{
    public partial class CSS385_Mp2_EnemyBehavior : MonoBehaviour
    {

        // All instances of Enemy shares this one CSS385_Mp2_WayPoint and EnemySystem
        static private CSS385_Mp2_WayPointSystem sWayPoints = null;
        static private CSS385_Mp2_EnemySpawnSystem sEnemySystem = null;
        static public void InitializeEnemySystem(CSS385_Mp2_EnemySpawnSystem s, CSS385_Mp2_WayPointSystem w) { sEnemySystem = s; sWayPoints = w; }

        private const float kSpeed = 20f;
        private int mWayPointIndex = 0;

        private const float kTurnRate = 0.03f / 60f;
        private int mHitCount = 0;

        // Use this for initialization
        void Start()
        {
            mWayPointIndex = sWayPoints.GetInitWayIndex();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateFSM();
        }

        private void PointAtPosition(Vector3 p, float r)
        {
            Vector3 v = p - transform.position;
            transform.up = Vector3.LerpUnclamped(transform.up, v, r);
        }

        #region Trigger into chase or die
        private void OnTriggerEnter2D(Collider2D collision)
        {
            TriggerCheck(collision.gameObject);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            TriggerCheck(collision.gameObject);
        }

        private void TriggerCheck(GameObject g)
        {
            if (g.tag == "Player")
            {
                HitByHero(g.gameObject);

            }
            else if (g.name == "Egg(Clone)")
            {
                HitByEgg();
            }
        }
        #endregion
    }
}