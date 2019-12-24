using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.RT;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ASL
{
    /// <summary>
    /// ASLObject_LocalFunctions: ASLObject Partial Class containing all of the functions and variables relating to local actions - actions that affect a single player instead of all players
    /// </summary>
    public partial class ASLObject : MonoBehaviour
    {
        /// <summary>How long a claim has been active on this object. This gets reset, unless otherwise specified, every time a player claims an object</summary>
        private float m_ClaimReleaseTimer = 0;
        /// <summary>How long a claim can be held for this object. This gets set every time a player claims an object</summary>
        private float m_ClaimTime = 0;
        /// <summary>Flag indicating if object is currently being used to upload a Texture2D as only 1 
        /// texture can be uploaded at a time to prevent race condition issues</summary>
        private bool m_CurrentlyUploadingTexture2D;
        /// <summary> A list of string data that holds information on the PostDownload function. Allows communication between two listener functions. </summary>
        private List<string> m_PostDownloadFunctionInfo;
        /// <summary>A list of upload ids for objects that have not yet been sent to everyone else for download</summary>
        private List<string> m_UploadIds;
        /// <summary> A list that holds the file size of Texture2Ds to be uploaded to help confirm which ASL object sent the Texture2D</summary>
        private List<int> m_Texture2DUploadSizeList;

        /// <summary>
        /// Flag indicating if this ASL object has been used to set/send a cloud anchor. Since cloud anchors are asynchronous, this prevents an ASL object from
        /// potentially being used set to multiple anchors and causing errors once those anchors are created
        /// </summary>
        private bool m_HaventSetACloudAnchor;

        /// <summary>
        /// Struct containing data types that allow the correct post download function to be executed when a user wants to sync
        /// </summary>
        public readonly struct PostDownloadInfo
        {
            /// <summary>The post download function to be executed</summary>
            public readonly PostDownloadFunction s_postDownloadFunction;
            /// <summary>The Texture2D that was uploaded/downloaded and associated with the passed in post download function</summary>
            public readonly Texture2D s_myTexture2D;

            /// <summary>
            /// The upload/download id of the Texture2D. It is used to determine which texture we are working with when a message comes in
            /// informing the user that they can execute a post download function now.
            /// </summary>
            public readonly string s_uploadId;

            /// <summary>
            /// Constructor for our PostDownloadInfo struct
            /// </summary>
            /// <param name="_postDownloadFunction">The post download function to be executed</param>
            /// <param name="_myTexture2D">The Texture2D that was uploaded/downloaded and associated with the passed in post download function</param>
            /// <param name="_uploadId">The upload/download id of the Texture2D. It is used to determine which texture we are working with when a message comes in
            /// informing the user that they can execute a post download function now.</param>
            public PostDownloadInfo(PostDownloadFunction _postDownloadFunction, Texture2D _myTexture2D, string _uploadId)
            {
                s_postDownloadFunction = _postDownloadFunction;
                s_myTexture2D = _myTexture2D;
                s_uploadId = _uploadId;
            }
        }

        /// <summary>
        /// A list of variables that contains information about the PostDownload function and its parameters, allowing synchronization control
        /// post download for all users
        /// </summary>
        public List<PostDownloadInfo> m_PostDownloadInfoList { get; private set; }

        /// <summary>Function that is executed upon object initialization</summary>
        private void Awake()
        {
            m_Id = string.Empty;
            m_Mine = false;
            m_OutStandingClaims = false;
            m_OutstandingClaimCallbackCount = 0;
            m_PostDownloadFunctionInfo = new List<string>();
            m_PostDownloadInfoList = new List<PostDownloadInfo>();
            m_UploadIds = new List<string>();
            m_Texture2DUploadSizeList = new List<int>();
            m_CurrentlyUploadingTexture2D = false;
        }

        /// <summary>Function that is executed upon object start</summary>
        private void Start()
        {
            //All GS Upload messages will be channeled through GetUploadMessage
            UploadCompleteMessage.Listener += GetUploadMessage;
            m_ResolvedCloudAnchor = false;
            m_HaventSetACloudAnchor = false;
        }

        /// <summary>
        /// Currently counts down this object's claim time and releases the object back to the relay server after the specified amount of time has passed.
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // If we own it and we have no callbacks to perform
            //Then we don't need to own it anymore and thus can start to cancel it
            if (m_Mine && m_OutstandingClaimCallbackCount == 0 && m_ClaimTime > 0 && m_ClaimTime != 0) //if 0, then hold onto until stolen
            {
                m_ClaimReleaseTimer += Time.deltaTime * 1000; //Translate to milliseconds
                if (m_ClaimReleaseTimer > m_ClaimTime) //If time to release our claim
                {
                    m_ReleaseFunction?.Invoke(gameObject); //If user wants to do something before object is released - let them do so
                    _LocallyRemoveReleaseCallback();
                    using (RTData data = RTData.Get())
                    {
                        data.SetString((int)GameController.DataCode.Id, m_Id);
                        GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.ReleaseClaimToServer, GameSparksRT.DeliveryIntent.RELIABLE, data);
                    }
                    m_Mine = false; //Release                    
                }
            }
        }

        /// <summary>
        /// Removes any outstanding claim callbacks for this object as well as sets the outstanding claim flag to false
        /// </summary>
        public void _LocallyRemoveClaimCallbacks()
        {
            m_OutStandingClaims = false;
            m_ClaimCallback = null;
        }

        /// <summary>
        /// This function should NOT be called by the user as it will only update the local player, it will not update all players. Claim the object as our own or give up the object. 
        /// This function is used when an incoming packet from the relay server needs to update the claim of this objects. 
        /// </summary>
        /// <param name="_claim">Based on what was sent, claim this is our own or relinquish it.</param>
        public void _LocallySetClaim(bool _claim)
        {
            m_Mine = _claim;
        }

        /// <summary>
        /// This function should NOT be called by the user as it will only update the local player, it will not update all players. Set the unique ID of this object. 
        /// This function is used when an incoming packet from the relay server needs to update the id of this object, this happens when the object is created. 
        /// </summary>
        /// <param name="_id">The new id</param>
        public void _LocallySetID(string _id)
        {
            m_Id = _id;
        }

        /// <summary>
        /// This function should NOT be called by the user as it will only update the local player, it will not update all players. Sets the anchor point for this ASL Object. 
        /// This function is used when an incoming packet from the relay server needs to update the anchor point of this objects. 
        /// </summary>
        /// <param name="_anchorId">The new anchor id for this object</param>
        public void _LocallySetAnchorID(string _anchorId)
        {
            m_AnchorID = _anchorId;
        }

        /// <summary>
        /// Locally sets the flag that allows this object to continue processing the cloud anchor
        /// </summary>
        /// <param name="_cloudAnchorResolved">Flag indicating if all clients have resolved this cloud anchor</param>
        public void _LocallySetCloudAnchorResolved(bool _cloudAnchorResolved)
        {
            m_ResolvedCloudAnchor = _cloudAnchorResolved;
        }

        /// <summary>
        /// This function will only update the local player, not all players. Sets the claim cancelled recovery callback function for this object. 
        /// </summary>
        /// <param name="_rejectedClaimRecoveryFunction">The cancelled claim user provided function</param>
        public void _LocallySetClaimCancelledRecoveryCallback(ClaimCancelledRecoveryCallback _rejectedClaimRecoveryFunction)
        {
            m_ClaimCancelledRecoveryCallback = _rejectedClaimRecoveryFunction;
        }

        /// <summary>
        /// This function will only update the local player, not all players. Sets callback function that will be called upon object creation for this object. 
        /// </summary>
        /// <param name="_yourUponCreationFunction">The function to be executed upon this object's creation, provided by the user</param>
        public void _LocallySetGameObjectCreatedCallback(ASLGameObjectCreatedCallback _yourUponCreationFunction)
        {
            m_ASLGameObjectCreatedCallback = _yourUponCreationFunction;
        }

        /// <summary>
        /// This function will only update the local player, not all players. Sets the float callback function for this object. 
        /// </summary>
        /// <param name="_yourFloatFunction">The function that will be used to perform an action whenever a user sends a float</param>
        /// <example><code>
        /// void Start()
        /// {
        ///     gameobject.GetComponent&lt;ASL.ASLObject&gt;()._LocallySetFloatCallback(UserDefinedFunction)
        /// }
        /// public void UserDefinedFunction(string _id, float[] f)
        /// {
        ///     //Update some value for all users based on f. 
        ///     //Example:
        ///     playerHealth = f[0]; //Where playerHealth is shown to kept track/shown to all users
        /// }
        ///</code></example>
        public void _LocallySetFloatCallback(FloatCallback _yourFloatFunction)
        {
            m_FloatCallback = _yourFloatFunction;
        }

        /// <summary>
        /// This function will only update the local player, not all players. Set the release function to be executed upon this object's release (when claim switches to false) 
        /// </summary>
        /// <param name="_releaseFunction">The user provided release function</param>
        public void _LocallySetReleaseFunction(ReleaseFunction _releaseFunction)
        {
            m_ReleaseFunction = _releaseFunction;
        }

        /// <summary>
        /// This function will only update the local player, not all players. Removes any release functions that may be attached to this object
        /// </summary>
        public void _LocallyRemoveReleaseCallback()
        {
            m_ReleaseFunction = null;
        }

        /// <summary>
        /// This function will only update the local player, not all players. It sets the function to be called after an image as been downloaded from the server
        /// </summary>
        /// <param name="_postDownloadFunction">The function to execute after the passed in Texture2D is downloaded from the server</param>
        /// /// <param name="_myTexture2D">The Texture2D that was downloaded from the server</param>
        /// /// <param name="_uploadId">The upload ID of the Texture2D that was downloaded from the server, used to find the same texture later</param>
        public void _LocallySetPostDownloadFunction(PostDownloadFunction _postDownloadFunction, Texture2D _myTexture2D, string _uploadId)
        {
            m_PostDownloadFunction = _postDownloadFunction;
            PostDownloadInfo myPostDownloadInfo = new PostDownloadInfo(_postDownloadFunction, _myTexture2D, _uploadId);
            m_PostDownloadInfoList.Add(myPostDownloadInfo);
        }

        /// <summary>
        /// This function will only update the local player, not all players. It removes the most recently called PostDownloadFunction from the PostDownloadInfoList
        /// </summary>
        /// <param name="_index">The index to remove from the PostDownloadInfoList</param>
        public void _LocallyRemovePostDownloadFunction(int _index)
        {
            if (m_PostDownloadInfoList.Count - 1 >= 0) //If there is something to remove
            {
                m_PostDownloadInfoList.RemoveAt(_index); //Remove which
            }
        }

        /// <summary>
        /// Changes the Texture2D into a byte array and then uploads that byte array to the GameSpark database. Once upload is completed, 
        /// it will send a packet to all users with the information needed to download the Texture2D. Note: Texture2D must be smaller than 3.5 MB
        /// Race condition can occur between UploadTexture2D function and this function. Measures have been put into place to help prevent this
        /// </summary>
        /// <param name="uploadUrl">The GameSparks generated upload URL</param>
        /// <param name="_myTexture2D">The Texture2D to be uploaded</param>
        /// <param name="_postDownloadFunctionInfo">The function to be called after downloading the Texture2D</param>
        /// <param name="_uploadAsPNG">Flag indicating if the Texture2D should be uploaded as a PNG (true) or JPG (false)</param>
        /// <param name="_syncStart">Flag indicating if once downloaded, if this texture2D should wait until all users have downloaded 
        /// before it executes the function it is attached to</param>
        /// <returns>Is a coroutine/IEnumerator, so waits asynchronously until the upload is complete or errors</returns>
        private IEnumerator UploadTexture2D(string uploadUrl, Texture2D _myTexture2D, string _postDownloadFunctionInfo, bool _uploadAsPNG, bool _syncStart)
        {
            while (m_CurrentlyUploadingTexture2D)
            {
                Debug.LogWarning("Object: " + gameObject.name + " " + m_Id + " is already uploading a Texture2D. Waiting for previous Texture2D to finish uploading...");
                yield return new WaitForSeconds(.5f);
            }

            byte[] bytes;
            //Change Texture2D into png 
            if (_uploadAsPNG)
            {
                bytes = _myTexture2D.EncodeToPNG(); //Can also encode to jpg, just make sure to change the file extensions down below
            }
            else
            {
                bytes = _myTexture2D.EncodeToJPG();
            }
            if (bytes.Length > 3500000)
            {
                Debug.LogError("Texture2D: " + _myTexture2D.name + " is too large to uploaded. Max size is 3.5 MB.");
                yield break; //End 
            }
            // Create a Web Form, this will be our POST method's data
            var form = new WWWForm();
            form.AddField("frameCount", Time.frameCount.ToString());
            if (_uploadAsPNG)
            {
                form.AddBinaryData("file", bytes, _myTexture2D.name + ".png", "image/png");
            }
            else
            {
                form.AddBinaryData("file", bytes, _myTexture2D.name + ".png", "image/jpg");
            }
                   
            ////POST the Texture2D byte file to GameSparks
            using (var upload = UnityWebRequest.Post(uploadUrl, form))
            {
                m_CurrentlyUploadingTexture2D = true;
                yield return upload.SendWebRequest();

                if (upload.isNetworkError || upload.isHttpError)
                {
                    print("Upload error: " + upload.error);
                }
                else
                {
                    //Finished - push info to list to be used and removed in GetUploadMessage (which will trigger now that upload is finished)
                    m_Texture2DUploadSizeList.Add(bytes.Length);
                    m_PostDownloadFunctionInfo.Add(_myTexture2D.name);
                    m_PostDownloadFunctionInfo.Add(_postDownloadFunctionInfo);
                    m_PostDownloadFunctionInfo.Add(System.Convert.ToInt32(_syncStart).ToString());
                    
                    if (m_UploadIds.Count > 0) //If GetUploadMessage has already assigned the Upload Id
                    {
                        InformAllUsersTexture2DReadyForDownload(); //Send out packet informing users to download Texture2D
                    }

                }
            }
        }

        /// <summary>
        /// Takes information gathered from two async functions and uses it to actually send the packet out to all users
        /// informing them that they can begin downloading the Texture2D. 
        /// </summary>
        /// <returns></returns>
        public void InformAllUsersTexture2DReadyForDownload()
        {
            m_CurrentlyUploadingTexture2D = false;
            //Send the upload ID to everyone so they can download the correct file
            using (RTData data = RTData.Get())
            {
                data.SetString((int)GameController.DataCode.Id, m_Id);
                data.SetString((int)GameController.DataCode.Texture2DName, m_PostDownloadFunctionInfo[0]);
                data.SetString((int)GameController.DataCode.Texture2DPostDownloadFunctionInfo, m_PostDownloadFunctionInfo[1]);
                data.SetString((int)GameController.DataCode.Texture2DSyncStartFlag, m_PostDownloadFunctionInfo[2]);
                data.SetString((int)GameController.DataCode.Texture2DUploadId, m_UploadIds[0]);
                GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.SendTexture2D, GameSparksRT.DeliveryIntent.RELIABLE, data);
                m_PostDownloadFunctionInfo.RemoveRange(0, 3); //Update list so we don't send repeat data
                m_UploadIds.RemoveAt(0); //Update list so we don't send repeat data
                m_Texture2DUploadSizeList.RemoveAt(0); //Update list so we don't send repeat data
            }
        }

        /// <summary>
        /// Our upload message listener. Triggers when an upload was successful, setting m_UploadId to be used in junction
        /// with m_PostDownloadFunctionInfo. Race condition can occur between this function and UploadTexture2D. Measures have
        /// been put into place to help prevent this
        /// </summary>
        /// <param name="message">The upload message that triggered this listener</param>
        private void GetUploadMessage(GSMessage message)
        {
            //Since GetUploadMessage is a listener, it gets triggered on all (locally) ASL objects, even those that did not send a texture
            //Therefore, we check if this ASL object was the sender (or at the very least, a sender, if the file size happens to be the same),
            //and if so, add the upload id. 
            foreach (var texture2DFileSize in m_Texture2DUploadSizeList)
            {
                message.BaseData.GetGSData("uploadData").BaseData.TryGetValue("fileSize", out var value);
                if (int.Parse(value.ToString()) == texture2DFileSize)
                {
                    m_UploadIds.Add(message.BaseData.GetString("uploadId"));
                    if (m_PostDownloadFunctionInfo.Count > 0) //If UploadTexture2D has completed (if upload completed before GetUpload message came back)
                    {
                        InformAllUsersTexture2DReadyForDownload(); //Send out packet informing users to download Texture2D
                    }
                    return;
                }
            }
        }
    }
}
