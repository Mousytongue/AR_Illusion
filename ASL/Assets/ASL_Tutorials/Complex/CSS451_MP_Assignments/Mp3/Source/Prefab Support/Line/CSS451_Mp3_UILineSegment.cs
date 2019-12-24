#define OurOwnRotation
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    // Assume hanging on a cylinder
    public class CSS451_Mp3_UILineSegment : CSS451_Mp3_LineSegment
    {
        public CSS451_Mp3_LineEndPt mEndP1;
        public CSS451_Mp3_LineEndPt mEndP2;

        //If value of mEndP1 or mEndP2 changes, call SetEndPoints(mEndP1, mEndP2)

        private Vector3 mOldP1;
        private Vector3 mOldP2;

        // Use this for initialization
        void Start()
        {
            Debug.Assert(mEndP1 != null);
            Debug.Assert(mEndP2 != null);
            base.SetEndPoints(mEndP1.GetPosition(), mEndP2.GetPosition());
            ComputeLineDetails();

            mOldP1 = mEndP1.transform.position;
            mOldP2 = mEndP2.transform.position;
        }

        private void Update()
        {
            if (mOldP1 != mEndP1.transform.localPosition || mOldP2 != mEndP2.transform.localPosition)
            {
                base.SetEndPoints(mEndP1.transform.localPosition, mEndP2.transform.localPosition);
                mOldP1 = mEndP1.transform.localPosition;
                mOldP2 = mEndP2.transform.localPosition;
            }
        }

        // setters
        public override void SetEndPoints(Vector3 p1, Vector3 P2)
        {
            base.SetEndPoints(p1, P2);
            mEndP1.SetPosition(p1);
            mEndP2.SetPosition(P2);
        }

        public bool SetEndPoint(string onWall, Vector3 p)
        {
            bool hit = (mEndP1.SetPosition(onWall, p) || mEndP2.SetPosition(onWall, p));
            if (hit)
            {
                base.SetEndPoints(mEndP1.transform.localPosition, mEndP2.transform.localPosition);
                ComputeLineDetails();
            }
            return hit;
        }

    }
}