using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


namespace ASL
{
    /// <summary>
    /// This class connects us to other players via functions in <see cref="GameSparksManager"/> and itself regardless if <see cref="QuickConnect"/> is being used or not. 
    /// This scene must always be built.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        #region Private Variables 
        /// <summary>Contains the names of all of our Lobby Manager panels for clearer reading when calling DisplayPanel()</summary>
        private enum PanelSelection { MainMenu, ConnectionMenu, HostMenu, RoomView, LobbyScreen, Setup };
        /// <summary>Informs this class if <see cref="QuickConnect"/> is being used or not to set up a connection, adjusting what is displayed to the user based on this value</summary>
        private bool m_QuickConnect;
        /// <summary>The name of the room that users should join</summary>
        private string m_RoomName;
        /// <summary> Hold the match details needed to join a match </summary>
        private struct m_MatchDetails
        {
            /// <summary>The id of the match to join</summary>
            public string id; //Needed to join a match
            /// <summary>The short code of the match to join</summary>
            public string shortCode;
        }

        /// <summary>The match that a user selected to join</summary>
        private m_MatchDetails m_ChosenMatch;

        /// <summary>The position of the old match button</summary>
        private Vector3 m_OldMatchButtonPosition = Vector3.zero;

        /// <summary> List of match short codes gathered from the GameSparks API. These are used to host and find matches.</summary>
        private List<string> m_MatchShortCodes = null;

        /// <summary> Creates a temporary RTSession which will hold data to be used in the real RTSession when the users ready up - thus launching the real RTSession </summary>
        private RTSessionInfo m_TempRTSessionInfo =  null;

        /// <summary>This panel stays visible throughout the lobby, displaying 
        /// the user's username and connection status</summary>
        private GameObject m_InfoTextPanel = null;

        /// <summary>This panel is the first one to displayed on scene load. 
        /// It allows the user to create a username and connect</summary>
        private GameObject m_MainMenuPanel = null;

        /// <summary>This panel appears after the user has created a user name and connected.
        /// It gives the user the option to host a session or look for one.</summary>
        private GameObject m_ConnectionMenuPanel = null;

        /// <summary>This is the panel that appears when the user chooses to host a session.
        /// It gives the user the option to select the match type and maximum player count</summary>
        private GameObject m_HostMenuPanel = null;

        /// <summary>This is the panel that appears when the user chooses to find a session.
        /// It displays all of the matches currently being hosted and allows the player to join
        /// any of the ones available.</summary>
        private GameObject m_RoomViewPanel = null;

        /// <summary>This text displays the user's username while in the LobbyScene. 
        /// This is a child of the Info Text Panel.</summary>
        private GameObject m_UserNameText = null;

        /// <summary>This text displays the user's connection status with GameSparks API while they are in the LobbyScene.
        /// It is a child of the Info Text Panel.</summary>
        private GameObject m_ConnectionStatusText = null;

        /// <summary>This input field is where the user inputs their username. It is a child of the Main Menu Panel.</summary>
        private GameObject m_UsernameInput = null;

        /// <summary>This button is what allows the player to select their username - connecting them to the GameSparks API
        /// and moves them to the connection menu panel. It is a child of the Main Menu Panel.</summary>
        public static GameObject m_ConnectButton = null;

        /// <summary> This button launches the host menu panel for the user. From here they can set up the session they want to host. </summary>
        private GameObject m_HostButton = null;

        /// <summary> This button launches the Room View panel where users can see all the games currently being hosted. /// </summary>
        private GameObject m_FindButton = null;

        /// <summary> The dropdown menu that the host can select to determine what match they will be hosting.</summary>
        private GameObject m_HostSelectionDropDown = null;

        /// <summary> This button starts hosting a match for the selected match in m_HostSelectionDropDown.</summary>
        private GameObject m_StartHostingButton = null;

        /// <summary> The screen the player sees after they have joined a match. </summary>
        private GameObject m_LobbyScreen = null;

        /// <summary> Panel indicating to the user that they will be joining the lobby shortly. </summary>
        private GameObject m_SetupPanel = null;

        /// <summary> The button that player pushes to indicate they are ready for the match to begin. </summary>
        public static GameObject m_ReadyButton = null;

        /// <summary> List of all of the players on the lobby screen. </summary>
        private GameObject m_PlayerListText = null;

        /// <summary> The amount of players out of total players in a match - displayed to the user on the lobby screen. </summary>
        private GameObject m_PlayerCountText = null;

        /// <summary> The name of the match displayed to the user on the lobby screen. </summary>
        private GameObject m_MatchNameText = null;

        /// <summary> Displays the error that occurred when attempting to host a match. </summary>
        private GameObject m_HostErrorText = null;

        /// <summary> Displays the error that occurred when attempting to login. </summary>
        private GameObject m_LoginErrorText = null;

        /// <summary> Displays the error the at occurred when attempting to search for a match. </summary>
        private GameObject m_SearchMatchErrorText = null;

        /// <summary> Returns the user to the Connection Menu panel. </summary>
        private GameObject m_BackButton = null;

        /// <summary> Refreshes the search for finding a room (sends the request again). </summary>
        private GameObject m_RefreshButton = null;

        /// <summary> Using data gathered from the match clicked on, the user joins a match. </summary>
        private GameObject m_JoinMatchButton = null;

        /// <summary>Flag indicating if we can join a match yet (server calls are set up)</summary>
        private bool m_JoinMatchButtonInteractable = false;

        /// <summary> The prefab that will spawn buttons containing match info for users to join a match with. </summary>
        private GameObject m_MatchButtonPrefab = null;

        /// <summary> Allows the user to name their match/room. </summary>
        private GameObject m_RoomNameInputField = null;

        /// <summary> Display any errors when attempting to join a match</summary>
        private GameObject m_JoinMatchErrorText = null;

        /// <summary> String containing the max player count for a match</summary>
        private string m_MaxPlayerCount = "0";

        /// <summary>A version of the room name that does contain illegal characters (like a space)</summary>
        private string m_cleanRoomName = "";

        /// <summary>Flag indicating if user is hosting a match</summary>
        private bool m_hosting = false;
        #endregion

        #region Public Variables
        /// <summary>
        /// Static string that represents the scene name of the scene to load after all players ready up. Static so that it can be seen in other classes without having to have an
        /// object of this class instantiated
        /// </summary>
        public static string m_StaticStartingScene = string.Empty;
        /// <summary>
        /// The non-static string that represents scene name of the scene to load after all players ready up. Used to set its static counter part since it can be seen in the Unity Editor and static 
        /// variables cannot.
        /// </summary>
        public string m_StartingScene = string.Empty;

        /// <summary>Flag indicating if user has switched to manual match making - used when QuickConnect person leaves a room</summary>
        public static bool m_SwitchedToManualMatchMaking = false;

        /// <summary>Flag indicating that a game has started and that no other users can join that room/match anymore</summary>
        public static bool m_GameStarted = false;

        #endregion

        /// <summary>Start is called before the first frame update</summary>
        void Start()
        {
            #region Initialize needed UI elements
            
            m_InfoTextPanel = GameObject.Find("InfoTextPanel");          
            m_UserNameText = GameObject.Find("UsernameText");
            m_ConnectionStatusText = GameObject.Find("ConnectionStatusText");

            m_MainMenuPanel = GameObject.Find("MainMenuPanel");
            m_UsernameInput = GameObject.Find("UsernameInputField");
            m_ConnectButton = GameObject.Find("ConnectButton");
            m_LoginErrorText = GameObject.Find("LoginErrorText");

            m_ConnectionMenuPanel = GameObject.Find("ConnectionMenuPanel");
            m_HostButton = GameObject.Find("HostSessionButton");
            m_FindButton = GameObject.Find("FindSessionButton");

            m_HostMenuPanel = GameObject.Find("HostMenuPanel");
            m_HostSelectionDropDown = GameObject.Find("MatchNamesDropDown");
            m_RoomNameInputField = GameObject.Find("RoomNameInputField");
            m_StartHostingButton = GameObject.Find("StartHostingButton");
            m_HostErrorText = GameObject.Find("HostErrorText");

            m_RoomViewPanel = GameObject.Find("RoomViewPanel");
            m_RefreshButton = GameObject.Find("RefreshButton");
            m_JoinMatchButton = GameObject.Find("JoinMatchButton");
            m_SearchMatchErrorText = GameObject.Find("SearchMatchErrorText");
            m_JoinMatchErrorText = GameObject.Find("JoinMatchErrorText");

            m_LobbyScreen = GameObject.Find("LobbyScreenPanel");
            m_MatchNameText = GameObject.Find("MatchNameText");
            m_PlayerCountText = GameObject.Find("PlayerCountText");
            m_PlayerListText = GameObject.Find("PlayerListText");
            m_ReadyButton = GameObject.Find("ReadyButton");

            m_SetupPanel = GameObject.Find("SetupPanel");

            m_BackButton = GameObject.Find("BackButton");
                        
            m_MatchButtonPrefab = (GameObject)Resources.Load("ASL_Prefabs/AvailableMatchButtons", typeof(GameObject));
                        
            #endregion

            #region Debug.Assert statements
            Debug.Assert(m_InfoTextPanel != null, "The Info Text Panel was null - attach it to this script");
            Debug.Assert(m_MainMenuPanel != null, "The Main Menu Panel was null - attach it to this script");
            Debug.Assert(m_ConnectionMenuPanel != null, "The Connection Menu Panel was null - attach it to this script");
            Debug.Assert(m_HostMenuPanel != null, "Info Host Menu Panel was null - attach it to this script");
            Debug.Assert(m_RoomViewPanel != null, "Info Room View Panel was null - attach it to this script");
            Debug.Assert(m_UserNameText != null, "Username text object under the Info Text Panel was null - attach it to this script");
            Debug.Assert(m_ConnectionStatusText != null, "Connection Status text object under the Info Text Panel was null - attach it to this script");
            Debug.Assert(m_UsernameInput != null, "Username Input Field under the Main Menu Panel was null - attach it to this script");
            Debug.Assert(m_ConnectButton != null, "Connect button under the Main Menu Panel was null - attach it to this script");
            Debug.Assert(m_HostButton != null, "Host Session button under the Connection Menu Panel was null - attach it to this script");
            Debug.Assert(m_FindButton != null, "Find button under the Connection Menu Panel was null - attach it to this script");
            Debug.Assert(m_HostSelectionDropDown != null, "Host dropdown selection under the Host Menu Panel was null - attach it to this script");
            Debug.Assert(m_StartHostingButton != null, "Start hosting button under the Host Menu Panel was null - attach it to this script");
            Debug.Assert(m_LobbyScreen != null, "The Lobby Screen panel was null - attach it to this script");
            Debug.Assert(m_ReadyButton != null, "Ready button under the Lobby Screen panel was null - attach it to this script");
            Debug.Assert(m_PlayerListText != null, "Players text under PlayerHolder in Lobby Screen panel was null - attach it to this script");
            Debug.Assert(m_PlayerCountText != null, "Player Count text under MatchDetails in Lobby Screen panel was null - attach it to this script");
            Debug.Assert(m_MatchNameText != null, "Match Name text under MatchDetails in Lobby Screen panel was null - attach it to this script");
            Debug.Assert(m_HostErrorText != null, "Host Error Text text in Host Menu panel was null - attach it to this script");
            Debug.Assert(m_SearchMatchErrorText != null, "Search Match Text text in Room Find panel was null - attach it to this script");
            Debug.Assert(m_BackButton != null, "Back button was null - attach it to this script");
            Debug.Assert(m_RefreshButton != null, "Refresh button in Room View panel was null - attach it to this script");
            Debug.Assert(m_JoinMatchButton != null, "Join button in Room View panel was null - attach it to this script");
            Debug.Assert(m_MatchButtonPrefab != null, "Join button in Room View panel was null - attach it to this script");
            Debug.Assert(m_RoomNameInputField != null, "User Defined Room Name Input Field in the Host Menu panel was null - attach it to this script");
            Debug.Assert(m_JoinMatchErrorText != null, "Join Match Error text in the Room View panel was null - attach it to this script");
            Debug.Assert(m_LoginErrorText != null, "Login Error text in the Main Menu Panel was null - attach it to this script");

        #endregion

            //Set match button to starting position - will increment whenever we add a new match
            m_OldMatchButtonPosition = m_MatchButtonPrefab.transform.localPosition;
            m_QuickConnect = QuickConnect.m_StaticQuickStart;
            m_RoomName = QuickConnect.m_StaticRoomName;
            m_ConnectionStatusText.GetComponent<Text>().text = "No Connection..."; //Let the user know they aren't connected yet.
            m_SearchMatchErrorText.SetActive(false);
            m_HostErrorText.SetActive(false);
            m_JoinMatchErrorText.SetActive(false);
            m_ReadyButton.GetComponent<Button>().interactable = false;
            //Set Main Menu to be the current panel showing
            DisplayPanel(PanelSelection.MainMenu);
            m_StaticStartingScene = m_StartingScene;
            m_ConnectButton.GetComponent<Button>().interactable = false;
            m_JoinMatchButton.GetComponent<Button>().interactable = false;
            m_HostButton.GetComponent<Button>().interactable = false;
            m_FindButton.GetComponent<Button>().interactable = false;
            //Button Click Listeners
            #region GUI Listeners

            m_BackButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                m_HostButton.GetComponent<Button>().interactable = false;
                m_FindButton.GetComponent<Button>().interactable = false;
                m_JoinMatchButton.GetComponent<Button>().interactable = false;
                m_JoinMatchButtonInteractable = false;
                //Disconnect from anything we may be connected to via the lobby screen (just 1 match)
                if (m_LobbyScreen.activeSelf)
                {
                    if (m_ChosenMatch.shortCode == "QS")
                    {
                        Disconnect(m_ChosenMatch.shortCode, m_cleanRoomName, true);
                    }
                    else
                    {
                        Disconnect(m_ChosenMatch.shortCode, "", true);
                    }                                   
                }
                //Disconnect from pending matches we created (this will destroy the match as we are the only ones in them) 
                if (m_RoomViewPanel.activeSelf)
                {
                    foreach (string match in m_MatchShortCodes)
                    {
                        Disconnect(match, "");
                    }
                }
                DisplayPanel(PanelSelection.ConnectionMenu);
                if (m_MatchShortCodes == null || m_MatchShortCodes?.Count == 0) //If we haven't generated short codes yet
                {
                    GetMatchShortCodes();
                }
                m_SwitchedToManualMatchMaking = true;
            });

            m_ConnectButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                // string.empty is passed in for a password because we don't require users have to a password but the GameSparks API does
                GameSparksManager.Instance().AuthenticateUser(m_UsernameInput.GetComponentsInChildren<Text>()[1].text, string.Empty, OnRegistration, OnAuthentication);
                m_LoginErrorText.GetComponent<Text>().text = string.Empty;
            });

            m_HostButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                m_StartHostingButton.GetComponent<Button>().interactable = false;
                //Bring up the Host Menu Panel
                DisplayPanel(PanelSelection.HostMenu);
                SetupHostDropDownMatchOptions();
            });

            m_StartHostingButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                //Setting up match screen:
                DisplayPanel(PanelSelection.Setup);
                //Start the match making process
                HostRoom();
            });

            m_FindButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                //Bring up the Room Find Panel
                DisplayPanel(PanelSelection.RoomView);
                //Search for rooms
                SearchForHostedRooms();
            });

            m_JoinMatchButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                //Join the selected match
                JoinMatch();
            });

            //This is a listener for the Ready Button. On click, we will pass the stored RTSessionInfo to the GameSparksManager to create a new RT session
            m_ReadyButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameSparksManager.Instance().StartNewRTSession(m_TempRTSessionInfo);
            });

            //Search for rooms again
            m_RefreshButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                m_JoinMatchErrorText.SetActive(false);
                m_SearchMatchErrorText.SetActive(false);
                SearchForHostedRooms();
            });

            #endregion

            #region GameSpark API Listeners
            //This listener will update the text in the match details field if no match was found
            GameSparks.Api.Messages.MatchNotFoundMessage.Listener = (message) => 
            {
                m_HostErrorText.SetActive(true);
                m_HostErrorText.GetComponent<Text>().text = "Failed to host match: \"" + message.JSONString + "\"";
                m_BackButton.gameObject.SetActive(true);
                DisplayPanel(PanelSelection.HostMenu);
            };

            //This listener will inform players they found a match
            GameSparks.Api.Messages.MatchFoundMessage.Listener += OnMatchFound;
            //This listener will inform players an update has occurred in their match (like a new player joining or leaving)
            GameSparks.Api.Messages.MatchUpdatedMessage.Listener += OnMatchUpdate;
            #endregion

        }

        /// <summary> Create a new match making request, hosting a match based upon the dropdown menu option </summary>
        private void HostRoom()
        {
            m_hosting = true;
            QuickConnect.m_StaticQuickStart = true; //Not technically true, but prevents QuickConnect from firing upon scene load
            //Gather data for hosting the match
            string matchShortCode = m_MatchShortCodes[m_HostSelectionDropDown.GetComponent<Dropdown>().value];
            m_cleanRoomName = m_RoomNameInputField.GetComponentsInChildren<Text>()[1].text;
            GSRequestData matchData = new GSRequestData();
            matchData.AddString("m_cleanRoomName", m_cleanRoomName);

            //Get max player count
            new LogEventRequest()
                .SetEventKey("GetFoundMatchInfo")
                .SetEventAttribute("MatchShortCode", matchShortCode)
                .Send((_nameResponse) =>
                {
                    //Since these calls are asynchronous, call host from inside here
                    var matchInfo = JsonUtility.FromJson<JSONFoundMatchInfoConverter>(_nameResponse.ScriptData.JSON);
                    string maxPlayerCount = matchInfo.m_MaxPlayerCount.ToString();
                    matchData.AddString("maxPlayerCount", maxPlayerCount);

                    //Start hosting the match
                    new MatchmakingRequest()
                        .SetMatchShortCode(matchShortCode)
                        .SetSkill(0)
                        .SetMatchData(matchData)
                        .Send((_matchMakingResponse) =>
                        {
                            if (_matchMakingResponse.HasErrors)
                            {
                                Debug.LogError("GSM| Matchmaking Error\n" + _matchMakingResponse.Errors.JSON);
                                m_HostErrorText.SetActive(true);
                                m_HostErrorText.GetComponent<Text>().text = "Error attempting to host match: \"" + _matchMakingResponse.Errors.JSON + "\"";
                                DisplayPanel(PanelSelection.HostMenu);
                            }
                            else //Host was successful - update lobby screen 
                            {
                                //Add player count and match name to lobby screen
                                m_PlayerCountText.GetComponent<Text>().text = "1/" + matchInfo.m_MaxPlayerCount.ToString();
                                m_MatchNameText.GetComponent<Text>().text = matchShortCode + ": " + m_cleanRoomName;
                                 
                                //Add player name to the player list in the lobby screen
                                m_PlayerListText.GetComponent<Text>().text = m_UsernameInput.GetComponentsInChildren<Text>()[1].text; //Display the users in the room
                                m_ChosenMatch.shortCode = matchShortCode; //Update matchShortCode in case "host" disconnects
                                GameSparksManager.Instance().SetMatchShortCode(m_ChosenMatch.shortCode);
                            }
                        });

                });

        }

        /// <summary> Search for and display the possible matches the player could join </summary>
        private void SearchForHostedRooms()
        {
            m_hosting = false;
            foreach (string shortCode in m_MatchShortCodes)
            {
                //Destroy any duplicate match buttons (remove them all as we're about to repopulate it anyways)
                ResetRoomView();

                //Put the user into match making (thus making them available to match with others)
                new MatchmakingRequest()
                    .SetMatchShortCode(shortCode)
                    .SetSkill(0)
                    .Send((_matchMakingResponse) =>
                    {
                        if (_matchMakingResponse.HasErrors)
                        {
                            Debug.LogError("GSM| Matchmaking Error\n" + _matchMakingResponse.Errors.JSON);
                            m_SearchMatchErrorText.SetActive(true);
                            m_SearchMatchErrorText.GetComponent<Text>().text = "Error attempting to join match making: \"" + _matchMakingResponse.Errors.JSON + "\"";
                        }
                    });

                //Have the player search for pending matches
                new FindPendingMatchesRequest()
                    .SetMatchShortCode(shortCode)
                    .Send((_findMatchResponse) =>
                    {
                        if (_findMatchResponse.HasErrors)
                        {
                            Debug.LogError("GSM| Finding match Error\n" + _findMatchResponse.Errors.JSON);
                            m_SearchMatchErrorText.SetActive(true);
                            m_SearchMatchErrorText.GetComponent<Text>().text = "Error attempting to find match: \"" + _findMatchResponse.Errors.JSON + "\"";
                        }
                        else
                        {
                            m_SearchMatchErrorText.SetActive(true);
                            m_SearchMatchErrorText.GetComponent<Text>().text = "Loading matches...";
                            foreach (var match in _findMatchResponse.PendingMatches)
                            {
                                //If this match does not have a room name, then that means it was created so a user could join a match - not host
                                //So it shouldn't be shown.
                                if (match.MatchData.GetString("m_cleanRoomName")?.ToString() == null)
                                {
                                    continue;
                                }
                                //Create the match button
                                Button aMatch = Instantiate(m_MatchButtonPrefab, GameObject.Find("FindMatchesScrollWindow").transform).GetComponent<Button>();

                                aMatch.transform.localPosition = m_OldMatchButtonPosition;
                                //Update the old position so next match button will be placed correctly
                                m_OldMatchButtonPosition.y = m_OldMatchButtonPosition.y - 60f;

                                //Set the different texts of this button (ShortCode, MatchName, PlayerCount, MatchID)
                                var buttonText = aMatch.GetComponentsInChildren<Text>();
                                buttonText[0].text = match.MatchShortCode;
                                buttonText[1].text = match.MatchData.GetString("m_cleanRoomName");
                                buttonText[2].text = match.MatchedPlayers.Count().ToString() + "/" + match.MatchData.GetString("maxPlayerCount");

                                //When clicked on, record data so we can join that match if the join button is clicked

                                aMatch.onClick.AddListener(() =>
                                {
                                    if (m_JoinMatchButtonInteractable)
                                    {
                                        m_JoinMatchButton.GetComponent<Button>().interactable = true;
                                    }
                                    else
                                    {
                                        m_JoinMatchButton.GetComponent<Button>().interactable = false;                       
                                    }
                                    m_ChosenMatch.id = match.Id;
                                    m_ChosenMatch.shortCode = match.MatchShortCode;
                                });

                                aMatch.tag = "PotentialMatchButton";
                                aMatch.interactable = false;
                            }
                        }

                    });
            }
        }

        /// <summary> Requests to join the selected match</summary>
        private void JoinMatch()
        {
            QuickConnect.m_StaticQuickStart = true; //Not technically true, but prevents QuickConnect from firing upon scene load
            //Since we disconnected from our matches, reconnect to match making
            new MatchmakingRequest()
                .SetMatchShortCode(m_ChosenMatch.shortCode)
                .SetSkill(0)
                .Send((_matchMakingResponse) =>
                {
                    if (_matchMakingResponse.HasErrors)
                    {
                        Debug.LogError("GSM| Matchmaking Error\n" + _matchMakingResponse.Errors.JSON);
                        m_SearchMatchErrorText.SetActive(true);
                        m_SearchMatchErrorText.GetComponent<Text>().text = "Error attempting to join match making: \"" + _matchMakingResponse.Errors.JSON + "\"";
                    }
                    else
                    {
                        //Join the match we wanted to join now that we're back in match making
                        m_hosting = true; //Not really hosting, but ready to join a match
                        new JoinPendingMatchRequest()
                            .SetMatchShortCode(m_ChosenMatch.shortCode)
                            .SetPendingMatchId(m_ChosenMatch.id)
                            .Send((_JoinPendingMatchResponse) =>
                            {
                                if (_JoinPendingMatchResponse.HasErrors)
                                {
                                    Debug.LogError("GSM| Finding match Error\n" + _JoinPendingMatchResponse.Errors.JSON);
                                    m_JoinMatchErrorText.SetActive(true);
                                    m_JoinMatchErrorText.GetComponent<Text>().text = "Error attempting to join match: \"" + _JoinPendingMatchResponse.Errors.JSON + "\"";
                                }
                                else
                                {
                                    GameSparksManager.Instance().SetMatchShortCode(m_ChosenMatch.shortCode);
                                }
                            });
                    }
                });



            
        }

        /// <summary> Disconnects the user from the specified match </summary>
        /// <param name="_shortCode">the match short code used to determine what match to disconnect from</param>
        /// /// <param name="_matchGroup">The name of the match group this player is a part of</param>
        /// /// <param name="_EndRTSession">Flag indicating whether or not the RT session should be ended with this disconnect</param>
        private void Disconnect(string _shortCode, string _matchGroup, bool _EndRTSession = false)
        {
            new LogEventRequest().SetEventKey("Disconnect").SetEventAttribute("MatchShortCode", _shortCode).
                SetEventAttribute("MatchGroup", _matchGroup).Send((_disconnectResponse) => 
                {
                    GameSparksManager.Instance()?.GetRTSession()?.Disconnect();
                    if (_EndRTSession)
                    {
                        //Disconnect from RT Session 
                        GameSparksManager.Instance().EndRTSession(leaveMatch: true); //Leave the match as well as disconnect from RT
                    }
                    m_HostButton.GetComponent<Button>().interactable = true;
                    m_FindButton.GetComponent<Button>().interactable = true;
                });
        }

        /// <summary>
        /// Triggers when a match is found either through hosting or through selecting a match. Sets the lobby screen variables
        /// </summary>
        /// <param name="_message">The message from the GameSparks API</param>
        private void OnMatchFound(GameSparks.Api.Messages.MatchFoundMessage _message)
        {
            //If we don't have ourselves as an ID yet, then we can't find a match (though from a GameSparks perspective we can)
            if (!GameSparksManager.m_PlayerIds.ContainsValue(true)) { return; }

            //Get matchGroup
            string matchGroup = _message.MatchGroup;
            if (string.IsNullOrEmpty(matchGroup))
            {
                matchGroup = "";
            }
            GameSparksManager.Instance().SetMatchGroup(matchGroup);

            //Don't transition to the lobby screen if just looking for a match
            if (!m_hosting && (!m_QuickConnect || m_SwitchedToManualMatchMaking))
            {
                m_JoinMatchButton.GetComponent<Button>().interactable = false;
                m_JoinMatchButtonInteractable = false;
                //Disconnect from match instance so we can join the match we want to join (since we're looking at matches)
                new LogEventRequest().SetEventKey("Disconnect").SetEventAttribute("MatchShortCode", _message.MatchShortCode).
                    SetEventAttribute("MatchGroup", matchGroup).Send((_disconnectResponse) =>
                    {
                        m_JoinMatchButtonInteractable = true;
                        foreach (GameObject _potentialMatch in GameObject.FindGameObjectsWithTag("PotentialMatchButton"))
                        {
                            _potentialMatch.GetComponent<Button>().interactable = true;
                        }
                        m_SearchMatchErrorText.GetComponent<Text>().text = "";
                        m_SearchMatchErrorText.SetActive(false);
                    });

                return;
            }

            DisplayPanel(PanelSelection.LobbyScreen);
            GameSparksManager.Instance().m_MyMatchId = _message.MatchId;
            m_TempRTSessionInfo = new RTSessionInfo(_message); // we'll store the match data until we need to create an RT session instance
            
            //Trigger Cloud event to gather max player count for this match and also grab the match name, placing these variables on the lobby screen.
            new LogEventRequest()
                .SetEventKey("GetFoundMatchInfo")
                .SetEventAttribute("MatchShortCode", _message.MatchShortCode)
                .Send((_nameResponse) =>
                {
                    var matchInfo = JsonUtility.FromJson<JSONFoundMatchInfoConverter>(_nameResponse.ScriptData.JSON);
                    StringBuilder matchDetails = new StringBuilder();
                    m_MaxPlayerCount = matchInfo.m_MaxPlayerCount.ToString();
                    m_PlayerCountText.GetComponent<Text>().text = _message.Participants.Count() + "/" + m_MaxPlayerCount;
                    m_MatchNameText.GetComponent<Text>().text = _message.MatchShortCode + ": " + _message.MatchData.GetString("m_cleanRoomName");
                    m_ReadyButton.GetComponent<Button>().interactable = true; //Allow user to ready up
                });

            //Add player name to the player list 
            StringBuilder playerList = new StringBuilder();

            foreach (GameSparks.Api.Messages.MatchFoundMessage._Participant player in _message.Participants)
            {
                playerList.AppendLine(player.DisplayName); //Add the player number and display name to the list
                if (!GameSparksManager.m_PlayerIds.ContainsKey(player.Id))
                {
                    GameSparksManager.m_PlayerIds.Add(player.Id, false);
                }

            }
            m_PlayerListText.GetComponent<Text>().text = playerList.ToString(); //Display the users in the room
            
        }

        /// <summary>
        /// Triggers when a match is found either through hosting or through selecting a match. Sets the lobby screen variables
        /// </summary>
        /// <param name="_message">The message from the GameSparks API</param>
        private void OnMatchUpdate(GameSparks.Api.Messages.MatchUpdatedMessage _message)
        {
            if (m_GameStarted) {return;} //No need to update lobby screen if game is started
            m_TempRTSessionInfo.GetPlayerList().Clear(); //Clear player list so we can ensure it is correctly updated
            //Add player name to the player list 
            StringBuilder playerList = new StringBuilder();
            foreach (GameSparks.Api.Messages.MatchUpdatedMessage._Participant player in _message.Participants)
            {
                playerList.AppendLine(player.DisplayName); //Add the player number and display name to the list
                if (!GameSparksManager.m_PlayerIds.ContainsKey(player.Id))
                {
                    GameSparksManager.m_PlayerIds.Add(player.Id, false);
                }
                Debug.Log("Updating player list");
                m_TempRTSessionInfo.GetPlayerList().Add(new RTPlayer(player.DisplayName, player.Id, (int)player.PeerId));
            }
            m_PlayerCountText.GetComponent<Text>().text = _message.Participants.Count() + "/" + m_MaxPlayerCount;
            m_PlayerListText.GetComponent<Text>().text = playerList.ToString(); //Display the users in the room
            m_ReadyButton.GetComponent<Button>().interactable = true; //Allow user to ready up


            //If a player left the match, restart the ready up process
            if (_message?.RemovedPlayers?.Count > 0)
            {
                //If connected to RT session, disconnect
                GameSparksManager.Instance().EndRTSession();
                //Remove player from our player list
                foreach (var player in _message.RemovedPlayers)
                {
                    if (GameSparksManager.m_PlayerIds.ContainsKey(player))
                    {
                        GameSparksManager.m_PlayerIds.Remove(player);
                    }
                }
                
            }

        }

        /// <summary>
        /// Set active the passed in panel while deactivating all others.
        /// </summary>
        /// <param name="_panel">The panel to become active</param>
        private void DisplayPanel(PanelSelection _panel)
        {
            switch (_panel)
            {
                case PanelSelection.MainMenu:
                    m_MainMenuPanel.SetActive(true);
                    m_ConnectionMenuPanel.SetActive(false);
                    m_HostMenuPanel.SetActive(false);
                    m_RoomViewPanel.SetActive(false);
                    m_LobbyScreen.SetActive(false);
                    m_BackButton.gameObject.SetActive(false);
                    m_SetupPanel.gameObject.SetActive(false);
                    break;
                case PanelSelection.ConnectionMenu:
                    m_MainMenuPanel.SetActive(false);
                    m_ConnectionMenuPanel.SetActive(true);
                    m_HostMenuPanel.SetActive(false);
                    m_RoomViewPanel.SetActive(false);
                    m_LobbyScreen.SetActive(false);
                    m_BackButton.gameObject.SetActive(false);
                    m_HostErrorText.gameObject.SetActive(false);
                    m_SetupPanel.gameObject.SetActive(false);
                    break;
                case PanelSelection.HostMenu:
                    m_MainMenuPanel.SetActive(false);
                    m_ConnectionMenuPanel.SetActive(false);
                    m_HostMenuPanel.SetActive(true);
                    m_RoomViewPanel.SetActive(false);
                    m_LobbyScreen.SetActive(false);
                    m_BackButton.gameObject.SetActive(true);
                    m_SetupPanel.gameObject.SetActive(false);
                    break;
                case PanelSelection.RoomView:
                    m_MainMenuPanel.SetActive(false);
                    m_ConnectionMenuPanel.SetActive(false);
                    m_HostMenuPanel.SetActive(false);
                    m_RoomViewPanel.SetActive(true);
                    m_LobbyScreen.SetActive(false);
                    m_BackButton.gameObject.SetActive(true);
                    m_SetupPanel.gameObject.SetActive(false);
                    break;
                case PanelSelection.LobbyScreen:
                    m_MainMenuPanel.SetActive(false);
                    m_ConnectionMenuPanel.SetActive(false);
                    m_HostMenuPanel.SetActive(false);
                    m_RoomViewPanel.SetActive(false);
                    m_LobbyScreen.SetActive(true);
                    m_BackButton.gameObject.SetActive(true);
                    m_SetupPanel.gameObject.SetActive(false);
                    break;
                case PanelSelection.Setup:
                    m_MainMenuPanel.SetActive(false);
                    m_ConnectionMenuPanel.SetActive(false);
                    m_HostMenuPanel.SetActive(false);
                    m_RoomViewPanel.SetActive(false);
                    m_LobbyScreen.SetActive(false);
                    m_BackButton.gameObject.SetActive(false);
                    m_SetupPanel.gameObject.SetActive(true);
                    break;
                default:
                    break;

            }
            
        }

        /// <summary>
        /// Gets called every time a player gets registered with the GameSparks API for ASL. We use it to update the user's username 
        /// and connection status as well as move them to the next panel
        /// </summary>
        /// <param name="_response">The callback variable from the GameSparks API.</param>
        private void OnRegistration(RegistrationResponse _response)
        {
            //Update user's information for display
            m_UserNameText.GetComponent<Text>().text = "Username: " + _response.DisplayName;
            m_ConnectionStatusText.GetComponent<Text>().text = "Connection Status: Connected";

            Connect();

        }

        /// <summary>
        /// Gets called every time a player gets authenticated with the GameSparks API for ASL. We use it to update the user's username
        /// and connection status as well as move them to the next panel.
        /// </summary>
        /// <param name="_response">The callback variable from the GameSparks API.</param>
        /// /// <param name="_loginSuccessful">Flag indicating if login was successful</param>
        /// /// <param name="_errorMessage">The error message received if Authentication fails</param>
        private void OnAuthentication(AuthenticationResponse _response, bool _loginSuccessful, string _errorMessage)
        {
            if (_loginSuccessful)
            {
                //Update user's information for display
                m_UserNameText.GetComponent<Text>().text = "Username: " + _response.DisplayName;
                m_ConnectionStatusText.GetComponent<Text>().text = "Connection Status: Connected";

                Connect();
            }
            else
            {
                m_LoginErrorText.GetComponent<Text>().text = "Error: " + _errorMessage;
            }
        }

        /// <summary>
        /// Connects a user to a game using GameSparks auto-matchmaking ability based upon the room name that the programmer specified in the Unity Editor 
        /// for the <see cref="QuickConnect"/> script if they are using <see cref="QuickConnect"/>. Otherwise is moves onto the next step of manually connecting players
        /// </summary>
        private void Connect()
        {
            if (m_QuickConnect && !m_SwitchedToManualMatchMaking)
            {
                GSRequestData matchData = new GSRequestData();
                m_cleanRoomName = Regex.Replace(m_RoomName, @"\s+", "");
                matchData.AddString("m_cleanRoomName", m_cleanRoomName);
                new MatchmakingRequest()
                .SetMatchShortCode("QS") //Quick Start short code
                .SetSkill(1)
                .SetMatchData(matchData)
                .SetMatchGroup(m_cleanRoomName)
                .Send((_matchMakingResponse) =>
                {
                    if (_matchMakingResponse.HasErrors)
                    {
                        Debug.LogError("GSM| Matchmaking Error\n" + _matchMakingResponse.Errors.JSON);
                        m_HostErrorText.SetActive(true);
                        m_HostErrorText.GetComponent<Text>().text = "Error attempting to host match: \"" + _matchMakingResponse.Errors.JSON + "\"";
                        DisplayPanel(PanelSelection.HostMenu);
                    }
                    else //Host was successful - update lobby screen 
                    {
                        DisplayPanel(PanelSelection.LobbyScreen);

                        //Add player count and match name to lobby screen
                        m_PlayerCountText.GetComponent<Text>().text = "1/20";
                        m_MatchNameText.GetComponent<Text>().text = "QS" + ": " + m_RoomName;

                        //Add player name to the player list in the lobby screen
                        m_PlayerListText.GetComponent<Text>().text = m_UsernameInput.GetComponentsInChildren<Text>()[1].text; //Display the users in the room
                        m_ChosenMatch.shortCode = "QS"; //Update matchShortCode in case "host" disconnects
                        GameSparksManager.Instance().SetMatchShortCode(m_ChosenMatch.shortCode);
                    }
                });
            }
            else
            {
                //Switch to the connection menu
                DisplayPanel(PanelSelection.ConnectionMenu);
                GetMatchShortCodes();
            }
        }

        /// <summary>
        /// Add the matches have been created in the GameSparks API/browser (https://portal2.gamesparks.net/games/G376367AmCEW/config/multiplayer/matches) 
        /// to our dropdown menu for the user to select what match they want to host. These matches were discovered by GetMatchShortCodes() called in our start function
        /// </summary>
        private void SetupHostDropDownMatchOptions()
        {
            //Using the match short codes, get the match names to display to the user (more friendly than the short codes)
            new LogEventRequest()
                .SetEventKey("GetMatchNames")
                .SetEventAttribute("MatchName", m_MatchShortCodes)
                .Send((_nameResponse) =>
                {
                    var names = JsonUtility.FromJson<JSONMatchNameConverter>(_nameResponse.ScriptData.JSON);
                    //Clear any option that might exist but shouldn't (unlikely)
                    m_HostSelectionDropDown.GetComponent<Dropdown>().options.Clear();
                    //Add Match names as an option to host
                    m_HostSelectionDropDown.GetComponent<Dropdown>().AddOptions(names.m_Names);
                    m_StartHostingButton.GetComponent<Button>().interactable = true;
                });
        }

        /// <summary>
        /// Get all of the match Short Codes for this project (these blueprints for matches are created here https://portal2.gamesparks.net/games/G376367AmCEW/config/multiplayer/matches,
        /// thus these short codes don't change during the game and therefore this function can be called from start.
        /// </summary>
        void GetMatchShortCodes()
        {
            m_MatchShortCodes = new List<string>();
            new LogEventRequest()
                .SetEventKey("GetMatchesShortCode")
                .Send((_shortCodeResponse) =>
                {
                    //Extract short code from the configuration data we got
                    GSData shortCodeScriptData = _shortCodeResponse.ScriptData;
                    string matchConfigs = shortCodeScriptData.JSON.ToString();
                    var matchShortCodes = Regex.Matches(matchConfigs, "shortCode\":\"[^\"]*", RegexOptions.Multiline);
                    foreach (var shortCode in matchShortCodes)
                    {
                        string actualShortCode = Regex.Split(shortCode.ToString(), "shortCode\":\"")[1].ToString();
                        if (actualShortCode != "QS") //Don't add QS as it is an auto connect
                        {
                            m_HostButton.GetComponent<Button>().interactable = true;
                            m_FindButton.GetComponent<Button>().interactable = true;
                            m_MatchShortCodes.Add(actualShortCode);
                        }
                        
                    }
                });
        }

        /// <summary>
        /// Destroy the match buttons and reset the original positioning of the match buttons so the 
        /// refreshed list contains up to date matches, placed correctly
        /// </summary>
        void ResetRoomView()
        {
            foreach (GameObject match in GameObject.FindGameObjectsWithTag("MatchInfo"))
            {
                Destroy(match);
            }
            m_OldMatchButtonPosition = m_MatchButtonPrefab.transform.localPosition;
        }

        /// <summary> Disconnect the user when they quit. Only used when quitting on the lobby scene.</summary>
        private void OnApplicationQuit()
        {
            new LogEventRequest().SetEventKey("UpdateOnlineStatus").SetEventAttribute("DisconnectFlag", "false").Send((_disconnectResponse) => { });

            //Disconnect
            if (m_MatchShortCodes != null)
            {
                foreach (string match in m_MatchShortCodes)
                {
                    if (match == "QS")
                    {
                        Disconnect(match, m_cleanRoomName, true);
                    }
                    else
                    {
                        Disconnect(match, "", true);
                    }
                }
            }
        }
    }

}
