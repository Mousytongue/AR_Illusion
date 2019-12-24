﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mps
{
    public class CSS451_Mp3_XfromControl : MonoBehaviour
    {
        public Toggle T, R, S;
        public CSS451_Mp3_SliderWithEcho X, Y, Z;
        public Text ObjectName;

        private GameObject mSelected;
        private Vector3 mPreviousSliderValues = Vector3.zero;

        // Use this for initialization
        void Start()
        {
            T.onValueChanged.AddListener(SetToTranslation);
            R.onValueChanged.AddListener(SetToRotation);
            S.onValueChanged.AddListener(SetToScaling);
            X.SetSliderListener(XValueChanged);
            Y.SetSliderListener(YValueChanged);
            Z.SetSliderListener(ZValueChanged);

            T.isOn = true;
            R.isOn = false;
            S.isOn = false;
            SetToTranslation(true);
        }

        void SetToTranslation(bool v)
        {
            Vector3 p = GetSelectedXformParameter();
            mPreviousSliderValues = p;
            X.InitSliderRange(-20, 20, p.x);
            Y.InitSliderRange(-20, 20, p.y);
            Z.InitSliderRange(-20, 20, p.z);
        }

        void SetToScaling(bool v)
        {
            Vector3 s = GetSelectedXformParameter();
            mPreviousSliderValues = s;
            X.InitSliderRange(0.1f, 20, s.x);
            Y.InitSliderRange(0.1f, 20, s.y);
            Z.InitSliderRange(0.1f, 20, s.z);
        }

        void SetToRotation(bool v)
        {
            Vector3 r = GetSelectedXformParameter();
            mPreviousSliderValues = r;
            X.InitSliderRange(-180, 180, r.x);
            Y.InitSliderRange(-180, 180, r.y);
            Z.InitSliderRange(-180, 180, r.z);
        }

        void XValueChanged(float v)
        {
            Vector3 p = GetSelectedXformParameter();
            // if not in rotation, next two lines of work would be wasted
            float dx = v - mPreviousSliderValues.x;
            mPreviousSliderValues.x = v;
            Quaternion q = Quaternion.AngleAxis(dx, Vector3.right);
            p.x = v;
            SetSelectedXform(ref p, ref q);
        }

        void YValueChanged(float v)
        {
            Vector3 p = GetSelectedXformParameter();
            // if not in rotation, next two lines of work would be wasted
            float dy = v - mPreviousSliderValues.y;
            mPreviousSliderValues.y = v;
            Quaternion q = Quaternion.AngleAxis(dy, Vector3.up);
            p.y = v;
            SetSelectedXform(ref p, ref q);
        }

        void ZValueChanged(float v)
        {
            Vector3 p = GetSelectedXformParameter();
            // if not in rotation, next two lines of work would be wasterd
            float dz = v - mPreviousSliderValues.z;
            mPreviousSliderValues.z = v;
            Quaternion q = Quaternion.AngleAxis(dz, Vector3.forward);
            p.z = v;
            SetSelectedXform(ref p, ref q);
        }

        public void SetSelectedObject(GameObject g)
        {
            mSelected = g;
            mPreviousSliderValues = Vector3.zero;
            if (g != null)
                ObjectName.text = "Selected:" + g.name;
            else
                ObjectName.text = "Selected: none";
            ObjectSetUI();
        }

        public void ObjectSetUI()
        {
            Vector3 p = GetSelectedXformParameter();
            X.SetSliderValue(p.x);  // do not need to call back for this comes from the object
            Y.SetSliderValue(p.y);
            Z.SetSliderValue(p.z);
        }

        private Vector3 GetSelectedXformParameter()
        {
            Vector3 p;

            if (T.isOn)
            {
                if (mSelected != null)
                    p = mSelected.transform.localPosition;
                else
                    p = Vector3.zero;
            }
            else if (S.isOn)
            {
                if (mSelected != null)
                    p = mSelected.transform.localScale;
                else
                    p = Vector3.one;
            }
            else
            {
                p = Vector3.zero;
            }
            return p;
        }

        private void SetSelectedXform(ref Vector3 p, ref Quaternion q)
        {
            if (mSelected == null)
                return;

            Vector3 tempVector = p;
            Quaternion tempQuat = q;
            if (T.isOn)
            {
                mSelected.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
                {
                    mSelected.GetComponent<ASL.ASLObject>()?.SendAndSetLocalPosition(tempVector);
                });
            }
            else if (S.isOn)
            {
                mSelected.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
                {
                    mSelected.GetComponent<ASL.ASLObject>()?.SendAndSetLocalScale(tempVector);
                });
            }
            else
            {
                mSelected.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
                {
                    mSelected.GetComponent<ASL.ASLObject>()?.SendAndIncrementLocalRotation(tempQuat);
                });
            }
        }
    }
}