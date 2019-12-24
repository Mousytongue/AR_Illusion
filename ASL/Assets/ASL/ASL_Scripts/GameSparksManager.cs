using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using GameSparks.RT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ASL
{
    /// <summary>
    /// This class handles all of the incoming packets - directing them where they need to go. It persists between scene loads and is accessed through a singleton variable.
    /// </summary>
    public class GameSparksManager : MonoBehaviour
    {
        #region Private Variables
        /// <summary>The GameSparksManager singleton. This is an unique variable - only 1 exists at a time. </summary>
        private static GameSparksManager m_Instance = null;

        /// <summary> This is the real time unity variable </summary>
        private GameSparksRTUnity m_GameSparksRTUnity;

        /// <summary> Holds the information about our RTSession </summary>
        private RTSessionInfo m_SessionInfo;

        /// <summary> The joined match's shortCode. </summary>
        private string m_MatchShortCode;

        /// <summary> The joined match's group</summary>
        private string m_MyMatchGroup;
        /// <summary>The peer id of this player in the current match</summary>
        private int m_PlayerPeerId;

        #endregion

        /// <summary>The OpCodes that distinguish packets from one another </summary>
        public enum OpCode
        {
            /// <summary> Opcodes cannot have a value of 0, so this ensures our opcodes start at value 1 </summary>
            BadValue,
            /// <summary> Packet code representing a chat message</summary>
            ChatMessage,
            /// <summary> Packet code representing data that will set the local position of an ASL object</summary>
            SetLocalPosition,
            /// <summary>Packet code representing data that will set the local rotation of an ASL object</summary>
            SetLocalRotation,
            /// <summary>Packet code representing data that will set the local scale of an ASL object</summary>
            SetLocalScale,
            /// <summary>Packet code representing a claim</summary>
            Claim,
            /// <summary>Packet code for spawning a primitive object</summary>
            SpawnPrimitive,
            /// <summary>Packet code for setting an object's color </summary>
            SetObjectColor,
            /// <summary>Packet code for deleting an object</summary>
            DeleteObject,
            /// <summary>Packet code for setting an object's id</summary>
            SetObjectID,
            /// <summary>Packet code for loading the starting scene</summary>
            LoadStartScene,
            /// <summary>Packet code for creating an id for an object on the server</summary>
            ServerSetId,
            /// <summary>Packet code informing player who has a claim on an object to release to another player</summary>
            ReleaseClaimToPlayer,
            /// <summary>Packet code informing a player who claimed an object from another player that they have it now</summary>
            ClaimFromPlayer,
            /// <summary>Packet code informing a player their claim was rejected</summary>
            RejectClaim,
            /// <summary>Packet code to release a claim back to the server</summary>
            ReleaseClaimToServer,
            /// <summary>Packet code informing a player a float was sent</summary>
            SetFloat,
            /// <summary>Packet code for spawning a prefab</summary>
            SpawnPrefab,
            /// <summary>Packet code for updating the AR anchor point</summary>
            AnchorIDUpdate,
            /// <summary>Packet code for changing the scene</summary>
            LoadScene,
            /// <summary>Packet code for informing the user they are attempting to join a game already in progress</summary>
            GameInProgress,
            /// <summary> Packet code for informing the RT session that the player who sent this is ready to start the next scene (they have loaded it)</summary>
            SceneReady,
            /// <summary> Packet code for informing the RT session that this player is ready to destroy their pending match, when received, they will destroy.</summary>
            DestroyPendingMatch,
            /// <summary>Packet code representing data that will add to the local position of an ASL object</summary>
            IncrementLocalPosition,
            /// <summary>Packet code representing data that will add to the local rotation of an ASL object</summary>
            IncrementLocalRotation,
            /// <summary>Packet code representing data that will add to the local scale of an ASL object</summary>
            IncrementLocalScale,
            /// <summary>Packet code representing data that will be used to inform users that they can download a Texture2D from the GS server</summary>
            SendTexture2D,
            /// <summary>Packet code for informing the RT server that this player is ready to delete an uploaded image</summary>
            ReadyToDeleteUploadedImageOnServer,
            /// <summary>Packet code from the RT server informing this player that they can delete an uploaded image</summary>
            DeleteUploadedImageOnServer,
            /// <summary> Packet code representing data that will set the world position of an ASL object</summary>
            SetWorldPosition,
            /// <summary>Packet code representing data that will set the world rotation of an ASL object</summary>
            SetWorldRotation,
            /// <summary>Packet code representing data that will set the world scale of an ASL object</summary>
            SetWorldScale,
            /// <summary>Packet code representing data that will add to the world position of an ASL object</summary>
            IncrementWorldPosition,
            /// <summary>Packet code representing data that will add to the world rotation of an ASL object</summary>
            IncrementWorldRotation,
            /// <summary>Packet code representing data that will add to the world scale of an ASL object</summary>
            IncrementWorldScale,
            /// <summary> Packet code representing data that will be used to resolve a cloud anchor </summary>
            ResolveAnchorId,
            /// <summary>
            /// Packet code representing data that will be used to inform the relay server that this user has completed
            /// resolving a cloud anchor and will be received as a flag indicating that all users have resolved this cloud anchor 
            /// </summary>
            ResolvedCloudAnchor

        };

        /// <summary> The joined match's id</summary>
        public string m_MyMatchId;

        /// <summary>
        /// Dictionary containing all of the player ids for this game
        /// </summary>
        static public Dictionary<string, bool> m_PlayerIds = new Dictionary<string, bool>();

        /// <summary>
        /// A Getter for the m_GameSparksRTUnity variable. 
        /// </summary>
        /// <returns>GameSparksRTUnity variable</returns>
        public GameSparksRTUnity GetRTSession() { return m_GameSparksRTUnity; }

        /// <summary>
        /// A getter for the m_SessionInfo variable
        /// </summary>
        /// <returns></returns>
        public RTSessionInfo GetSessionInfo() { return m_SessionInfo; }

        /// <summary>
        /// A setter for the match short code - used for connecting to other players
        /// </summary>
        /// <param name="_shortCode">The short code of a match</param>
        public void SetMatchShortCode(string _shortCode) { m_MatchShortCode = _shortCode; }

        /// <summary>
        /// Gets the match's short code - used for connecting to other players
        /// </summary>
        /// <returns>The match short code</returns>
        public string GetMatchShortCode() { return m_MatchShortCode; }

        /// <summary>Sets the match group for this user</summary>
        /// <param name="_matchGroup"></param>
        public void SetMatchGroup(string _matchGroup) { m_MyMatchGroup = _matchGroup; } 

        /// <summary>Returns the user's match group</summary>
        /// <returns>The name of the match group the user that called this belongs to</returns>
        public string GetMyMatchGroup() { return m_MyMatchGroup; }

        /// <summary>
        /// A Getter for the GameSparksManager singleton - a variable representing this class
        /// </summary>
        /// <returns>The GameSparksManager single - a constructor for this class that is unique</returns>
        public static GameSparksManager Instance()
        {
            if (m_Instance != null)
            {
                return m_Instance;
            }
            else
            {
                Debug.LogError("GSM| GameSparkManager not initialized.");
            }
            return null;
        }

        /// <summary>
        /// Unity function that is ran right when this class is first initialized (before Start())
        /// </summary>
        private void Awake()
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject); //Since this holds our network information, we make sure it won't get deleted between scene loads
        }

        /// <summary>Unity function that is ran after Awake but before anything else</summary>
        private void Start()
        {
            m_PlayerPeerId = int.MaxValue; //Set to negative number to symbolize that we don't know the peer id of this player and won't until GetPlayerPeerId is called
        }

        /// <summary>
        /// Initializes and starts a new real time session for users
        /// </summary>
        /// <param name="_info">The session information for this game</param>
        public void StartNewRTSession(RTSessionInfo _info)
        {
            Debug.Log("GSM| Creating new RT Session instance...");
            m_SessionInfo = _info;
            m_GameSparksRTUnity = this.gameObject.AddComponent<GameSparksRTUnity>(); //Adds the RT script to the game
            //In order to create a new RT game we need a 'FindMatchResponse'
            //This would usually come from the server directly after a successful MatchmakingRequest
            //However, in our case, we want the game to be create only when all the players ready up
            //Therefore, the details from the response is passed in from the gameInfo and a mock-up of a FindMatchReponse is passed in
            GSRequestData mockedReponse = new GSRequestData()
                .AddNumber("port", (double)_info.GetPortID())
                .AddString("host", _info.GetHostURL())
                .AddString("accessToken", _info.GetAccessToken()); //Construct a dataset from the game-details

            FindMatchResponse response = new FindMatchResponse(mockedReponse); //Create a match-response from that data and pass it into the game-configuration
                                                                               //So in the game-configuration method we pass in the response which gives the Instance its connection settings
                                                                               //In this example, a lambda expression is used to pass in actions for 
                                                                               //OnPlayerConnect, OnPlayerDisconnect, OnReady, and OnPacket
                                                                               //The methods do exactly what they are named. For example, OnPacket gets called when a packet is received            _
            m_GameSparksRTUnity.Configure(response,
                (peerId) => { OnPlayerConnectedToGame(peerId); },
                (peerId) => { OnPlayerDisconnected(peerId); },
                (ready) => { OnRTReady(ready); },
                (packet) => { OnPacketReceived(packet); });

            m_GameSparksRTUnity.Connect(); //When the configuration is set, connect the game
        }

        /// <summary>Ends the RT session of the user that called this function</summary>
        /// <param name="leaveMatch">Flag indicating if the user should leave the match as well (quit match making)</param>
        public void EndRTSession(bool leaveMatch = false)
        {
            //If readied up - unready up
            LobbyManager.m_ReadyButton.GetComponent<Button>().interactable = true;
            //Check if in an RT session or not
            if (gameObject.GetComponent<GameSparksRTUnity>() != null) 
            {
                m_GameSparksRTUnity.Disconnect();
                Destroy(gameObject.GetComponent<GameSparksRTUnity>()); //Remove RT script
            }
            if (leaveMatch)
            {
                string myId = string.Empty;
                foreach (var me in m_PlayerIds)
                {
                    if (me.Value)
                    {
                        myId = me.Key;
                    }
                }
                new LogEventRequest().SetEventKey("leaveMatch").
                    SetEventAttribute("MyMatchID", m_MyMatchId).Send((_leaveResponse) =>
                    {
                        if (_leaveResponse.HasErrors)
                        {
                            Debug.Log(_leaveResponse.Errors.JSON);
                        }
                        else
                        {
                            m_PlayerIds.Clear();
                            m_PlayerIds.Add(myId, true);
                            if (myId == string.Empty)
                            {
                                Debug.LogError("My player id is empty. Something bad happened");
                            }
                        }
                    });
            }
        }

        /// <summary>
        /// Executes when a player connects to a game
        /// </summary>
        /// <param name="_peerId">The peer id of the player that connected</param>
        private void OnPlayerConnectedToGame(int _peerId)
        {
            Debug.Log("GSM| Player Connected, " + _peerId);
        }

        /// <summary>
        /// Executes when a player disconnects from a game
        /// </summary>
        /// <param name="_peerId">The peer id of the player that disconnected</param>
        private void OnPlayerDisconnected(int _peerId)
        {
            Debug.Log("GSM| Player Disconnected, " + _peerId);
        }

        /// <summary>
        /// Executes when a player is ready to start playing
        /// </summary>
        /// <param name="_isReady">Flag indicting the player is ready or not to start</param>
        private void OnRTReady(bool _isReady)
        {
            //Scene is loaded from OnPacketReceived, this just says we're ready
            if (_isReady)
            {
                //Disable ready up button
                LobbyManager.m_ReadyButton.GetComponent<Button>().interactable = false;
            }
        }

        /// <summary>
        /// Executes whenever a packet is received from the relay server and directs that packet to the correct location using its OpCode
        /// </summary>
        /// <param name="_packet">The packet that came from the relay server</param>
        private void OnPacketReceived(RTPacket _packet)
        {
            switch (_packet.OpCode)
            {
                //op-code 1 refers to any chat-messages being received by a player
                //From here we'll send them to the chat manager
                //case (int)OpCode.ChatMessage:
                    //if (m_ChatManager == null) //If we haven't been initialized yet
                    //{
                    //    m_ChatManager = GameObject.Find("Chat Manager").GetComponent<ChatManager>();
                    //}
                    //m_ChatManager.OnMessageReceived(_packet); //Send the whole packet to the chat manager to handle
              //      break;
                case (int)OpCode.SetLocalPosition:
                    GameController.Instance()?.SetLocalPosition(_packet);
                    break;
                case (int)OpCode.SetLocalRotation:
                    GameController.Instance()?.SetLocalRotation(_packet);
                    break;
                case (int)OpCode.SetLocalScale:
                    GameController.Instance()?.SetLocalScale(_packet);
                    break;
                case (int)OpCode.IncrementLocalPosition:
                    GameController.Instance()?.IncrementLocalPosition(_packet);
                    break;
                case (int)OpCode.IncrementLocalRotation:
                    GameController.Instance()?.IncrementLocalRotation(_packet);
                    break;
                case (int)OpCode.IncrementLocalScale:
                    GameController.Instance()?.IncrementLocalScale(_packet);
                    break;
                case (int)OpCode.SetWorldPosition:
                    GameController.Instance()?.SetWorldPosition(_packet);
                    break;
                case (int)OpCode.SetWorldRotation:
                    GameController.Instance()?.SetWorldRotation(_packet);
                    break;
                case (int)OpCode.SetWorldScale:
                    GameController.Instance()?.SetWorldScale(_packet);
                    break;
                case (int)OpCode.IncrementWorldPosition:
                    GameController.Instance()?.IncrementWorldPosition(_packet);
                    break;
                case (int)OpCode.IncrementWorldRotation:
                    GameController.Instance()?.IncrementWorldRotation(_packet);
                    break;
                case (int)OpCode.IncrementWorldScale:
                    GameController.Instance()?.IncrementWorldScale(_packet);
                    break;
                case (int)OpCode.Claim:
                    GameController.Instance()?.SetObjectClaim(_packet);
                    break;
                case (int)OpCode.ReleaseClaimToPlayer:
                    GameController.Instance()?.ReleaseClaimedObject(_packet);
                    break;
                case (int)OpCode.ClaimFromPlayer:
                    GameController.Instance()?.ObjectClaimReceived(_packet);
                    break;
                case (int)OpCode.SpawnPrimitive:
                    GameController.Instance()?.SpawnPrimitive(_packet);
                    break;
                case (int)OpCode.SpawnPrefab:
                    GameController.Instance()?.SpawnPrefab(_packet);
                    break;
                case (int)OpCode.SetObjectColor:
                    GameController.Instance()?.SetObjectColor(_packet);
                    break;
                case (int)OpCode.DeleteObject:
                    GameController.Instance()?.DeleteObject(_packet);
                    break;
                case (int)OpCode.SetObjectID:
                    GameController.Instance()?.SetObjectID(_packet);
                    break;
                case (int)OpCode.LoadStartScene:
                    GameController.Instance()?.LoadScene(_packet);
                    break;
                case (int)OpCode.LoadScene:
                    GameController.Instance()?.LoadScene(_packet);
                    break;
                case (int)OpCode.RejectClaim:
                    GameController.Instance()?.RejectClaim(_packet);
                    break;
                case (int)OpCode.SetFloat:
                    GameController.Instance()?.SetFloat(_packet);
                    break;
                case (int)OpCode.AnchorIDUpdate:
                    GameController.Instance()?.SetAnchorID(_packet);
                    break;
                case (int)OpCode.GameInProgress:
                    GameController.Instance()?.GameInProgress(_packet);
                    break;
                case (int)OpCode.SceneReady:
                    GameController.Instance()?.LoadScene(_packet);
                    break;
                case (int)OpCode.DestroyPendingMatch:
                    GameController.Instance()?.DestroyPendingMatch(_packet);
                    break;
                case (int)OpCode.SendTexture2D:
                    GameController.Instance()?.GetUploadedTexture2D(_packet);
                    break;
                case (int)OpCode.DeleteUploadedImageOnServer:
                    GameController.Instance()?.DeleteUploadedImageOnServer(_packet);
                    break;
                case (int)OpCode.ResolveAnchorId:
                    GameController.Instance()?.ResolveAnchorId(_packet);
                    break;
                case (int)OpCode.ResolvedCloudAnchor:
                    GameController.Instance()?.AllClientsFinishedResolvingCloudAnchor(_packet);
                    break;
            }
        }

        /// <summary> Disconnect the user from the servers when they quit. Used for whole game as this persists due to DontDestroyOnLoad</summary>
        private void OnApplicationQuit()
        {
            new LogEventRequest().SetEventKey("UpdateOnlineStatus").SetEventAttribute("DisconnectFlag", "false").Send((_disconnectResponse) => {});
            Instance()?.GetRTSession()?.Disconnect();
            new LogEventRequest().SetEventKey("Disconnect").SetEventAttribute("MatchShortCode", m_MatchShortCode).Send((_disconnectResponse) => 
            {
                //Wait for the response from the event and then kill the process. Ideally we would quit nicely, but that's not working.
                GS.Disconnect();
                if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
            });
        }

        #region Login & Registration
        /// <summary>
        /// Creates a callback to be used in AuthenticateUser(). This callback will contain the authentication information created by the GameSparks API.
        /// </summary>
        /// <param name="_authenticationResponse">The callback that will contain the authentication response from the GameSparks API.</param>
        /// <param name="_loginSuccessful">Flag indicating if the login was successful.</param>
        /// <param name="_errorMessage">The error message stating why a login was no successful. If successful, then this is empty.</param>
        public delegate void AuthCallback(AuthenticationResponse _authenticationResponse, bool _loginSuccessful, string _errorMessage);
        
        /// <summary>
        /// Creates a callback to be used in AuthenticateUser(). This callback will contain the registration information created by the GameSparks API.
        /// </summary>
        /// <param name="_registrationResponse">The callback that will contain the registration response from the GameSparks API.</param>
        public delegate void RegCallback(RegistrationResponse _registrationResponse);

        /// <summary>
        /// Authenticates the user with the GameSparks API. It first attempts to register the user, if that username already exist then it tries to authenticate them.
        /// This function will also prevent multiple users from logging in with the same name **EVENTUALLY**
        /// </summary>
        /// <param name="_userName">The username passed in through the username input field</param>
        /// <param name="_password">This password is an empty string because we don't require passwords, however the GameSparks API does</param>
        /// <param name="_registrationCallback">This is the callback containing the registration information from the GameSparks API</param>
        /// <param name="_authenticationCallback">This is the callback containing the authentication information from the GameSparks API</param>
        public void AuthenticateUser(string _userName, string _password, RegCallback _registrationCallback, AuthCallback _authenticationCallback)
        {
            //Give username and password (which is an empty string because we don't require a password) create a registration request
            new RegistrationRequest()
                .SetDisplayName(_userName)
                .SetUserName(_userName)
                .SetPassword(_password)
                .Send((_registrationResponse) =>
                {
                    if (!_registrationResponse.HasErrors) //If the registration response did not have any errors in it, then we can register the player
                    {
                        //Check for no errors so we can add our name - prevents async problems by doing it here. Will remove if repeat login
                        AddPlayer(_registrationResponse.UserId); //Temp add player (unless not duplicate login, then this will really add them)
                        _registrationCallback(_registrationResponse);
                    }
                    else //If there was some error
                    {
                        if (!(bool)_registrationResponse.NewPlayer) //Check to see if the error was that this player is not a new player - if so need to authenticate
                        {                           
                            Debug.Log("GSM| Existing User, Switching to Authentication");
                            //Given username and password (remember, password is just an empty string in our scenario) create a authentication request
                            new AuthenticationRequest()
                                .SetUserName(_userName)
                                .SetPassword(_password)
                                .Send((_authenticationResponse) =>
                                {
                                    new LogEventRequest()
                                        .SetEventKey("CheckOnlineStatus")
                                        .Send((_onlineResponse) =>
                                        {
                                            //Extract short code from the configuration data we got
                                            GSData scriptData = _onlineResponse.ScriptData;
                                            string onlineStatus = scriptData.JSON.ToString();

                                            if (onlineStatus.Contains("2")) //Someone is already logged in with this account
                                            {
                                                if (m_PlayerIds.TryGetValue(_authenticationResponse.UserId, out bool me)) //Remove player from our player list - not actually us
                                                {
                                                    m_PlayerIds.Remove(_authenticationResponse.UserId);
                                                }
                                                _authenticationCallback(_authenticationResponse, false, "User already logged in.");
                                            }
                                            else //No one is logged in as this user - we can log in
                                            {
                                                if (!_authenticationResponse.HasErrors) //Authentication was successful
                                                {
                                                    _authenticationCallback(_authenticationResponse, true, string.Empty);
                                                    //Set player ID to true if it is this player, or add ID if we don't have it already
                                                    AddPlayer(_authenticationResponse.UserId);
                                                }
                                                else //Authentication was not successful - reason is not known.
                                                {
                                                    Debug.LogWarning("GSM| Error Authenticating User\n" + _authenticationResponse.Errors.JSON);
                                                    _authenticationCallback(_authenticationResponse, false, _authenticationResponse.Errors.JSON);
                                                }
                                            }

                                        });
                                });
                        }
                        else //Player is a new player, but authentication/registration was unsuccessful
                        {
                            Debug.LogWarning("GSM| Error Authenticating User\n" + _registrationResponse.Errors.JSON);
                        }
                    }
                });
        }

        #endregion

        /// <summary>
        /// Adds a player to the GameSparks match so that all players know each other - true means it is the local player's id
        /// </summary>
        /// <param name="_playerId">The id of the player to add</param>
        private void AddPlayer(string _playerId)
        {
            if (m_PlayerIds.TryGetValue(_playerId, out bool me))
            {
                if (!me)
                {
                    m_PlayerIds[_playerId] = true;
                }
            }
            else
            {
                m_PlayerIds.Add(_playerId, true);
            }
        }

        /// <summary> Returns the current User's PeerId</summary>
        /// <returns>The current User's peerId</returns>
        public int GetPlayerPeerId()
        {
            if (m_PlayerPeerId != int.MaxValue)
            {
                return m_PlayerPeerId;
            }
            foreach (var potentiallyThisPlayer in m_PlayerIds)
            {
                if (potentiallyThisPlayer.Value)
                {
                    m_PlayerPeerId = Instance().GetSessionInfo().GetPlayerList().Find(player => player.m_Id == potentiallyThisPlayer.Key).m_PeerId;
                    return m_PlayerPeerId;
                }
            }
            Debug.LogError("Player peer Id was not found.");
            return int.MaxValue; //Player peer id was not found
        }

        /// <summary>Returns the current lowest peerID value out of all the currently connected players</summary>
        /// <returns>The lowest peer id of all the users in this match</returns>
        public int GetLowestPeerId()
        {
            int lowestPeerId = int.MaxValue;
            foreach (var potentiallyThisPlayer in m_PlayerIds)
            {
                int currentPeer = Instance().GetSessionInfo().GetPlayerList().Find(player => player.m_Id == potentiallyThisPlayer.Key)?.m_PeerId ?? int.MaxValue;
                if (lowestPeerId > currentPeer)
                {
                    lowestPeerId = currentPeer;
                }
            }
            return lowestPeerId;
        }

        /// <summary>
        /// Returns true if the caller is the lowest peer id user in the match. This is a good way to assign a "Host" player if desired.
        /// Though do keep in mind that ASL is a P2P network.
        /// </summary>
        /// <returns>True if caller has the lowest peer id</returns>
        public bool AmLowestPeer()
        {
            int currentLowest = GetLowestPeerId();
            GetPlayerPeerId();
            if (currentLowest == m_PlayerPeerId)
            {
                return true;
            }
            return false;
        }
    }
}