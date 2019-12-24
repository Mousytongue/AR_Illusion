using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    public class CSS385_Mp2_WayPoint : MonoBehaviour
    {
        private Vector3 mInitPosition = Vector3.zero;
        private int mHitCount = 0;
        private const int kHitLimit = 3;
        private const float kRepositionRange = 15f; // +- this value
        private Color mNormalColor = Color.white;

        // Start is called before the first frame update
        void Start()
        {
            Random.InitState(722018);
            mInitPosition = transform.position;
        }


        private void Reposition()
        {
            Vector3 p = mInitPosition;
            p += new Vector3(Random.Range(-kRepositionRange, kRepositionRange),
                             Random.Range(-kRepositionRange, kRepositionRange),
                             0f);
            transform.position = p;
            GetComponent<SpriteRenderer>().color = mNormalColor;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.name == "Egg(Clone)")
            {
                mHitCount++;
                Color c = mNormalColor * (float)(kHitLimit - mHitCount + 1) / (float)(kHitLimit + 1);
                GetComponent<SpriteRenderer>().color = c;
                if (mHitCount > kHitLimit)
                {
                    mHitCount = 0;
                    Reposition();
                }
            }
        }
    }
}