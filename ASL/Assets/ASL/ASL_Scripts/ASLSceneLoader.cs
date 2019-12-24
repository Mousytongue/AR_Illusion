using GameSparks.RT;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace ASL
{
    /// <summary>Used to load scenes for all users so that they start a scene in sync with one another. This scene must always be built.</summary>
    public class ASLSceneLoader : MonoBehaviour
    {
        /// <summary> The text object that will display the loading progress to the user</summary>
        public Text m_LoadingText = null;

        /// <summary>The name of the scene to load</summary>
        public static string m_SceneToLoad = string.Empty;

        /// <summary>Flag indicating that all players are finished loading, triggering the scene activation </summary>
        public static bool m_AllPlayersLoaded = false;

        /// <summary> Local flag indicating this user has loaded, allowing them to send a packet to the RT server to communicate with others that they are ready </summary>
        private bool m_Loaded = false;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(m_LoadingText != null, "Loading text not initialized."); //Ensure LoadingText is assigned to a text object
            Debug.Assert(m_SceneToLoad != string.Empty || m_SceneToLoad != null, "No scene was passed in to load."); //Ensure a new scene is given

            //Start Asynchronously loading the next scene
            StartCoroutine(LoadScene(m_SceneToLoad));
        }

        IEnumerator LoadScene(string _sceneName)
        {
            yield return null;

            //Begin to load scene specified
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneName);

            //Don't let the scene activate until all users have it loaded (done via RT server)
            asyncOperation.allowSceneActivation = false;

            //While the scene is loading - output the progress
            while (!asyncOperation.isDone)
            {
                m_LoadingText.text = "\n\nLoading Progress: " + (asyncOperation.progress * 100) + "%";
                //Check if scene is finished loading:
                if (asyncOperation.progress >= 0.9f)
                {
                    if (!m_Loaded)
                    {
                        m_Loaded = true; //Prevent multiple packets from sending unnecessarily
                        
                        //Send packet to RT informing users this player is ready
                        using (RTData data = RTData.Get())
                        {
                            GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.SceneReady, GameSparksRT.DeliveryIntent.RELIABLE, data);
                        }
                    }
                    //Change to text to inform user that they are now waiting on others
                    m_LoadingText.text = "\n\nFinished Loading. Waiting for other players...";
                    if (m_AllPlayersLoaded)
                    {
                        
                        using (RTData data = RTData.Get())
                        {
                            //Send a packet letting all users know this scene is now activated and they can destroy the pending match
                            GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.DestroyPendingMatch, GameSparksRT.DeliveryIntent.RELIABLE, data);
                        }
                        m_AllPlayersLoaded = false;                       
                        asyncOperation.allowSceneActivation = true;
                        
                    }
                }
                yield return null;                
            }
            

        }
        
    }
}