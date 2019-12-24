using GameSparks.Api.Requests;
using GameSparks.RT;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ASL
{
    /// <summary>
    /// GameController_PacketHandler: Part of the GameController class that processes all of the packets received by the relay server, passed to these specific functions from the GameSparksManager class
    /// </summary>
    public partial class GameController : MonoBehaviour
    {
        /// <summary>
        /// Looks for and assigns any ASLObjects that do not have a unique ID yet. This ID is given through the relay server. 
        /// This function is triggered by a packet received from the relay server. This function will keep the time scale at 0 until all ASL objects have a proper ID
        /// </summary>
        /// <param name="_packet">The packet containing the unique ID of an ASL Object</param>
        public void SetObjectID(RTPacket _packet)
        {
            //Cycle through all items in the dictionary looking for the items with invalid keys 
            //For objects that start in the scene, their keys are originally set to be invalid so we set them properly through the RT Server
            //Ensuring all clients have the same key for each object
            foreach(KeyValuePair<string, ASLObject> _object in ASLHelper.m_ASLObjects)
            {
                //Since GUIDs aren't numbers, if we find a number, then we know it's a fake key value and it should be updated to match all other clients
                if (int.TryParse(_object.Key, out int result))
                {
                    ASLHelper.m_ASLObjects.Add(_packet.Data.GetString((int)DataCode.Id), _object.Value);
                    ASLHelper.m_ASLObjects.Remove(_object.Key);
                    InitializeStartObject(_packet.Data.GetString((int)DataCode.Id), _object.Value);
                    break;
                }
                
            }
            
            m_ObjectIDAssignedCount--;
            if (m_ObjectIDAssignedCount <= 0)
            {
                m_PauseCanvas.SetActive(false);
                Time.timeScale = 1; //Restart time
            }
        }

        /// <summary>
        /// Destroys an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to delete</param>
        public void DeleteObject(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                ASLHelper.m_ASLObjects.Remove(_packet.Data.GetString((int)DataCode.Id));
                Destroy(myObject.gameObject);       
            }
        }

        /// <summary>
        /// Updates the local transform of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new position</param>
        public void SetLocalPosition(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.localPosition = (Vector3)_packet.Data.GetVector3((int)DataCode.Position);
            }
        }

        /// <summary>
        /// Updates the local transform of an ASL Object based upon its ID by taking the value passed and adding it to the current localPosition value.
        /// This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new position</param>
        public void IncrementLocalPosition(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.localPosition += (Vector3)_packet.Data.GetVector3((int)DataCode.Position);
            }
        }

        /// <summary>
        /// Updates the local rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector4 of the object's new rotation</param>
        public void SetLocalRotation(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.localRotation = new Quaternion
                    (_packet.Data.GetVector4((int)DataCode.Rotation).Value.x,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.y,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.z,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.w);
            }
        }

        /// <summary>
        /// Updates the local rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector4 of the object's new rotation</param>
        public void IncrementLocalRotation(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.localRotation *= new Quaternion
                    (_packet.Data.GetVector4((int)DataCode.Rotation).Value.x,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.y,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.z,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.w);
            }
        }

        /// <summary>
        /// Updates the local scale of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new scale</param>
        public void SetLocalScale(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.localScale = ((Vector3)_packet.Data.GetVector3((int)DataCode.Scale));
            }
        }

        /// <summary>
        /// Updates the local scale of an ASL Object based upon its ID by taking the value passed in and adding it to the current localScale value. 
        /// This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new scale</param>
        public void IncrementLocalScale(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.localScale += ((Vector3)_packet.Data.GetVector3((int)DataCode.Scale));
            }
        }

        /// <summary>
        /// Updates the world position of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new position</param>
        public void SetWorldPosition(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.position = (Vector3)_packet.Data.GetVector3((int)DataCode.Position);
            }
        }

        /// <summary>
        /// Updates the world transform of an ASL Object based upon its ID by taking the value passed and adding it to the current localPosition value.
        /// This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new position</param>
        public void IncrementWorldPosition(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.position += (Vector3)_packet.Data.GetVector3((int)DataCode.Position);
            }
        }

        /// <summary>
        /// Updates the world rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector4 of the object's new rotation</param>
        public void SetWorldRotation(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.rotation = new Quaternion
                    (_packet.Data.GetVector4((int)DataCode.Rotation).Value.x,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.y,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.z,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.w);
            }
        }

        /// <summary>
        /// Updates the world rotation of an ASL Object based upon its ID. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector4 of the object's new rotation</param>
        public void IncrementWorldRotation(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject.transform.rotation *= new Quaternion
                    (_packet.Data.GetVector4((int)DataCode.Rotation).Value.x,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.y,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.z,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.w);
            }
        }

        /// <summary>
        /// Updates the world scale of an ASL Object based upon its ID by setting its parent to null and then 
        /// reassigning its parent after setting the scale you want it to have. This function is triggered by a 
        /// packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new scale</param>
        public void SetWorldScale(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                var parent = myObject.transform.parent;
                myObject.transform.parent = null;
                myObject.transform.localScale = ((Vector3)_packet.Data.GetVector3((int)DataCode.Scale));
                myObject.transform.parent = parent;
            }
        }

        /// <summary>
        /// Updates the world scale of an ASL Object based upon its ID by taking the value passed in and adding it to the current scale value
        /// by setting its parent to null and then reassigning its parent after setting the scale you want it to have. This function 
        /// is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the Vector3 of the object's new scale</param>
        public void IncrementWorldScale(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                var parent = myObject.transform.parent;
                myObject.transform.parent = null;
                myObject.transform.localScale += ((Vector3)_packet.Data.GetVector3((int)DataCode.Scale));
                myObject.transform.parent = parent;                
            }
        }

        /// <summary>
        /// Updates the Anchor Point of an ASL Object based upon its ID. The anchor point is used for AR applications.
        /// This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server containing the ID of what ASL Object to modified
        /// and the object's new Anchor Point information</param>
        public void SetAnchorID(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {               
                myObject._LocallySetAnchorID(_packet.Data.GetString((int)DataCode.AnchorID));
            }
        }

        /// <summary>
        /// Finds and claims a specified object and updates everybody's permission for that object. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet containing the id of the object to claim</param>
        public void SetObjectClaim(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                if (GameSparksManager.m_PlayerIds.TryGetValue(_packet.Data.GetString((int)DataCode.Player), out bool me))
                {
                    if (me) //If this is the player who sent the claim
                    {                        
                        myObject._LocallySetClaim(true);
                        myObject.m_ClaimCallback?.Invoke();
                        myObject.m_OutstandingClaimCallbackCount = 0;
                        myObject._LocallyRemoveClaimCallbacks();
                    }
                    else //This is not the player who sent the claim - remove any claims this player may have (shouldn't be any)
                    {
                        myObject._LocallySetClaim(false);
                        myObject._LocallyRemoveClaimCallbacks();
                    }
                }
            }
        }

        /// <summary>
        /// Releases an object so another user can claim it. This function will also call this object's release function if it exists.
        /// This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet containing the id of the object that another player wants to claim</param>
        public void ReleaseClaimedObject(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                if (GameSparksManager.m_PlayerIds.TryGetValue(_packet.Data.GetString((int)DataCode.OldPlayer), out bool currentOwner))
                {
                    if (currentOwner) //If this client is the current owner
                    {
                        //Send a packet to new owner informing them that the previous owner (this client) no longer owns the object
                        //So they can do whatever they want with it now.
                        using (RTData data = RTData.Get())
                        {
                            data.SetString((int)DataCode.Id, myObject.m_Id);
                            data.SetString((int)DataCode.Player, _packet.Data.GetString((int)DataCode.Player));
                            GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.ClaimFromPlayer, 
                                GameSparksRT.DeliveryIntent.RELIABLE, data, (int)_packet.Data.GetInt((int)DataCode.PlayerPeerId));
                        }
                        myObject.m_ReleaseFunction?.Invoke(myObject.gameObject); //If user wants to do something before object is released - let them do so
                        myObject._LocallyRemoveReleaseCallback();

                        myObject._LocallySetClaim(false);
                        myObject._LocallyRemoveClaimCallbacks();
                    }                    
                }
            }
        }

        /// <summary>
        /// Get the claim to an object that was previously owned by another player. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet sent by the previous owner of this object, 
        /// it contains the id of the object to be claimed by the receiver of this packet.</param>
        public void ObjectClaimReceived(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject._LocallySetClaim(true);
                //Call the function the user passed into original claim as they now have "complete control" over the object
                myObject.m_ClaimCallback?.Invoke();
                myObject.m_OutstandingClaimCallbackCount = 0;
                myObject._LocallyRemoveClaimCallbacks();
            }
        }

        /// <summary>
        /// Sets the object specified by the id contained in _packet to the color specified in _packet. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet containing the id of the object to change the color of, the color for the owner of the object,
        /// and the color for those who don't own the object</param>
        public void SetObjectColor(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                if (GameSparksManager.m_PlayerIds.TryGetValue(_packet.Data.GetString((int)DataCode.Player), out bool sender))
                {
                    if (sender) //If this was the one who sent the color
                    {
                        myObject.GetComponent<Renderer>().material.color = (Color)_packet.Data.GetVector4((int)DataCode.MyColor);
                    }
                    else //Everyone else
                    {
                        myObject.GetComponent<Renderer>().material.color = (Color)_packet.Data.GetVector4((int)DataCode.OpponentColor);
                    }
                }
            }
        }

        /// <summary>
        /// This function spawns a primitive object with ASL attached as a component. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet containing the id of the object to create, what type of primitive to create, where to create it, and depending on what the user inputted, may
        /// contain its parent's id, a callback function's class name and function name that is called upon creation, and a callback function's class name and function name
        /// that will get called whenever a claim for that object is rejected.</param>
        public void SpawnPrimitive(RTPacket _packet)
        {
            GameObject newASLObject = GameObject.CreatePrimitive((PrimitiveType)(int)_packet.Data.GetInt((int)DataCode.PrimitiveType));
            //Do we need to set the parent?
            if (_packet.Data.GetString((int)DataCode.ParentId) != string.Empty || _packet.Data.GetString((int)DataCode.ParentId) != null)
            {
                SetParent(newASLObject, _packet.Data.GetString((int)DataCode.ParentId));
            }

            newASLObject.transform.localPosition = (Vector3)_packet.Data.GetVector3((int)DataCode.Position);
            newASLObject.transform.localRotation = new Quaternion(_packet.Data.GetVector4((int)DataCode.Rotation).Value.x, _packet.Data.GetVector4((int)DataCode.Rotation).Value.y,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.z, _packet.Data.GetVector4((int)DataCode.Rotation).Value.w);

            //Set ID
            newASLObject.AddComponent<ASLObject>()._LocallySetID(_packet.Data.GetString((int)DataCode.Id));

            //Add any components if needed
            if (_packet.Data.GetString((int)DataCode.ComponentName) != string.Empty && _packet.Data.GetString((int)DataCode.ComponentName) != null)
            {
                Type component = Type.GetType(_packet.Data.GetString((int)DataCode.ComponentName));
                newASLObject.AddComponent(component);
            }

            //If we have the means to set up the recovery callback function - then do it
            if (_packet.Data.GetString((int)DataCode.ClaimRecoveryClassName) != string.Empty && _packet.Data.GetString((int)DataCode.ClaimRecoveryClassName) != null &&
                _packet.Data.GetString((int)DataCode.ClaimRecoveryFunctionName) != string.Empty && _packet.Data.GetString((int)DataCode.ClaimRecoveryFunctionName) != null)
            {
                Type callerClass = Type.GetType(_packet.Data.GetString((int)DataCode.ClaimRecoveryClassName));
                newASLObject.GetComponent<ASLObject>()._LocallySetClaimCancelledRecoveryCallback((ASLObject.ClaimCancelledRecoveryCallback)Delegate.CreateDelegate(typeof(ASLObject.ClaimCancelledRecoveryCallback),
                    callerClass, _packet.Data.GetString((int)DataCode.ClaimRecoveryFunctionName)));
            }

            //If we have the means to set up the SendFloat callback function - then do it
            if (_packet.Data.GetString((int)DataCode.SendFloatClassName) != string.Empty && _packet.Data.GetString((int)DataCode.SendFloatClassName) != null &&
                _packet.Data.GetString((int)DataCode.SendFloatFunctionName) != string.Empty && _packet.Data.GetString((int)DataCode.SendFloatFunctionName) != null)
            {
                Type callerClass = Type.GetType(_packet.Data.GetString((int)DataCode.SendFloatClassName));
                newASLObject.GetComponent<ASLObject>()._LocallySetFloatCallback(
                    (ASLObject.FloatCallback)Delegate.CreateDelegate(typeof(ASLObject.FloatCallback),
                    callerClass, _packet.Data.GetString((int)DataCode.SendFloatFunctionName)));
            }

            //Add object to our dictionary
            ASLHelper.m_ASLObjects.Add(_packet.Data.GetString((int)DataCode.Id), newASLObject.GetComponent<ASLObject>());

            //If this client is the creator of this object, then call the ASLGameObjectCreatedCallback if it exists for this object
            if (GameSparksManager.m_PlayerIds.TryGetValue(_packet.Data.GetString((int)DataCode.Player), out bool isCreator))
            {
                if (isCreator && _packet.Data.GetString((int)DataCode.InstantiatedGameObjectClassName) != string.Empty 
                    && _packet.Data.GetString((int)DataCode.InstantiatedGameObjectClassName) != null &&
                _packet.Data.GetString((int)DataCode.InstantiatedGameObjectFunctionName) != string.Empty && _packet.Data.GetString((int)DataCode.InstantiatedGameObjectFunctionName) != null)
                {
                    //Find Callback function
                    Type callerClass = Type.GetType(_packet.Data.GetString((int)DataCode.InstantiatedGameObjectClassName));
                    newASLObject.GetComponent<ASLObject>()._LocallySetGameObjectCreatedCallback(
                        (ASLObject.ASLGameObjectCreatedCallback)Delegate.CreateDelegate(typeof(ASLObject.ASLGameObjectCreatedCallback),callerClass, 
                        _packet.Data.GetString((int)DataCode.InstantiatedGameObjectFunctionName)));
                    //Call function
                    newASLObject.GetComponent<ASLObject>().m_ASLGameObjectCreatedCallback.Invoke(newASLObject);
                }
            }
        }

        /// <summary>
        /// This function spawns a prefab object with ASL attached as a component. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet containing the id of the object to create, what prefab to create it with, where to create it, and depending on what the user inputted, may
        /// contain its parent's id, a callback function's class name and function name that is called upon creation, and a callback function's class name and function name
        /// that will get called whenever a claim for that object is rejected.</param>
        public void SpawnPrefab(RTPacket _packet)
        {
            GameObject newASLObject = Instantiate(Resources.Load(@"MyPrefabs\" + _packet.Data.GetString((int)DataCode.PrefabName))) as GameObject;
            //Do we need to set the parent?
            if (_packet.Data.GetString((int)DataCode.ParentId) != string.Empty || _packet.Data.GetString((int)DataCode.ParentId) != null)
            {
                SetParent(newASLObject, _packet.Data.GetString((int)DataCode.ParentId));
            }

            newASLObject.transform.localPosition = (Vector3)_packet.Data.GetVector3((int)DataCode.Position);
            newASLObject.transform.localRotation = new Quaternion(_packet.Data.GetVector4((int)DataCode.Rotation).Value.x, _packet.Data.GetVector4((int)DataCode.Rotation).Value.y,
                    _packet.Data.GetVector4((int)DataCode.Rotation).Value.z, _packet.Data.GetVector4((int)DataCode.Rotation).Value.w);

            //Set ID
            newASLObject.AddComponent<ASLObject>()._LocallySetID(_packet.Data.GetString((int)DataCode.Id));

            //Add any components if needed
            if (_packet.Data.GetString((int)DataCode.ComponentName) != string.Empty && _packet.Data.GetString((int)DataCode.ComponentName) != null)
            {
                Type component = Type.GetType(_packet.Data.GetString((int)DataCode.ComponentName));
                newASLObject.AddComponent(component);
            }

            //If we have the means to set up the recovery callback function - then do it
            if (_packet.Data.GetString((int)DataCode.ClaimRecoveryClassName) != string.Empty && _packet.Data.GetString((int)DataCode.ClaimRecoveryClassName) != null &&
                _packet.Data.GetString((int)DataCode.ClaimRecoveryFunctionName) != string.Empty && _packet.Data.GetString((int)DataCode.ClaimRecoveryFunctionName) != null)
            {
                Type callerClass = Type.GetType(_packet.Data.GetString((int)DataCode.ClaimRecoveryClassName));
                newASLObject.GetComponent<ASLObject>()._LocallySetClaimCancelledRecoveryCallback(
                    (ASLObject.ClaimCancelledRecoveryCallback)Delegate.CreateDelegate(typeof(ASLObject.ClaimCancelledRecoveryCallback),
                    callerClass, _packet.Data.GetString((int)DataCode.ClaimRecoveryFunctionName)));
            }

            //If we have the means to set up the SendFloat callback function - then do it
            if (_packet.Data.GetString((int)DataCode.SendFloatClassName) != string.Empty && _packet.Data.GetString((int)DataCode.SendFloatClassName) != null &&
                _packet.Data.GetString((int)DataCode.SendFloatFunctionName) != string.Empty && _packet.Data.GetString((int)DataCode.SendFloatFunctionName) != null)
            {
                Type callerClass = Type.GetType(_packet.Data.GetString((int)DataCode.SendFloatClassName));
                newASLObject.GetComponent<ASLObject>()._LocallySetFloatCallback(
                    (ASLObject.FloatCallback)Delegate.CreateDelegate(typeof(ASLObject.FloatCallback),
                    callerClass, _packet.Data.GetString((int)DataCode.SendFloatFunctionName)));
            }

            //Add object to our dictionary
            ASLHelper.m_ASLObjects.Add(_packet.Data.GetString((int)DataCode.Id), newASLObject.GetComponent<ASLObject>());

            //If this client is the creator of this object, then call the ASLGameObjectCreatedCallback if it exists for this object
            if (GameSparksManager.m_PlayerIds.TryGetValue(_packet.Data.GetString((int)DataCode.Player), out bool isCreator))
            {
                if (isCreator && _packet.Data.GetString((int)DataCode.InstantiatedGameObjectClassName) != string.Empty
                    && _packet.Data.GetString((int)DataCode.InstantiatedGameObjectClassName) != null &&
                _packet.Data.GetString((int)DataCode.InstantiatedGameObjectFunctionName) != string.Empty && _packet.Data.GetString((int)DataCode.InstantiatedGameObjectFunctionName) != null)
                {
                    //Find Callback function
                    Type callerClass = Type.GetType(_packet.Data.GetString((int)DataCode.InstantiatedGameObjectClassName));
                    newASLObject.GetComponent<ASLObject>()._LocallySetGameObjectCreatedCallback(
                        (ASLObject.ASLGameObjectCreatedCallback)Delegate.CreateDelegate(typeof(ASLObject.ASLGameObjectCreatedCallback), callerClass,
                        _packet.Data.GetString((int)DataCode.InstantiatedGameObjectFunctionName)));
                    //Call function
                    newASLObject.GetComponent<ASLObject>().m_ASLGameObjectCreatedCallback.Invoke(newASLObject);
                }
            }
        }

        /// <summary>
        /// Reject a player's claim request on an ASL Object. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet containing the id of the object that a player wanted to claim</param>
        public void RejectClaim(RTPacket _packet)
        {
            Debug.LogWarning("Claim Rejected id: " + _packet.Data.GetString((int)DataCode.Id));
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                Debug.LogWarning("Claim Rejected");
                //Remove all callbacks created as our claim was rejected
                if (myObject.m_ClaimCallback?.GetInvocationList().Length == null)
                {
                    myObject.m_ClaimCancelledRecoveryCallback?.Invoke(myObject.m_Id, 0);
                }
                else
                {
                    myObject.m_ClaimCancelledRecoveryCallback?.Invoke(myObject.m_Id, myObject.m_ClaimCallback.GetInvocationList().Length);
                }
                myObject._LocallyRemoveClaimCallbacks();
            }
        }

        /// <summary>
        /// Loads a new scene for all players. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet containing the name of the new scene to be loaded</param>
        public void LoadScene(RTPacket _packet)
        {
            if (_packet.OpCode == (int)GameSparksManager.OpCode.LoadStartScene) //If loading starting scene
            {
                //Pick the next scene to load depending on how this game was launched
                if (QuickConnect.m_StaticStartingScene != string.Empty) { ASLSceneLoader.m_SceneToLoad = QuickConnect.m_StaticStartingScene; }
                else { ASLSceneLoader.m_SceneToLoad = LobbyManager.m_StaticStartingScene; }

                Debug.Log("Scene: " + ASLSceneLoader.m_SceneToLoad);

                SceneManager.LoadScene("ASL_SceneLoader");
                LobbyManager.m_GameStarted = true;
            }
            else if (_packet.OpCode == (int)GameSparksManager.OpCode.LoadScene) //If loading any scene after starting scene
            {
                ASLSceneLoader.m_SceneToLoad = _packet.Data.GetString((int)DataCode.SceneName);
                SceneManager.LoadScene("ASL_SceneLoader");
            }
            else //Scene is ready to launch - inform user they can now activate the scene
            {
                ASLSceneLoader.m_AllPlayersLoaded = true;
            }           
        }

        /// <summary>
        /// Pass in the float value(s) from the relay server to a function of the user's choice (delegate function). The function that uses these float(s) is determined 
        /// by the user by setting the ASL Object of choosing's m_FloatCallback function to their own personal function. This function is triggered by a packet received from the relay server.
        /// Remember, though the user can pass in a float array, the max size of this array is 4 because we send it via a Vector4 due to GameSpark constraints
        /// </summary>
        /// <param name="_packet">The packet containing the id of the ASL Object and the float value to be passed into the user defined m_FloatCallback function</param>
        public void SetFloat(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                //Convert passed in Vector4 to a float array for our function
                Vector4 myFloatsAsVector4 = (Vector4)_packet.Data.GetVector4((int)DataCode.MyFloats);
                float[] myFloats = new float[4];
                myFloats[0] = myFloatsAsVector4.x;
                myFloats[1] = myFloatsAsVector4.y;
                myFloats[2] = myFloatsAsVector4.z;
                myFloats[3] = myFloatsAsVector4.w;

                //Sliders are  updated through SendFloat, so check here if this is a slider.
                //By doing this on the ASL side, users don't have to worry about forgetting to update the slider themselves
                if (myObject.GetComponent<ASLSliderWithEcho>())
                {
                    myObject.GetComponent<ASLSliderWithEcho>().UpdateSlider(myFloats[0]);
                }


                myObject.m_FloatCallback?.Invoke(_packet.Data.GetString((int)DataCode.Id), myFloats);
            }
        }

        /// <summary>
        /// Informs the user that they are unable to join a match because it is already in progress and
        /// that they should join a different room. This function is triggered by a packet received from the relay server.
        /// </summary>
        /// <param name="_packet">The packet from the relay server informing the user they attempting to
        /// ready up in a match that is already in progress</param>
        public void GameInProgress(RTPacket _packet)
        {
            Debug.LogWarning("Match already in progress. Start a new match with a new room name.");
            LobbyManager.m_SwitchedToManualMatchMaking = true;
        }

        /// <summary>
        /// Destroys the pending match this player is in so that more players cannot join this match. This prevents late comers or reconnectors from joining this match
        /// </summary>
        /// <param name="_packet">Empty packet from the relay server indicating that it is safe to destroy the pending match this player is a part of.</param>
        public void DestroyPendingMatch(RTPacket _packet)
        {           
            new LogEventRequest().SetEventKey("Disconnect").
                SetEventAttribute("MatchShortCode", GameSparksManager.Instance().GetMatchShortCode()).
                SetEventAttribute("MatchGroup", GameSparksManager.Instance().GetMyMatchGroup())
                .Send((_disconnectResponse) => {});
        }

        /// <summary>
        /// Calls the coroutine function that will get the uploaded Texture2D from the GameSparks servers
        /// </summary>
        /// <param name="_packet">The data packet that contains the upload id of the image, 
        /// its name, information about the function to call after its been downloaded, and the id of the game object it was sent with</param>
        public void GetUploadedTexture2D(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                new GetUploadedRequest().SetUploadId(_packet.Data.GetString((int)DataCode.Texture2DUploadId)).Send((response) =>
                {
                    //pass the URL to our coroutine that will accept the data
                    StartCoroutine(DownloadImage(response.Url, myObject.gameObject, _packet.Data.GetString((int)DataCode.Texture2DName), 
                        _packet.Data.GetString((int)DataCode.Texture2DPostDownloadFunctionInfo), _packet.Data.GetString((int)DataCode.Texture2DUploadId), 
                        _packet.Data.GetString((int)DataCode.Texture2DSyncStartFlag)));
                });
            }
        }

        /// <summary>
        /// Deletes the uploaded image on the GameSparks server to prevent us from going over our data limit.
        /// </summary>
        /// <param name="_packet">Contains the upload id of the image to delete from the server</param>
        public void DeleteUploadedImageOnServer(RTPacket _packet)
        {
            //Delete uploaded image
            new LogEventRequest().SetEventKey("RemoveUploadedImage").
                SetEventAttribute("UploadedId", _packet.Data.GetString((int)DataCode.Texture2DUploadId))
                .Send((_removedUploadedImageResponse) => { });
            
            //If necessary, execute post download function now that all have downloaded
            ExecuteSynchronizedPostDownloadFunction(_packet);
        }

        /// <summary>
        /// Finds the correct function associated with this Texture2D and then executes it now that all users have downloaded this texture/image.
        /// </summary>
        /// <param name="_packet">Contains the upload id of the image to execute its associated function. This method is used if syncStart 
        /// was set to true, forcing users to wait until all users have downloaded the image/Texture2D before executing the function attached
        /// to it.</param>
        public void ExecuteSynchronizedPostDownloadFunction(RTPacket _packet)
        {
            //Execute function associated with uploaded image if syncStart was enabled
            if (_packet.Data.GetString((int)DataCode.Texture2DSyncStartFlag) == "1") //if _syncStart was true
            {
                if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
                {
                    int indexCount = 0;
                    foreach (var listItem in myObject.GetComponent<ASLObject>().m_PostDownloadInfoList)
                    {
                        //If our upload ids match - then this is the function to execute
                        if (listItem.s_uploadId == _packet.Data.GetString((int)DataCode.Texture2DUploadId))
                        {
                            listItem.s_postDownloadFunction?.Invoke(myObject.gameObject, listItem.s_myTexture2D);
                            myObject.GetComponent<ASLObject>()._LocallyRemovePostDownloadFunction(indexCount);
                            break; //Found our texture function to execute - leave foreach
                        }
                        indexCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Packet informing user to start trying to resolve a cloud anchor
        /// </summary>
        /// <param name="_packet">Contains cloud anchor id to resolve, ASLObjects to attach to the cloud anchor, 
        /// whether or not to set the world origin, and if to wait for others or not</param>
        public void ResolveAnchorId(RTPacket _packet)
        {
             StartCoroutine(ResolveCloudAnchor(_packet.Data.GetString((int)DataCode.Id), _packet.Data.GetString((int)DataCode.AnchorID),
                 _packet.Data.GetString((int)DataCode.SetWorldOrigin), _packet.Data.GetString((int)DataCode.WaitForAllUsersToResolve)));
        }

        /// <summary>
        /// CoRoutine that resolves the cloud anchor
        /// </summary>
        /// <param name="_objectId">The ASLObject tied to this cloud anchor</param>
        /// <param name="anchorID">The cloud anchor to resolve</param>
        /// <param name="_setWorldOrigin">Whether or not to set the world origin</param>
        /// <param name="_waitForAllUsersToResolve">Whether or not to wait for all users before creating the cloud anchor</param>
        /// <returns>yield wait for - until tracking</returns>
        private IEnumerator ResolveCloudAnchor(string _objectId, string anchorID, string _setWorldOrigin, string _waitForAllUsersToResolve)
        {
            //If not the device is currently not tracking, wait to resolve the anchor
            while (Session.Status != SessionStatus.Tracking)
            {
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("in:");
            XPSession.ResolveCloudAnchor(anchorID).ThenAction((Action<CloudAnchorResult>)(result =>
            {
                //If failed to resolve
                if (result.Response != CloudServiceResponse.Success)
                {
                    Debug.LogError("Could not resolve Cloud Anchor: " + anchorID + " " + result.Response);
                }

                Debug.Log("Successfully Resolved cloud anchor: " + anchorID);
                //Now have at least one cloud anchor in the scene
                ASLObject anchorObjectPrefab;
                if (ASLHelper.m_ASLObjects.TryGetValue(_objectId ?? string.Empty, out ASLObject myObject)) //if user has ASL object -> ASL Object was created before linking to cloud anchor
                {
                    anchorObjectPrefab = myObject;
                    anchorObjectPrefab._LocallySetAnchorID(result.Anchor.CloudId);
                    anchorObjectPrefab.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); //Set scale to be 10 cm
                }
                else //ASL Object was created to link to cloud anchor - do the same here
                {
                    //Uncomment the line below to aid in visual debugging (helps display the cloud anchor)
                    //anchorObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<ASLObject>(); //if null, then create empty game object   
                    anchorObjectPrefab = new GameObject().AddComponent<ASLObject>();
                    anchorObjectPrefab._LocallySetAnchorID(result.Anchor.CloudId); //Add ASLObject component to this anchor and set its anchor id variable
                    anchorObjectPrefab._LocallySetID(result.Anchor.CloudId); //Locally set the id of this object to be that of the anchor id (which is unique)

                    //Add this anchor object to our ASL dictionary using the anchor id as its key. All users will do this once they resolve this cloud anchor to ensure they still in sync.
                    ASLHelper.m_ASLObjects.Add(result.Anchor.CloudId, anchorObjectPrefab.GetComponent<ASLObject>());
                    //anchorObjectPrefab.GetComponent<Material>().color = Color.magenta;
                    anchorObjectPrefab.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f); //Set scale to be 10 cm
                }

                if (bool.Parse(_waitForAllUsersToResolve))
                {
                    Debug.Log("Sent resolved");
                    //Send packet to relay server letting it know this user is ready
                    using (RTData data = RTData.Get())
                    {
                        data.SetString((int)DataCode.Id, anchorObjectPrefab.m_Id);
                        GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.ResolvedCloudAnchor, GameSparksRT.DeliveryIntent.RELIABLE, data);
                    }
                    //Wait for others
                    anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(result, bool.Parse(_setWorldOrigin), null);
                }
                else
                {
                    anchorObjectPrefab._LocallySetCloudAnchorResolved(true);
                    anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(result, bool.Parse(_setWorldOrigin), null);
                }

            }));
        }

        public void AllClientsFinishedResolvingCloudAnchor(RTPacket _packet)
        {
            if (ASLHelper.m_ASLObjects.TryGetValue(_packet.Data.GetString((int)DataCode.Id) ?? string.Empty, out ASLObject myObject))
            {
                myObject._LocallySetCloudAnchorResolved(true);
            }
        }

    }
}