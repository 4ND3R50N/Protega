using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega.ApplicationAdapter.Classes.Database.Entity
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
        private IPAddress _IP;
        private bool? _isOnline;
        #endregion

        #region Constructor
        public EPlayer()
        { }
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
        public IPAddress IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        /// <summary>
        /// Ban status of the player. 0 = unpunished, 1 = banned
        /// </summary>
        public bool? isOnline
        {
            get { return _isOnline; }
            set { _isOnline = value; }
        }
        #endregion
    }
}
