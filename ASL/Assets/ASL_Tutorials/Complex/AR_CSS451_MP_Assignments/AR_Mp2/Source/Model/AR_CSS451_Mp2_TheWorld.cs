using UnityEngine;

namespace Mps
{
    public partial class AR_CSS451_Mp2_TheWorld : MonoBehaviour
    {
        private GameObject mSelected = null;
        private Color kSelectedColor = new Color(0.8f, 0.8f, 0.1f, 0.5f);
        private Color kOpponentSelectedColor = new Color(0.1647059f, 0.7606649f, 1f, .5f);
        private Color mOrgObjColor = Color.white; // remember obj's original color
        public AR_CSS451_Mp2_XformControl mXform;

        public int m_CloudAnchorExecutionType;

        // Use this for initialization
        void Start()
        {
            // OK, this is a little ugly ...
            mSelected = GameObject.Find("GrandParent");
            ShowSelectedAxisFrame(false, mSelected);
            mSelected = GameObject.Find("Parent");
            ShowSelectedAxisFrame(false, mSelected);
            mSelected = GameObject.Find("Child");
            ShowSelectedAxisFrame(false, mSelected);
            mSelected = null;
        }

        public GameObject SelectObject(GameObject obj)
        {
            if ((obj != null) && (obj.name == "CreationPlane" || obj.transform.parent?.name == "AxisFrame"))
            {
                obj = null;
            }

            SetObjectSelection(obj);
            return mSelected;
        }

        private void SetObjectSelection(GameObject g)
        {
            //If old selected is not null and is an ASL Object
            if (mSelected?.GetComponent<ASL.ASLObject>() != null)
            {
                GameObject oldSelected = mSelected; //Obtain a pointer to this object as it will change before claim comes through
                mSelected.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    //Release this object as we are going to select a new object(whatever g is)
                    oldSelected.GetComponent<ASL.ASLObject>().m_ReleaseFunction.Invoke(oldSelected);
                    oldSelected.GetComponent<ASL.ASLObject>()._LocallyRemoveReleaseCallback();
                }, 1000, false); //Don't reset release timer - release asap.
                ShowSelectedAxisFrame(false, mSelected);
            }

            mSelected = g;
            if (mSelected?.GetComponent<ASL.ASLObject>() != null)
            {
                mOrgObjColor = g.GetComponent<AR_CSS451_Mp2_ColorSupport>().m_OrgObjColor; // save a copy of original color
                GameObject currentSelected = mSelected;
                mSelected.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    currentSelected.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(kSelectedColor, kOpponentSelectedColor);
                    currentSelected.GetComponent<ASL.ASLObject>()._LocallySetReleaseFunction(OnRelease);
                }, 0); //Hold until stolen
                ShowSelectedAxisFrame(true, mSelected);
            }
        }

        private void OnRelease(GameObject _myGameObject)
        {
            //If this is an ASL object:
            if (_myGameObject?.GetComponent<ASL.ASLObject>() != null)
            {
                //Update color - because we are in OnRelease, we already own the object 
                //and therefore don't need to do a claim
                _myGameObject.GetComponent<ASL.ASLObject>()?.SendAndSetObjectColor(mOrgObjColor, mOrgObjColor);

                //Remove axis frame on this object
                ShowSelectedAxisFrame(false, _myGameObject);
            }

            //If we are releasing the current object we have selected - update GUI
            if (_myGameObject?.GetComponent<ASL.ASLObject>()?.m_Id == mSelected?.GetComponent<ASL.ASLObject>()?.m_Id)
            {
                mSelected = null;
                mXform.SetSelectedObject(mSelected); //Update UI to null value
            }

        }

        private void ShowSelectedAxisFrame(bool on, GameObject go)
        {
            bool found = false;
            if (go != null)
            {
                int i = 0;
                while ((!found) && (i < go.transform.childCount))
                {
                    Transform g = go.transform.GetChild(i);
                    if (g.gameObject.name == "AxisFrame")
                    {
                        for (int gi = 0; gi < g.childCount; gi++)
                        {
                            Transform ax = g.GetChild(gi);
                            ax.GetComponent<Renderer>().enabled = on;
                        }
                        found = true;
                    }
                    i = i + 1;
                }
            }
        }
    }
}