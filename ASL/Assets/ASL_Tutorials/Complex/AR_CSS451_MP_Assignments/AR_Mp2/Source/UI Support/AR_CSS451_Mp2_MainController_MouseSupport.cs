using GoogleARCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

namespace Mps
{
    public partial class AR_CSS451_Mp2_MainController : MonoBehaviour
    {
        public Text m_CloudAnchorFeedback;
        // Mouse click selection 
        void FingerSelect()
        {
            Touch touch;

            // If the player has not touched the screen then the update is complete.
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId)) // check for GUI
            {
                //Ray cast to check hit location
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(MainCamera.ScreenPointToRay(Input.GetTouch(0).position), out hitInfo, Mathf.Infinity, 1);

                if (hit)// if we hit a game object (does not detect if we hit a trackable plane to set cloud anchor)
                {
                    SelectObject(hitInfo.transform.gameObject);
                }                   
                else //if we did not hit a game object
                {
                    //If we haven't set a cloud anchor yet && we are the Host -> then we can set a cloud anchor
                    //Note: ASL does not prevent users from create unlimited cloud anchors, this means if two users create a cloud anchor at the same time
                    //With the same ASL object, there is the chance things will become out of sync. This if statement is one way to avoid that synchronization problem
                    if (ASL.ASLHelper.m_CloudAnchors.Count <= 0 && ASL.GameSparksManager.Instance().AmLowestPeer()) 
                    {
                        TrackableHit arcoreHitResult = new TrackableHit();
                        Pose? m_LastHitPose = null;

                        // Raycast against the location the player touched to search for planes. -> used for setting cloud anchor
                        if (ASL.ARWorldOriginHelper.Instance().Raycast(touch.position.x, touch.position.y,
                                TrackableHitFlags.PlaneWithinPolygon, out arcoreHitResult))
                        {
                            m_LastHitPose = arcoreHitResult.Pose;
                        }

                        // If there was a successful hit on a trackable plane -> create a cloud anchor
                        if (m_LastHitPose != null)
                        {
                            DetermineCloudAnchorCreationStyle(arcoreHitResult);
                        }
                    }
                    else //If we haven't set a cloud anchor in the scene yet
                    {
                        m_CloudAnchorFeedback.text = "";
                        //if we have, then select nothing
                        SelectObject(null);
                    }
                }
                    
            }
            
        }
   
        //Select what to do based upon what button was selected in the beginning of the scene
        void DetermineCloudAnchorCreationStyle(TrackableHit _hit)
        {
            m_CloudAnchorFeedback.text = "Creating cloud anchor...";
            //Hit result, ASLObject to follow anchor (by becoming a child at (0,0,0), function to call after creation, sync start or not, set world origin or not 
            //ASL.ASLHelper.CreateARCoreCloudAnchor(, , , , );
            switch (mModel.m_CloudAnchorExecutionType)
            {
                case 0:
                    // Parent "TheWorld" to CLoud Anchor - no world origin
                    mModel.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                    {
                        ASL.ASLHelper.CreateARCoreCloudAnchor(_hit, mModel.GetComponent<ASL.ASLObject>(), null, true, false);
                    });                  
                    break;
                case 1:
                    //Parent "TheWorld" to Cloud Anchor - Cloud Anchor is world origin
                    mModel.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                    {
                        ASL.ASLHelper.CreateARCoreCloudAnchor(_hit, mModel.GetComponent<ASL.ASLObject>(), null, true, true);
                    });                   
                    break;
                case 2:
                    //Create world origin with 1 cloud anchor and then create another cloud anchor and parent "TheWorld" to it
                    ASL.ASLHelper.CreateARCoreCloudAnchor(_hit, null, ParentToAnchorWithOriginBeingDifferentAnchor, true, true);
                    break;
                case 3:
                    // No world origin, set "TheWorld"'s initial position to that of the cloud anchor's (not parenting)
                    ASL.ASLHelper.CreateARCoreCloudAnchor(_hit, null, SetInitialPositionToCloudAnchorsInitialPosition, true, false);
                    break;
                case 4:
                    // Set world origin and set "TheWorld"'s initial position to that of the cloud anchor's (That of the world's origin - no parent)
                    ASL.ASLHelper.CreateARCoreCloudAnchor(_hit, null, SetInitialPositionToCloudAnchorsInitialPosition, true, true);
                    break;
                case 5:
                    //Create world origin with 1 cloud anchor and then create another cloud anchor and set "TheWorld"'s initial position to that of that cloud's anchor (no parent)
                    ASL.ASLHelper.CreateARCoreCloudAnchor(_hit, null, AnchorWithOriginBeingDifferentAnchor, true, true);
                    break;
                default:
                    break;
            }
            

        }

        /// <summary>
        /// Used to do exactly what the function name says
        /// </summary>
        /// <param name="_gameObject">The gameobject that comes with the post cloud anchor function - is not used here</param>
        /// <param name="_worldOriginAnchorSpawnLocation">The original cloud anchor trackable</param>
        public void ParentToAnchorWithOriginBeingDifferentAnchor(GameObject _gameObject, TrackableHit _worldOriginAnchorSpawnLocation)
        {
            //Place the new anchor at the world origin (but not the location of the world origin anchor - yes they're different and yes its annoying)
            TrackableHit nextCloudAnchorTrackable = new TrackableHit(new Pose(Vector3.zero, _worldOriginAnchorSpawnLocation.Pose.rotation),
                _worldOriginAnchorSpawnLocation.Distance, _worldOriginAnchorSpawnLocation.Flags, _worldOriginAnchorSpawnLocation.Trackable);

            mModel.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                //Parent "TheWorld" to Cloud Anchor - Cloud Anchor is world origin
                ASL.ASLHelper.CreateARCoreCloudAnchor(nextCloudAnchorTrackable, mModel.GetComponent<ASL.ASLObject>(), null, true, false);
            });

        }

        /// <summary>
        /// Used to set the position of "TheWorld" to that of the appropriate cloud anchor, but doesn't follow that cloud anchor if it changes as it is not parented to it
        /// </summary>
        /// <param name="_gameObject">The gameobject that comes with the post cloud anchor function - is not used here</param>
        /// <param name="_cloudAnchorSpawnLocation">The original cloud anchor trackable</param>
        public void SetInitialPositionToCloudAnchorsInitialPosition(GameObject _gameObject, TrackableHit _cloudAnchorSpawnLocation)
        {
            //Doing this will set this position, if world origin is set, to (0, 0, 0). If not, then it'll set to whatever position the cloud anchor is at
            mModel.GetComponent<ASL.ASLObject>().SendAndSetClaim(() => 
            { 
                mModel.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(_gameObject.transform.position);
                mModel.GetComponent<ASL.ASLObject>().SendAndSetLocalRotation(_gameObject.transform.rotation);
            });
        }

        /// <summary>
        /// Used to do exactly what the function name says
        /// <param name="_gameObject">The gameobject that comes with the post cloud anchor function - is not used here</param>
        /// <param name="_worldOriginAnchorSpawnLocation">The original cloud anchor trackable</param>
        /// </summary>
        public void AnchorWithOriginBeingDifferentAnchor(GameObject _gameObject, TrackableHit _worldOriginAnchorSpawnLocation)
        {
            //Place the new anchor at the world origin (but not the location of the world origin anchor - yes they're different and yes its annoying)
            TrackableHit nextCloudAnchorTrackable = new TrackableHit(new Pose(Vector3.zero, _worldOriginAnchorSpawnLocation.Pose.rotation),
                _worldOriginAnchorSpawnLocation.Distance, _worldOriginAnchorSpawnLocation.Flags, _worldOriginAnchorSpawnLocation.Trackable);


            //Set "TheWorld"'s initial position to the new cloud anchor's initial position
            ASL.ASLHelper.CreateARCoreCloudAnchor(nextCloudAnchorTrackable, null, SetInitialPositionToCloudAnchorsInitialPosition, true, false);
        }

    }
}