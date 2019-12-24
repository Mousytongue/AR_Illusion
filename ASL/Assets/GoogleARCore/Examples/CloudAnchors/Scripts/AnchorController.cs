//-----------------------------------------------------------------------
// <copyright file="AnchorController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------


using GoogleARCore;
using GoogleARCore.CrossPlatform;
using UnityEngine;

public class AnchorController : MonoBehaviour
{
    //Anchor ID
    private bool is_Host = false;
    private string m_CloudAnchorId = string.Empty;

    //Host? Should Resolve?
    public bool m_ShouldResolve = false;

    private InitialController m_InitialController;

    public void Awake()
    {
        m_InitialController = GameObject.Find("InitialController").GetComponent<InitialController>();
    }

    public void Start()
    {
        if (m_CloudAnchorId != string.Empty)
        {
            m_ShouldResolve = true;
        }
    }

    public void Update()
    {
        //if (!m_ShouldResolve)
        //{
        //    return;
        //}

        //if (this.gameObject.GetComponent<ASL.ASLObject>().m_AnchorPoint == string.Empty)
        //    return;

        //_ResolveAnchorFromId(this.gameObject.GetComponent<ASL.ASLObject>().m_AnchorPoint);
    }

    /// <summary>
    /// Hosts the user placed cloud anchor and associates the resulting Id with this object.
    /// </summary>
    /// <param name="lastPlacedAnchor">The last placed anchor.</param>
    public void HostLastPlacedAnchor(Component lastPlacedAnchor)
    {
    //    is_Host = true;


    //    var anchor = (Anchor)lastPlacedAnchor;
    //    XPSession.CreateCloudAnchor(anchor).ThenAction(result =>
    //    {
    //        m_InitialController.SetSnackbarText(result.Response.ToString());
    //        if (result.Response != CloudServiceResponse.Success)
    //        {
    //            Debug.Log(string.Format("Failed to host Cloud Anchor: {0}", result.Response));
    //            return;
    //        }

    //        Debug.Log(string.Format(
    //            "Cloud Anchor {0} was created and saved.", result.Anchor.CloudId));
    //        this.gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
    //        { 
    //            this.gameObject.GetComponent<ASL.ASLObject>().SendAndSetAnchorPoint(result.Anchor.CloudId);
    //            float[] key = { 0, 0, 0, 99 };
    //            this.gameObject.GetComponent<ASL.ASLObject>().SendFloat4(key);
    //        });
    //});
    }

 

    // Resolves an anchor id and instantiates an Anchor prefab on it.
    private void _ResolveAnchorFromId(string cloudAnchorId)
    {
        // If device is not tracking, let's wait to try to resolve the anchor.
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }
        m_ShouldResolve = false;

        XPSession.ResolveCloudAnchor(cloudAnchorId).ThenAction(
            (System.Action<CloudAnchorResult>)(result =>
                {
                    if (result.Response != CloudServiceResponse.Success)
                    {
                        Debug.LogError(string.Format(
                            "Client could not resolve Cloud Anchor {0}: {1}",
                            cloudAnchorId, result.Response));

                        m_ShouldResolve = true;
                        return;
                    }

                    Debug.Log(string.Format(
                        "Client successfully resolved Cloud Anchor {0}.",
                        cloudAnchorId));

                    m_InitialController.SetSnackbarText(result.Response.ToString());
                    _OnResolved(result.Anchor.transform);
                }));
    }


    // Callback invoked once the Cloud Anchor is resolved.
    private void _OnResolved(Transform anchorTransform)
    {
        var initialController = GameObject.Find("InitialController").GetComponent<InitialController>();
        m_InitialController.SetWorldOrigin(anchorTransform);
    }

    public void _OnHost()
    {
        if (!is_Host)
            m_ShouldResolve = true;
    }

}

