using GameSparks.Api.Messages;
using System.Collections.Generic;

namespace ASL
{
    /// <summary>
    /// Holds a RealTime session info to be used when all players are ready
    /// </summary>
    public class RTSessionInfo
    {
        /// <summary>The host URL of this session</summary>
        private string m_HostURL;
        /// <summary>The access token of this session</summary>
        private string m_AccessToken;
        /// <summary>The port id of this session</summary>
        private int m_PortID;
        /// <summary>The match id of this session</summary>
        private string m_MatchID;
        /// <summary>The list of players for this session</summary>
        private List<RTPlayer> m_PlayerList = new List<RTPlayer>();
        /// <summary>The list of player ids for this session</summary>
        private List<string> m_PlayerListIds = new List<string>();

        /// <summary>Get the GameSparks Host URL</summary>
        /// <returns>The Host URL for connection</returns>
        public string GetHostURL() { return this.m_HostURL; }
        /// <summary>Gets the access token for this real time session</summary>
        /// <returns>Returns the access token for this real time session</returns>
        public string GetAccessToken() { return this.m_AccessToken; }
        /// <summary>Gets the port id for this session</summary>
        /// <returns>Returns the GameSparks port id for this session</returns>
        public int GetPortID() { return this.m_PortID; }
        /// <summary>Gets the GameSparks match ID for this session</summary>
        /// <returns>Returns the match ID for this session</returns>
        public string GetMatchID() { return this.m_MatchID; }
        /// <summary>Gets all the players in this session</summary>
        /// <returns>A list of the players in this session</returns>
        public List<RTPlayer> GetPlayerList() { return m_PlayerList; }

        /// <summary>
        /// Updates the real time session information based upon the match found message generated when a match is found
        /// </summary>
        /// <param name="_message">information about the match found</param>
        public RTSessionInfo(MatchFoundMessage _message)
        {
            m_PortID = (int)_message.Port;
            m_HostURL = _message.Host;
            m_AccessToken = _message.AccessToken;
            m_MatchID = _message.MatchId;

            //Loop through each participant and get their peerId and display name:
            foreach (MatchFoundMessage._Participant p in _message.Participants)
            {
                m_PlayerList.Add(new RTPlayer(p.DisplayName, p.Id, (int)p.PeerId));
            }
        }



    }
}