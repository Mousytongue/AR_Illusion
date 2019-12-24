using UnityEngine;

namespace Mps
{
    public partial class CSS451_Mp2_MainController : MonoBehaviour
    {

        // reference to all UI elements in the Canvas
        public Camera MainCamera = null;
        public CSS451_Mp2_XformControl mXform = null;
        public CSS451_Mp2_DropDownCreate mCreateMenu = null;
        public CSS451_Mp2_TheWorld mModel = null;


        // Use this for initialization
        void Start()
        {
            Debug.Assert(MainCamera != null);
            Debug.Assert(mXform != null);
            Debug.Assert(mModel != null);
            Debug.Assert(mCreateMenu != null);
        }

        // Update is called once per frame
        void Update()
        {
            LMBSelect();
        }

        private void SelectObject(GameObject g)
        {
            GameObject a = mModel.SelectObject(g);
            mXform.SetSelectedObject(a);
        }
    }
}