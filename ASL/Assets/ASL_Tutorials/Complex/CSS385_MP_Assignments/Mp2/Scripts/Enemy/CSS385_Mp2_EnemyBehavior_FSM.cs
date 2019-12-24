using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    public partial class CSS385_Mp2_EnemyBehavior : MonoBehaviour
    {
        private enum EnemyState
        {
            ePatrolState,
            eEnlargeState,
            eCWRotateState,
            eCCWRotateState,
            eShrinkState,
            eChaseState,
            eStunState,
            eEggState
        };
        private const float kSizeChangeFrames = 120f;
        private const float kRotateFrames = 80f;
        private const float kScaleRate = 2f / 60f;// around per second rate
        private const float kRotateRate = 90f / 60f;  // in degrees, around per second rate
        private Color kNormalColor = new Color(1f, 1f, 1f, 1.0f);
        private Color kChaseColor = new Color(1f, 0f, 0f, 1f);

        private int mStateFrameTick = 0;
        private EnemyState mState = EnemyState.ePatrolState;

        private GameObject mTarget = null;
        private const float kChaseDistance = 40f;
        private const float kChaseTurnRate = 0.8f;

        private void UpdateFSM()
        {
            switch (mState)
            {
                case EnemyState.eEnlargeState:
                    ServiceEnlargeState();
                    break;
                case EnemyState.eCWRotateState:
                    ServiceCWState();
                    break;
                case EnemyState.eCCWRotateState:
                    ServiceCCWState();
                    break;
                case EnemyState.eShrinkState:
                    ServiceShrinkState();
                    break;
                case EnemyState.ePatrolState:
                    ServicePatrolState();
                    break;
                case EnemyState.eChaseState:
                    ServiceChaseState();
                    break;
                case EnemyState.eStunState:
                    ServiceStunState();
                    break;
                case EnemyState.eEggState:
                    ServiceEggState();
                    break;
            }
        }

        #region Simple FSM services
        private void ServiceEnlargeState()
        {
            if (mStateFrameTick > kSizeChangeFrames)
            {
                // Transite to next state
                mState = EnemyState.eShrinkState;
                mStateFrameTick = 0;
            }
            else
            {
                mStateFrameTick++;

                // assume scale in X/Y are the same
                float s = transform.localScale.x;
                s += kScaleRate;
                transform.localScale = new Vector3(s, s, 1);
            }
        }

        private void ServiceShrinkState()
        {
            if (mStateFrameTick > kSizeChangeFrames)
            {
                // Transite to next state
                mState = EnemyState.ePatrolState;
                mTarget = null;
                GetComponent<SpriteRenderer>().color = kNormalColor;
                mStateFrameTick = 0;
            }
            else
            {
                mStateFrameTick++;

                // assume scale in X/Y are the same
                float s = transform.localScale.x;
                s -= kScaleRate;
                transform.localScale = new Vector3(s, s, 1);
            }
        }

        private void ServiceCWState()
        {
            if (mStateFrameTick > kRotateFrames)
            {
                // Transite to next state
                mState = EnemyState.eCCWRotateState;
                mStateFrameTick = 0;
            }
            else
            {
                mStateFrameTick++;

                Vector3 angles = transform.rotation.eulerAngles;
                angles.z += kRotateRate;
                transform.rotation = Quaternion.Euler(0, 0, angles.z);
            }
        }

        private void ServiceCCWState()
        {
            if (mStateFrameTick > kRotateFrames)
            {
                // Transite to next state
                mState = EnemyState.eChaseState;
                mStateFrameTick = 0;
            }
            else
            {
                mStateFrameTick++;

                Vector3 angles = transform.rotation.eulerAngles;
                angles.z -= kRotateRate;
                transform.rotation = Quaternion.Euler(0, 0, angles.z);
            }
        }
        #endregion

        private void ServicePatrolState()
        {
            sWayPoints.CheckNextWayPoint(transform.position, ref mWayPointIndex);
            PointAtPosition(sWayPoints.WayPoint(mWayPointIndex), kTurnRate);
            transform.position += (kSpeed * Time.smoothDeltaTime) * transform.up;
        }

        private void ServiceChaseState()
        {
            Debug.Assert(mTarget != null);
            if (Vector3.Distance(transform.position, mTarget.transform.position) < kChaseDistance)
            {
                PointAtPosition(mTarget.transform.position, kChaseTurnRate);
                transform.position += (kSpeed * Time.smoothDeltaTime) * transform.up;
            }
            else
            {
                // done
                mState = EnemyState.eEnlargeState;
                mStateFrameTick = 0;
                mTarget = null;
            }
        }
        private void ServiceStunState()
        {
            Vector3 angles = transform.rotation.eulerAngles;
            angles.z -= kRotateRate;
            transform.rotation = Quaternion.Euler(0, 0, angles.z);
        }

        private void ServiceEggState()
        {
        }
    }
}