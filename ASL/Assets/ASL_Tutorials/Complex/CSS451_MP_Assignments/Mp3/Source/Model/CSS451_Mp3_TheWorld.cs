using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mps
{
    public partial class CSS451_Mp3_TheWorld : MonoBehaviour
    {

        public CSS451_Mp3_UILineSegment AimingLine;
        public CSS451_Mp3_UILineSegment BigLine;
        public CSS451_Mp3_Plane Barrier;
        public GameObject travelingBall;

        public CSS451_Mp3_SliderWithEcho IntervalControl = null;
        public CSS451_Mp3_SliderWithEcho SpeedControl = null;
        public CSS451_Mp3_SliderWithEcho DeathControl = null;

        const float kBigLineWidth = 3f;

        // to keep track of last time a ball is generated
        float mSinceLastGenerated = 10f;
        float mGenerateInterval = 1f;  // in second
        float mSpeed = 6f; // 1 unit per second
        float mAliveTime = 10f;

        // Use this for initialization
        void Start()
        {
            Debug.Assert(AimingLine != null);
            Debug.Assert(BigLine != null);
            Debug.Assert(Barrier != null);

            IntervalControl.GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(SetLocalBallInterval);
            SpeedControl.GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(SetLocalBallSpeed);
            DeathControl.GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(SetLocalAliveTime);

            BigLine.SetWidth(kBigLineWidth);
        }

        void Update()
        {
            mSinceLastGenerated += Time.deltaTime;
            if (mSinceLastGenerated >= mGenerateInterval)
            {
                GameObject g = Instantiate(travelingBall);
                CSS451_Mp3_TravelingBall b = g.GetComponent<CSS451_Mp3_TravelingBall>();
                b.Initialize(this);

                mSinceLastGenerated = 0f;
            }


        }

        public bool SetLineEndPoint(string onWall, Vector3 p)
        {
            return AimingLine.SetEndPoint(onWall, p) || BigLine.SetEndPoint(onWall, p);
        }


        // Setter/Getter
        public void SetNetworkBallSpeed(float s)
        {
            if (mSpeed != s) //Don't update if not necessary
            {
                SpeedControl.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
                {
                    float[] myFloatArray = new float[1];
                    myFloatArray[0] = s;
                    SpeedControl.GetComponent<ASL.ASLObject>()?.SendFloat4(myFloatArray);
                });
            }
        }
        public void SetLocalBallSpeed(string _id, float[] s)
        {
            mSpeed = s[0];
            SpeedControl.TheSlider.value = s[0];
            SpeedControl.TheEcho.text = s[0].ToString("0.0000");
        }
        public float GetBallSpeed() { return mSpeed; }
        public void SetNetworkBallInterval(float i)
        {
            if (mGenerateInterval != i) //Don't update if not necessary
            {
                IntervalControl.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
                {
                    float[] myFloatArray = new float[1];
                    myFloatArray[0] = i;
                    IntervalControl.GetComponent<ASL.ASLObject>()?.SendFloat4(myFloatArray);
                });
            }

        }
        public void SetLocalBallInterval(string _id, float[] f)
        {
            mGenerateInterval = f[0];
            IntervalControl.TheSlider.value = f[0];
            IntervalControl.TheEcho.text = f[0].ToString("0.0000");
        }
        public float GetBallInterval() { return mGenerateInterval; }
        public void SetNetworkAliveTime(float l)
        {
            if (mAliveTime != l) //Don't update if not necessary
            {
                DeathControl.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
                {
                    float[] myFloatArray = new float[1];
                    myFloatArray[0] = l;
                    DeathControl.GetComponent<ASL.ASLObject>()?.SendFloat4(myFloatArray);
                });
            }
        }
        public void SetLocalAliveTime(string _id, float[] l)
        {
            mAliveTime = l[0];
            DeathControl.TheSlider.value = l[0];
            DeathControl.TheEcho.text = l[0].ToString("0.0000");
        }
        public float GetAliveTime() { return mAliveTime; }

        public GameObject GetBarrierObject() { return Barrier.gameObject; }
        public CSS451_Mp3_LineSegment GetAimLine() { return AimingLine; }
    }
}