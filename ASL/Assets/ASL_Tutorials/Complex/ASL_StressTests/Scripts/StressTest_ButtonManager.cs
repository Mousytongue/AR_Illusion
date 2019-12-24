using UnityEngine;
using UnityEngine.UI;

namespace StressTesting
{
    /// <summary>Button Manager for Stress Tests</summary>
    public class StressTest_ButtonManager : MonoBehaviour
    {
        /// <summary>A handle to the Delete Random button</summary>
        public Button mDeleteButton = null;
        /// <summary>A handle to the Stop button</summary>
        public Button mStopAll = null;

        void Start()
        {
            Debug.Assert(mDeleteButton != null);
            Debug.Assert(mStopAll != null);
        }

        /// <summary>Delete a random ASL object in the scene</summary>
        public void DeleteObject()
        {
            Debug.Log("Randomly selecting and deleting an object...");
            //Since we don't keep track of the amount of ASL objects in the scene, we need to find all of them so we know which ones we can move
            int objectNumber = -1;
            var aslObjects = FindObjectsOfType<ASL.ASLObject>(); //Warning: Getting objects this way is slow
            if (aslObjects.Length > 0) //If there is an ASL object to move
            {
                objectNumber = Random.Range(0, aslObjects.Length); //Randomly grab an ASL object
                aslObjects[objectNumber].SendAndSetClaim(() =>
                {
                    aslObjects[objectNumber].DeleteObject(); //Once claimed, delete this object
                });
            }
        }

        /// <summary>
        /// Find a random object and use it to stop all other objects in the scene so you can examine their positions to see if they're still in sync
        /// </summary>
        public void StopAllClients()
        {
            var randomObject = FindObjectOfType<ASL.ASLObject>();
            randomObject.GetComponent<ASL.ASLObject>()?.SendAndSetClaim(() =>
            {
                float[] myValue = new float[4];
                myValue[0] = 0;
                myValue[1] = 1;
                myValue[2] = 2;
                myValue[3] = 3;
                randomObject.GetComponent<ASL.ASLObject>()?.SendFloat4(myValue);
            });

        }

    }
}