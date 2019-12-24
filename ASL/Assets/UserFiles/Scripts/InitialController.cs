using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GoogleARCore;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class InitialController : MonoBehaviour
{
    public Camera FirstPersonCamera;
    public Text UI_Snackbar;
    public GameObject PlaneVisualizerObj;
    public GameObject MainPlane;
    public FloorCaptureController FloorCaptureControl;
    public GameObject FloorControlCanvas;
    static public WorldOriginHelper WorldOriginHelp;

    //To keep track if a cloud anchor is needed 
    public bool IsOriginPlaced { get; private set; }
    private bool m_AnchorAlreadyInstantiated = false;
    private bool m_AnchorFinishedHosting = false;

    public bool auto_setup = true;

    public bool m_IsStarted = false;
    private bool m_IsQuitting = false;

    private Pose? m_LastHitPose = null;

    private static Component m_WorldOriginAnchor = null;

    public Button AnchorButton;

    void AnchorButtonPressed()
    {

        TrackableHit arcoreHitResult = new TrackableHit();
        m_LastHitPose = null;
        if (WorldOriginHelp.Raycast(1300, 800,
                        TrackableHitFlags.PlaneWithinPolygon, out arcoreHitResult))
        {
            m_LastHitPose = arcoreHitResult.Pose;
        }

        if (m_LastHitPose != null)
        {

            m_WorldOriginAnchor = arcoreHitResult.Trackable.CreateAnchor(arcoreHitResult.Pose);

            SetWorldOrigin(m_WorldOriginAnchor.transform);
            _InstantiateAnchor();


            //WorldOriginHelp.SetNoPlanes(true);
            SetFloorY(arcoreHitResult.Pose.position.y);
            IllusionModeStart();
            UI_Snackbar.text = "Take a picture of only your floor in the frame";
            m_IsStarted = true;
        }
    }

    public void Awake()
    {
        WorldOriginHelp = GameObject.Find("WorldOriginHelper").GetComponent<WorldOriginHelper>();
        AnchorButton.onClick.AddListener(delegate { AnchorButtonPressed(); });

        //Locks screen to portrait
        //Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToPortrait = false;
        //Screen.orientation = ScreenOrientation.Portrait;
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        _UpdateApplicationLifecycle();

        //If the y / cloud anchor is set, return
        if (m_IsStarted)
            return;

        if (auto_setup)
        {
            UI_Snackbar.text = "Initialize Settings Called";
            SetFloorY(-1f);
            FloorCaptureControl.SetFloorY(-1);
            WorldOriginHelp.SetNoPlanes(true);
            IllusionModeStart();
            m_IsStarted = true;

           
        }

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        // Raycast against the location the player touched to search for planes.
        TrackableHit arcoreHitResult = new TrackableHit();
        m_LastHitPose = null;
        if (WorldOriginHelp.Raycast(touch.position.x, touch.position.y,
                        TrackableHitFlags.PlaneWithinPolygon, out arcoreHitResult))
        {
            m_LastHitPose = arcoreHitResult.Pose;
        }

        if (m_LastHitPose != null)
        {

            m_WorldOriginAnchor = arcoreHitResult.Trackable.CreateAnchor(arcoreHitResult.Pose);

            SetWorldOrigin(m_WorldOriginAnchor.transform);
            _InstantiateAnchor();


            WorldOriginHelp.SetNoPlanes(true);
            SetFloorY(arcoreHitResult.Pose.position.y);
            FloorCaptureControl.SetFloorY(arcoreHitResult.Pose.position.y);
            IllusionModeStart();
           // UI_Snackbar.text = "Take a picture of only your floor in the frame";
            m_IsStarted = true;
        }
    }

    public void SetWorldOrigin(Transform anchorTransform)
    {
        if (IsOriginPlaced)
        {
            Debug.LogWarning("The World Origin can be set only once.");
            return;
        }

        IsOriginPlaced = true;

        WorldOriginHelp.SetWorldOrigin(anchorTransform);
    }

    void SetFloorY(float val)
    {
        MainPlane.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            MainPlane.GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(new Vector3(0, val, 0));
        });
    }

    void IllusionModeStart()
    {
        FloorControlCanvas.SetActive(true);
    }

    private void _InstantiateAnchor()
    {
        ASL.ASLHelper.InstanitateASLObject("AnchorPrefab", Vector3.zero, Quaternion.identity, "", "",
            AfterAnchorSpawnFunction, null, AnchorFloatsFunction);
    }

    public static void AfterAnchorSpawnFunction(GameObject _myGameObject)
    {
        _myGameObject.GetComponent<AnchorController>().HostLastPlacedAnchor(m_WorldOriginAnchor);
    }

    public static void AnchorFloatsFunction(string _id, float[] _myFloats)
    {
        if (_myFloats[3] == 99)
        {
            if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject myObject))
            {
                myObject.GetComponent<AnchorController>()._OnHost();
                WorldOriginHelp.SetNoPlanes(true);
            }
        }
    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to
        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage(
                "ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }

    public void SetSnackbarText(string text)
    {
        UI_Snackbar.text = text;
    }
}
