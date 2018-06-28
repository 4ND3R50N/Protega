using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net;

namespace Protega___Server.Classes.Entity
{
    [Serializable]
    public class EPlayer
    {
        #region Declaration of values in the class
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        private string _ID;
        private string _Name;
        private string _IP;
        private string _Language;
        private string _OperatingSystem;
        private EApplication _Application;

        private IPAddress _GameIP;

        //Game Account details
        private string _GameAccID;
        private string _GameAccName;
        private int _CheckCounter;

        /// <summary>
        /// Ban status of the player. 0 = unpunished, 1 = banned
        /// </summary>
        private bool? _isBanned;
        #endregion

        #region Constructor
        public EPlayer()
        { _Application = new EApplication(); }
        #endregion

        #region Accessors functions
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        /// <summary>
        /// Name of the player
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Latest IP of the player
        /// </summary>
        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        /// <summary>
        /// Language of the player
        /// </summary>
        public string Language
        {
            get { return _Language; }
            set { _Language = value; }
        }

        /// <summary>
        /// OperatingSystem of the player
        /// </summary>
        public string OperatingSystem
        {
            get { return _OperatingSystem; }
            set { _OperatingSystem = value; }
        }

        /// <summary>
        /// Connected application of the player
        /// </summary>
        public EApplication Application
        {
            get { return _Application; }
            set { _Application = value; }
        }

        /// <summary>
        /// Ban status of the player. 0 = unpunished, 1 = banned
        /// </summary>
        public bool? isBanned
        {
            get { return _isBanned; }
            set { _isBanned = value; }
        }

        /// <summary>
        /// IP Address of the player that is received by the game server
        /// </summary>
        public IPAddress GameIP
        {
            get { return _GameIP; }
            set { _GameIP = value; }
        }
        #endregion
    }

    /// <summary>
    /// Description :
    /// Define a list of Players and provide functions to interact with
    /// </summary>
    //[Serializable]
    //public class ECollectionPlayer : ArrayList
    //{
    //    public List<EPlayer> ToList()
    //    {
    //        List<EPlayer> ListData = new List<EPlayer>();
    //        ListData.AddRange(this.OfType<EPlayer>());

    //        return ListData;
    //    }

    //}
}
