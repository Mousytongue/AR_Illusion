using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudAnchorController : MonoBehaviour
{
    public WorldOriginHelper WorldOriginHelp;

    public bool IsOriginPlaced { get; private set; }
    private bool m_AnchorAlreadyInstantiated = false;
    private bool m_AnchorFinishedHosting = false;
    private bool m_PassedResolvingPreparedTime = false;
    private const float k_ResolvingPrepareTime = 3.0f;

    // The anchor component that defines the shared world origin.
    private Component m_WorldOriginAnchor = null;

    // The current cloud anchor mode.
    private ApplicationMode m_CurrentMode = ApplicationMode.Ready;

    public enum ApplicationMode
    {
        Ready,
        Hosting,
        Resolving,
    }
    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "CloudAnchorController";
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

