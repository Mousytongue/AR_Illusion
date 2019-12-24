using UnityEngine;
using UnityEngine.UI;

namespace Mps
{
    public class CSS451_Mp2_XformControl : MonoBehaviour
    {
        public Toggle T, R, S;
        public CSS451_Mp2_SliderWithEcho X, Y, Z;
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

        //---------------------------------------------------------------------------------
        // Initialize slider bars to specific function
        void SetToTranslation(bool v)
        {
            Vector3 p = ReadObjectXfrom();
            mPreviousSliderValues = p;
            X.InitSliderRange(-20, 20, p.x);
            Y.InitSliderRange(-20, 20, p.y);
            Z.InitSliderRange(-20, 20, p.z);
        }

        void SetToScaling(bool v)
        {
            Vector3 s = ReadObjectXfrom();
            mPreviousSliderValues = s;
            X.InitSliderRange(0.1f, 20, s.x);
            Y.InitSliderRange(0.1f, 20, s.y);
            Z.InitSliderRange(0.1f, 20, s.z);
        }

        void SetToRotation(bool v)
        {
            Vector3 r = ReadObjectXfrom();
            mPreviousSliderValues = r;
            X.InitSliderRange(-180, 180, r.x);
            Y.InitSliderRange(-180, 180, r.y);
            Z.InitSliderRange(-180, 180, r.z);
            mPreviousSliderValues = r;
        }
        //---------------------------------------------------------------------------------

        //---------------------------------------------------------------------------------
        // respond to slider bar value changes
        void XValueChanged(float v)
        {
            Vector3 p = ReadObjectXfrom();
            // if not in rotation, next two lines of work would be wasted
            float dx = v - mPreviousSliderValues.x;
            mPreviousSliderValues.x = v;
            Quaternion q = Quaternion.AngleAxis(dx, Vector3.right);
            p.x = v;
            UISetObjectXform(ref p, ref q);
        }

        void YValueChanged(float v)
        {
            Vector3 p = ReadObjectXfrom();
            // if not in rotation, next two lines of work would be wasted
            float dy = v - mPreviousSliderValues.y;
            mPreviousSliderValues.y = v;
            Quaternion q = Quaternion.AngleAxis(dy, Vector3.up);
            p.y = v;
            UISetObjectXform(ref p, ref q);
        }

        void ZValueChanged(float v)
        {
            Vector3 p = ReadObjectXfrom();
            // if not in rotation, next two lines of work would be wasted
            float dz = v - mPreviousSliderValues.z;
            mPreviousSliderValues.z = v;
            Quaternion q = Quaternion.AngleAxis(dz, Vector3.forward);
            p.z = v;
            UISetObjectXform(ref p, ref q);
        }
        //---------------------------------------------------------------------------------

        // new object selected
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
            Vector3 p = ReadObjectXfrom();
            X.SetSliderValue(p.x);  // do not need to call back for this comes from the object
            Y.SetSliderValue(p.y);
            Z.SetSliderValue(p.z);
        }

        private Vector3 ReadObjectXfrom()
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
                p = Vector3.zero; //Too hard to determine old rotation values for slider bar due to Quaternions, thus just set back to 0
            }
            return p;
        }

        private void UISetObjectXform(ref Vector3 p, ref Quaternion q)
        {
            if (mSelected == null)
                return;

            if (T.isOn)
            {
                Vector3 tempVector = p;
                mSelected.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    mSelected.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(tempVector);
                }, 0); //Hold until stolen
            }
            else if (S.isOn)
            {
                Vector3 tempVector = p;
                mSelected.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    mSelected.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(tempVector);
                }, 0); //Hold until stolen
            }
            else
            {
                Quaternion tempQuaternion = q;
                mSelected.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    mSelected.GetComponent<ASL.ASLObject>().SendAndIncrementLocalRotation(tempQuaternion);
                }, 0); //Hold until stolen
            }
        }

    }
}