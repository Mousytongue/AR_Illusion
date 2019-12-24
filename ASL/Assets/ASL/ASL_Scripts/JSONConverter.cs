using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASL
{
    /// <summary>
    /// Used to convert the name of a match from the GameSparks Server into a readable format to help display information to the player
    /// </summary>
    [Serializable]
    public class JSONMatchNameConverter
    {
        /// <summary>
        /// The names of the different match types from the GameSparks server
        /// </summary>
        public List<string> m_Names;
    }

    /// <summary>
    /// Used to convert match found information from the GameSparks Servers into a readable format to display to the player
    /// </summary>
    [Serializable]
    public class JSONFoundMatchInfoConverter
    {
        /// <summary>
        /// The max player count for this match
        /// </summary>
        public int m_MaxPlayerCount;
    }
}
