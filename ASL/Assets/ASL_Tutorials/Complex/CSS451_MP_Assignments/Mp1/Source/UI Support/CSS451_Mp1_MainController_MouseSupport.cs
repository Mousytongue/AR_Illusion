using UnityEngine;

namespace Mps
{
    public partial class CSS451_Mp1_MainController : MonoBehaviour
    {

        // Mouse click selection 
        // Mouse click checking
        void CheckMouseClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("Mouse is down");

                RaycastHit hitInfo = new RaycastHit();
                // Note: mTarget is in a layer not rayCast!!

                bool hit = Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, 1);
                // 1 is the mask for default layer
                if (hit)
                {
                    TheWorld.SelectObjectAt(hitInfo.transform.gameObject, hitInfo.point);
                    // Main controller SHOULD NOT know anything about what
                    // user wants to do with mouse click
                }
                else
                {
                    Debug.Log("No hit");
                }
            }
        }
    }
}