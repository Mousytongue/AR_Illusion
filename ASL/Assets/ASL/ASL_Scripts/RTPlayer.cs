
namespace ASL
{
    /// <summary>
    /// Contains information about a player connected to GameSparks
    /// </summary>
    public class RTPlayer
    {
        /// <summary>
        /// Constructor for RTPlayer. 
        /// </summary>
        /// <param name="_displayName">The user's display name</param>
        /// <param name="_id">The user's GameSparks Id</param>
        /// <param name="_peerId">The user's peer id, which is usually based upon order in joining the game</param>
        public RTPlayer(string _displayName, string _id, int _peerId)
        {
            m_DisplayName = _displayName;
            m_Id = _id;
            m_PeerId = _peerId;
        }

        /// <summary>The user's display name</summary>
        public string m_DisplayName;
        /// <summary>The user's id</summary>
        public string m_Id;
        /// <summary>The user's peer id</summary>
        public int m_PeerId;

    }
}