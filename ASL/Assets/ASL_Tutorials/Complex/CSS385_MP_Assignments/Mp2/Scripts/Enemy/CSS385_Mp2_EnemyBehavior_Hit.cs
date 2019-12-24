using UnityEngine;
using System.Collections;

namespace Mps
{
    public partial class CSS385_Mp2_EnemyBehavior : MonoBehaviour
    {

        private void HitByHero(GameObject g)
        {
            if (mState == EnemyState.ePatrolState)
            {
                mState = EnemyState.eCWRotateState;
                mStateFrameTick = 0;
                GetComponent<SpriteRenderer>().color = kChaseColor;
                mTarget = g;
            }
            else if (mState == EnemyState.eChaseState)
            {
                // Debug.Log("Got you!");
                CSS385_Mp2_GlobalBehavior.sTheGlobalBehavior.HeroHitByEnemy();
                sEnemySystem.ReplaceOneEnemy();
                Destroy(gameObject);  // 
            }
        }

        private void HitByEgg()
        {
            mHitCount++;
            if (mHitCount == 1)
            {
                mState = EnemyState.eStunState;
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/EnemyStunned");
            }
            else if (mHitCount == 2)
            {
                mState = EnemyState.eEggState;
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/Egg");
            }
            else
            {
                ThisEnemyIsHit();
            }
        }

        private void ThisEnemyIsHit()
        {
            sEnemySystem.OneEnemyDestroyed();
            Destroy(gameObject);
        }
    }
}