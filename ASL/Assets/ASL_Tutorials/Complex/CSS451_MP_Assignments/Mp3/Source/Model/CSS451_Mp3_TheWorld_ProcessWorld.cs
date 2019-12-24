using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    public partial class CSS451_Mp3_TheWorld : MonoBehaviour
    {

        float kBigLineRadius = kBigLineWidth / 2f;
        float kDistToBigToShow = 10f;

        public bool ProcessBarrier(CSS451_Mp3_TravelingBall b, Transform shadowXform)
        {
            bool castShadow = false;
            if (Barrier.PtInfrontOf(b.GetPosition()))
            {
                float d = Barrier.DistantToPoint(b.GetPosition());
                Vector3 onBarrier = b.GetPosition() - Barrier.GetNormal() * d;

                if (Barrier.InActiveZone(onBarrier))
                {
                    castShadow = true;
                    // first, process shadow
                    Quaternion q = Quaternion.FromToRotation(Vector3.up, Barrier.GetNormal());
                    shadowXform.localRotation = q;
                    shadowXform.localPosition = onBarrier + Barrier.GetNormal() * 0.1f; // slight offet
                    shadowXform.localScale = new Vector3(1f, 0.1f, 1f);

                    if (Mathf.Abs(d) < 0.1f) // close enough
                    {
                        if (b.TravelTowards(Barrier.GetNormal()))
                            b.ReflectDir(Barrier.GetNormal());
                    }
                }
            }
            return castShadow;
        }

        public bool ProcessBigLine(CSS451_Mp3_TravelingBall b, Transform shadowXform)
        {
            bool castShadow = false;

            Vector3 ptOnLine;
            float d = BigLine.DistantToPoint(b.GetPosition(), out ptOnLine);
            if ((d > 0) && (d < kDistToBigToShow))
            {
                castShadow = true;
                Vector3 n = b.GetPosition() - ptOnLine;
                n.Normalize();
                shadowXform.localPosition = ptOnLine + kBigLineRadius * n;
                if (d <= kBigLineRadius)
                    b.ReflectDir(n);
            }
            return castShadow;
        }


    }
}