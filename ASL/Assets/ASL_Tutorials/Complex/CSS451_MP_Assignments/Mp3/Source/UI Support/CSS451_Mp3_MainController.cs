using System; // for assert
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for GUI elements: Button, Toggle

namespace Mps
{
    public partial class CSS451_Mp3_MainController : MonoBehaviour
    {

        // reference to all UI elements in the Canvas
        public Camera MainCamera = null;
        public CSS451_Mp3_XfromControl mXform = null;
        public CSS451_Mp3_TheWorld mModel = null;

        public CSS451_Mp3_SliderWithEcho IntervalControl = null;
        public CSS451_Mp3_SliderWithEcho SpeedControl = null;
        public CSS451_Mp3_SliderWithEcho DeathControl = null;


        // Use this for initialization
        void Start()
        {
            Debug.Assert(MainCamera != null);
            Debug.Assert(mXform != null);
            Debug.Assert(mModel != null);
            Debug.Assert(IntervalControl != null);
            Debug.Assert(SpeedControl != null);
            Debug.Assert(DeathControl != null);

            IntervalControl.SetSliderLabel("Interval");
            IntervalControl.InitSliderRange(0.5f, 4f, 1f);
            IntervalControl.SetSliderListener(mModel.SetNetworkBallInterval);
            mModel.SetNetworkBallInterval(IntervalControl.GetSliderValue());

            SpeedControl.SetSliderLabel("Speed");
            SpeedControl.InitSliderRange(0.5f, 15f, 6f);
            SpeedControl.SetSliderListener(mModel.SetNetworkBallSpeed);
            mModel.SetNetworkBallSpeed(SpeedControl.GetSliderValue());

            DeathControl.SetSliderLabel("Alive Sec");
            DeathControl.InitSliderRange(1, 15, 10);
            DeathControl.SetSliderListener(mModel.SetNetworkAliveTime);
            mModel.SetNetworkAliveTime(DeathControl.GetSliderValue());

            mXform.SetSelectedObject(mModel.GetBarrierObject());
        }

        // Update is called once per frame
        void Update()
        {
            LMBService();
        }
    }
}