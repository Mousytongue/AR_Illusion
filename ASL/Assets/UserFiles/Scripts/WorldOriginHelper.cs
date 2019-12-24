using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;


//This class changes the world origin when using cloud anchors
//This is done by modifying a parent object on the main camera

public class WorldOriginHelper : MonoBehaviour
{
    //Camera's parent transform
    public Transform ARCoreDeviceTransform;

    // A prefab for tracking and visualizing detected planes.
    public GameObject DetectedPlanePrefab;

    private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();

    private List<GameObject> m_PlanesBeforeOrigin = new List<GameObject>();

    //Indicates whether origin has been placed
    private bool m_IsOriginPlaced = false;

    //This will be the reference the shared world origin;
    private Transform m_AnchorTransform;

    private bool is_NoPlanes = false;

    public void SetNoPlanes(bool mode)
    {
        is_NoPlanes = mode;

        GameObject[] meshes = GameObject.FindGameObjectsWithTag("Mesh");
        foreach (GameObject mesh in meshes)
        {
            Debug.Log("Turn Off meshes called");
            mesh.SetActive(!mode);
        }

        //Turns off the plane detector within the device. May not be needed if using dynmaic spawning
        //FindObjectOfType<ARCoreSession>().SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Disabled;
    }

    // Update is called once per frame
    void Update()
    {
        //If i dont want more meshes, return out of here
        if (is_NoPlanes)
        {
            return;
        }

        // Check that motion tracking is tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        Pose worldPose = _WorldToAnchorPose(Pose.identity);

        // Iterate over planes found in this frame and instantiate corresponding GameObjects to
        // visualize them.
        Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.New);
        for (int i = 0; i < m_NewPlanes.Count; i++)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane. The
            // transform is set to the origin with an identity rotation since the mesh for our
            // prefab is updated in Unity World coordinates.
            GameObject planeObject = Instantiate(
                DetectedPlanePrefab, worldPose.position, worldPose.rotation, transform);
            planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(m_NewPlanes[i]);

            if (!m_IsOriginPlaced)
            {
                m_PlanesBeforeOrigin.Add(planeObject);
            }
        }

    }

    public void SetWorldOrigin(Transform anchorTransform)
    {
        if (m_IsOriginPlaced)
        {
            Debug.LogWarning("World origin cannot be set twice.");
            return;
        }

        m_IsOriginPlaced = true;

        m_AnchorTransform = anchorTransform;

        Pose worldPose = _WorldToAnchorPose(new Pose(ARCoreDeviceTransform.position,
                                                          ARCoreDeviceTransform.rotation));
        ARCoreDeviceTransform.SetPositionAndRotation(worldPose.position, worldPose.rotation);

        foreach (GameObject plane in m_PlanesBeforeOrigin)
        {
            if (plane != null)
            {
                plane.transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            }
        }
    }

    /// Performs a raycast against physical objects being tracked by ARCore. This function wraps
    /// <c>Frame.Raycast</c> to add the necessary offset if the WorldOrigin is moved when a
    /// Cloud Anchor is placed.
    public bool Raycast(float x, float y, TrackableHitFlags filter, out TrackableHit hitResult)
    {
        bool foundHit = Frame.Raycast(x, y, filter, out hitResult);
        if (foundHit)
        {
            Pose worldPose = _WorldToAnchorPose(hitResult.Pose);
            TrackableHit newHit = new TrackableHit(
                worldPose, hitResult.Distance, hitResult.Flags, hitResult.Trackable);
            hitResult = newHit;
        }

        return foundHit;
    }

    /// Converts a pose from Unity world space to Anchor-relative space.
    public Pose _WorldToAnchorPose(Pose pose)
    {
        if (!m_IsOriginPlaced)
        {
            return pose;
        }

        Matrix4x4 anchorTWorld = Matrix4x4.TRS(
            m_AnchorTransform.position, m_AnchorTransform.rotation, Vector3.one).inverse;

        Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
        Quaternion rotation = pose.rotation * Quaternion.LookRotation(
            anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

        return new Pose(position, rotation);
    }
}
