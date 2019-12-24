using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
// NOTE:
// - InstantPreviewInput does not support `deltaPosition`.
// - InstantPreviewInput does not support input from
//   multiple simultaneous screen touches.
// - InstantPreviewInput might miss frames. A steady stream
//   of touch events across frames while holding your finger
//   on the screen is not guaranteed.
// - InstantPreviewInput does not generate Unity UI event system
//   events from device touches. Use mouse/keyboard in the editor
//   instead.
using Input = GoogleARCore.InstantPreviewInput;
#endif

namespace SimpleDemos
{
    public class ARCloudAnchor_Example : MonoBehaviour
    {
        /// <summary>
        /// Object that will be parented to the cloud anchor
        /// </summary>
        public GameObject m_ObjectToPairWithCloudAnchor;
        
        /// <summary>
        /// Update function - checks for user input and then creates a cloud anchor wherever the user tapped
        /// </summary>
        void Update()
        {
            Touch touch;
            // If the player has not touched the screen then the update is complete.
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            Debug.Log("Touch: " + touch);

            // Ignore the touch if it's pointing on UI objects.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            TrackableHit arcoreHitResult = new TrackableHit();
            Pose? m_LastHitPose = null;

            // Raycast against the location the player touched to search for planes.
            if (ASL.ARWorldOriginHelper.Instance().Raycast(touch.position.x, touch.position.y,
                    TrackableHitFlags.PlaneWithinPolygon, out arcoreHitResult))
            {
                m_LastHitPose = arcoreHitResult.Pose;
            }

            // If there was a successful hit
            //If we haven't set a cloud anchor yet && we are the Host -> then we can set a cloud anchor
            //Note: ASL does not prevent users from create unlimited cloud anchors, this means if two users create a cloud anchor at the same time
            //With the same ASL object, there is the chance things will become out of sync. This if statement is one way to avoid that synchronization problem
            if (m_LastHitPose != null && ASL.ASLHelper.m_CloudAnchors.Count <= 0 && ASL.GameSparksManager.Instance().AmLowestPeer())
            {
                m_ObjectToPairWithCloudAnchor.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    //Hit result, ASLObject to follow anchor (by becoming a child at (0,0,0), function to call after creation, sync start or not, set world origin or not 
                    ASL.ASLHelper.CreateARCoreCloudAnchor(arcoreHitResult, m_ObjectToPairWithCloudAnchor.GetComponent<ASL.ASLObject>(), null, true, true);
                });
                
            }

        }
    }
}