﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Protega___Server.Classes.Entity
{
    [Serializable]
    public class EPlayer
    {
        #region Declaration of values in the class
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        private int? _ID = null;
        private string _Name;
        private string _IP;

        /// <summary>
        /// Ban status of the player. 0 = unpunished, 1 = banned
        /// </summary>
        private bool? _isBanned;
        #endregion

        #region Constructor
        public EPlayer()
        { }
        #endregion

        #region Accessors functions
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        public int? ID
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
        /// Ban status of the player. 0 = unpunished, 1 = banned
        /// </summary>
        public bool? isBanned
        {
            get { return _isBanned; }
            set { _isBanned = value; }
        }
        #endregion
    }

    /// <summary>
    /// Description :
    /// Define a list of Players and provide functions to interact with
    /// </summary>
    [Serializable]
    public class ECollectionPlayer : ArrayList
    {
        public List<EPlayer> ToList()
        {
            List<EPlayer> ListData = new List<EPlayer>();
            ListData.AddRange(this.OfType<EPlayer>());

            return ListData;
        }

    }
}
