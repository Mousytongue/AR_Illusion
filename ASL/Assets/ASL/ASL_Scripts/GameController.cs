using GameSparks.RT;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ASL
{
    /// <summary>
    /// GameController: Part of the GameController class that handles everything specific to GameController or used by just the GameController class
    /// </summary>
    public partial class GameController : MonoBehaviour
    {
        /// <summary> The GameController singleton. This is an unique variable - only 1 exists at a time. </summary>
        private static GameController m_Instance = null;
        private int m_ObjectIDAssignedCount = 0;
        private GameObject m_PauseCanvas;
        private GameObject m_PauseText;

        /// <summary>
        /// The DataCode that allows us to strip data from packets. If you update this list, make sure you update the same list on the GameSparksPortal page.
        /// </summary>
        public enum DataCode
        {
            /// <summary>DataCode does not recognize 0 as value, this is here to ensure our actual Data Codes start at 1</summary>
            BadValue,
            /// <summary>The DataCode that represents our object's id. This is used in almost every packet.</summary>
            Id,
            /// <summary>The DataCode that represents our local position</summary>
            Position,
            /// <summary>The DataCode that represents our local rotation</summary>
            Rotation,
            /// <summary>The DataCode that represents our local scale</summary>
            Scale,
            /// <summary>The DataCode that represents the primitive type of an object</summary>
            PrimitiveType,
            /// <summary>The DataCode that represents the id of a parent object</summary>
            ParentId,
            /// <summary>The DataCode that represents the color we will set the object we own to</summary>
            MyColor,
            /// <summary>The DataCode that represents the color we will set the object we own to for everyone else</summary>
            OpponentColor,
            /// <summary>The DataCode that represents the player id</summary>
            Player,
            /// <summary>The DataCode that represents the old player's id</summary>
            OldPlayer,
            /// <summary>The DataCode that represents a player's peer id</summary>
            PlayerPeerId,
            /// <summary>The DataCode that represents a float value</summary>
            MyFloats,
            /// <summary>The DataCode that represents the name of a prefab</summary>
            PrefabName,
            /// <summary>The DataCode that represents the name of the user defined claim recovery function name</summary>
            ClaimRecoveryFunctionName,
            /// <summary>The DataCode that represents the name of the user defined claim recovery class name</summary>
            ClaimRecoveryClassName,
            /// <summary>The DataCode that represents the name of the user defined post instantiated game object class name</summary>
            InstantiatedGameObjectClassName,
            /// <summary>The DataCode that represents the name of the user defined post instantiated game object function name</summary>
            InstantiatedGameObjectFunctionName,
            /// <summary>The DataCode that represents the anchor point of an object</summary>
            AnchorID,
            /// <summary>The DataCode that represents the name of a scene</summary>
            SceneName,
            /// <summary>The DataCode that represents the name of the user defined <see cref="ASLObject.FloatCallback"/> class name</summary>
            SendFloatClassName,
            /// <summary>The DataCode that represents the name of the user defined <see cref="ASLObject.FloatCallback"/> function name</summary>
            SendFloatFunctionName,
            /// <summary>The DataCode that represents the name of the component with its namespace that the user wants to add to an object for all other users</summary>
            ComponentName,
            /// <summary>The DataCode that represents the upload ID of a Texture2D, allowing other users to find and download a specific Texture2D</summary>
            Texture2DUploadId,
            /// <summary>The DataCode that represents the name of the Texture2D that was sent</summary>
            Texture2DName,
            /// <summary>The DataCode that represents the information on the <see cref="ASLObject.PostDownloadFunction"/> that will called after a Texture2D is downloaded</summary>
            Texture2DPostDownloadFunctionInfo,
            /// <summary>Flag that informs the application if it should wait to execute its Texture2D Post download function, or if it should execute it once it can</summary>
            Texture2DSyncStartFlag,
            /// <summary>Flag that informs the user if they should set this cloud anchor as the world origin or not</summary>
            SetWorldOrigin,
            /// <summary>Flag that informs the user if they should wait for all other users to resolve this cloud anchor or not</summary>
            WaitForAllUsersToResolve

        }

        /// <summary>
        /// A Getter for the GameController singleton - a variable representing this class
        /// </summary>
        /// <returns>The GameController singleton - a constructor for this class that is unique</returns>
        public static GameController Instance()
        {
            if (m_Instance != null)
            {
                return m_Instance;
            }
            else
            {
                Debug.Log("GameController is null.");
                return m_Instance ?? null;
            }
        }

        /// <summary>
        /// Set assign the singleton for this class
        /// </summary>
        private void Awake()
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject); //Since this holds our network information, we make sure it won't get deleted between scene loads
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            m_ObjectIDAssignedCount = 0;
            #region Setup Loading Pause Text Graphics

            m_PauseCanvas = new GameObject("FinalLoadingCanvas");
            m_PauseCanvas.transform.SetParent(transform);
            m_PauseCanvas.AddComponent<Canvas>();
            m_PauseCanvas.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            m_PauseCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            m_PauseText = new GameObject("FinalizingLoadingText");
            m_PauseText.transform.SetParent(m_PauseCanvas.transform);
            m_PauseText.AddComponent<UnityEngine.UI.Text>().text = "Finalizing loading... Game will begin automatically once all ASL Objects are synced.";
            UnityEngine.UI.Text pauseText = m_PauseText.GetComponent<UnityEngine.UI.Text>();
            pauseText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            pauseText.fontSize = 20;
            pauseText.color = Color.black;
            m_PauseText.transform.localPosition = new Vector3(0, 150, 0);
            m_PauseText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);
            m_PauseCanvas.SetActive(false);

            #endregion
            SceneManager.sceneLoaded += SyncronizeID;
        }

        /// <summary>
        /// Find all ASL Objects in the scene and have the relay server create a unique ID for them as well as add this object to our ASLObject dictionary
        /// This function is triggered when this script is first loaded. This function will keep the timeScale at 0 until all objects have been ID.
        /// All objects are ID in the SetObjectID(RTPacket _packet) function.
        /// </summary>
        private void SyncronizeID(Scene _scene, LoadSceneMode _mode)
        {
            ASLObject[] m_StartingASLObjects = FindObjectsOfType(typeof(ASLObject)) as ASLObject[];

            //If there are ASL objects to initialize - pause until done
            if (m_StartingASLObjects.Length > 0)
            {
                Time.timeScale = 0; //"Pause" the game until all object's have their id's set
                m_PauseCanvas.SetActive(true);
            }
            //Sort by name + sqrt(transform components + localPosition * Dot(rotation, scale))
            m_StartingASLObjects = m_StartingASLObjects.OrderBy(aslObj => aslObj.name + 
                ((aslObj.transform.localPosition + new Vector3(aslObj.transform.localRotation.x, aslObj.transform.localRotation.y, aslObj.transform.localRotation.z) +
                 aslObj.transform.localScale) + (aslObj.transform.localPosition + Vector3.Dot(new Vector3(aslObj.transform.localRotation.x, aslObj.transform.localRotation.y, aslObj.transform.localRotation.z),
                 aslObj.transform.localScale) * Vector3.one)).sqrMagnitude)
                .ToArray();

            int startingObjectCount = 0;
            foreach (ASLObject _ASLObject in m_StartingASLObjects)
            {
                if (_ASLObject.m_Id == string.Empty || _ASLObject.m_Id == null) //If object does not have an ID
                {
                    m_ObjectIDAssignedCount++;
                    using (RTData data = RTData.Get())
                    {
                        //Have the RT server create an ID and send it back to everyone
                        GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.ServerSetId, GameSparksRT.DeliveryIntent.RELIABLE, data);
                        //Add the starting objects to our dictionary without a key. Key will be added later to ensure all clients have the same key
                        ASLHelper.m_ASLObjects.Add(startingObjectCount.ToString(), _ASLObject);
                    }
                    startingObjectCount++;
                }
            }
        }

        /// <summary>
        /// Sets the parent of a newly created object
        /// </summary>
        /// <param name="_myObject"></param>
        /// <param name="_parentID"></param>
        private void SetParent(GameObject _myObject, string _parentID)
        {
            bool matchFound = false;
            //Search for the parent in ASLObjects            
            if (ASLHelper.m_ASLObjects.TryGetValue(_parentID ?? string.Empty, out ASLObject myParent))
            {
                _myObject.transform.SetParent(myParent.transform);
                matchFound = true;
            }

            //Search for parent in regular GameObjects - this is slow compared to our first attempt. But it should only be ran if the user
            //attempts to set the parent object by passing in the parent's name and not their id.
            if (!matchFound)
            {
                GameObject[] allGameObjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
                foreach (GameObject _gameObject in allGameObjects)
                {
                    if (_gameObject.name == _parentID)
                    {
                        _myObject.transform.SetParent(_gameObject.transform);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Upon game start, any ASL Objects in the scene do not have synchronized IDs. This function changes their ID to be synced with other clients
        /// </summary>
        /// <param name="_id">The new id for _object</param>
        /// <param name="_object">The ASL object that will be getting a new id</param>
        /// <returns></returns>
        private bool InitializeStartObject(string _id, ASLObject _object)
        {
            if (ASLHelper.m_ASLObjects.ContainsKey(_id))
            {
                _object._LocallySetID(_id);
                _object._LocallyRemoveReleaseCallback();
                return true;
            }
            else
            {
                Debug.LogError("Attempted to set the id of an object to a value that does not exist in our dictionary. If this was intended, then" +
                    " add this object to the m_ASLObjects dictionary first.");
                return false;
            }
        }

        /// <summary>
        /// Attempts to download the image at the specified downloadUrl. Once downloaded, it will transform the byte[] into a Texture2D and 
        /// call the user defined function with this texture and the gameobject used to send it as parameters.
        /// </summary>
        /// <param name="downloadUrl">The URL where the image can be downloaded from - given to us by GameSparks</param>
        /// <param name="_aslObject">The object used for sending uploading and/or downloading this image</param>
        /// <param name="_textureName">The name of the texture that was downloaded</param>
        /// <param name="_postDownloadFunctionInfo">The function that will be called after the image is downloaded to
        /// perform whatever operations the user needs to do with the gameobject that is associated with this texture and this texture</param>
        /// <param name="_textureUploadId">The upload id of the image. This is a GameSparks given id.</param>
        /// /// <param name="_syncStart">Flag indicating if once downloaded, if this texture2D should wait until all users have downloaded 
        /// before it executes the function it is attached to</param>
        /// <returns>Is a coroutine/IEnumerator, so waits asynchronously until the download is complete or errors</returns>
        private IEnumerator DownloadImage(string downloadUrl, GameObject _aslObject, string _textureName, string _postDownloadFunctionInfo, string _textureUploadId, string _syncStart)
        {
            using (var download = UnityWebRequest.Get(downloadUrl))
            {
                yield return download.SendWebRequest();

                if (download.isNetworkError || download.isHttpError)
                {
                    print("Download error: " + download.error);
                }
                else //Successful
                {
                    //Inform server we have downloaded this image - once all have downloaded it will delete from the server
                    using (RTData data = RTData.Get())
                    {
                        data.SetString((int)DataCode.Id, _aslObject.GetComponent<ASLObject>().m_Id);
                        data.SetString((int)DataCode.Texture2DUploadId, _textureUploadId);
                        data.SetString((int)DataCode.Texture2DSyncStartFlag, _syncStart);
                        GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.ReadyToDeleteUploadedImageOnServer, GameSparksRT.DeliveryIntent.RELIABLE, data);
                    }

                    //Get Texture2D
                    Texture2D dummyTexture = new Texture2D(1, 1); //Size doesn't matter
                    dummyTexture.LoadImage(download.downloadHandler.data);
                    dummyTexture.name = _textureName;
                    
                    //Call PostDownloadFunction
                    var functionName = Regex.Match(_postDownloadFunctionInfo, @"(\w+)$");
                    functionName.Value.Replace(" ", "");
                    var className = Regex.Replace(_postDownloadFunctionInfo, @"\s(\w+)$", "");
                    Type callerClass = Type.GetType(className);
                    _aslObject.GetComponent<ASLObject>()._LocallySetPostDownloadFunction(
                        (ASLObject.PostDownloadFunction)Delegate.CreateDelegate(typeof(ASLObject.PostDownloadFunction), callerClass, functionName.Value), dummyTexture, _textureUploadId);

                    //If syncStart is false, execute right away and remove from list
                    if (_syncStart != "1") //if _syncStart is false
                    {
                        _aslObject.GetComponent<ASLObject>().m_PostDownloadFunction.Invoke(_aslObject.gameObject, dummyTexture);
                        _aslObject.GetComponent<ASLObject>()._LocallyRemovePostDownloadFunction(_aslObject.GetComponent<ASLObject>().m_PostDownloadInfoList.Count - 1);
                    }

                }
            }
        }
    }
}
