using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Entity
{
    [Serializable]
    class EHackVirtual
    {
        #region Declaration of values in the class
        private EPlayer _User;
        private string _BaseAddress;
        private string _Offset;
        private string _DetectedValue;
        private string _DefaultValue;
        private int _ApplicationID;

        #endregion

        #region Constructor
        public EHackVirtual()
        { User = new EPlayer(); }
        #endregion

        #region Accessors functions
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        public EPlayer User
        {
            get { return _User; }
            set { _User = value; }
        }
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        public string BaseAddress
        {
            get { return _BaseAddress; }
            set { _BaseAddress = value; }
        }

        /// <summary>
        /// Name of the player
        /// </summary>
        public string Offset
        {
            get { return _Offset; }
            set { _Offset = value; }
        }

        /// <summary>
        /// Latest IP of the player
        /// </summary>
        public string DetectedValue
        {
            get { return _DetectedValue; }
            set { _DetectedValue = value; }
        }

        /// <summary>
        /// Language of the player
        /// </summary>
        public string DefaultValue
        {
            get { return _DefaultValue; }
            set { _DefaultValue = value; }
        }

        /// <summary>
        /// ApplicationName of the player
        /// </summary>
        public int ApplicationID
        {
            get { return _ApplicationID; }
            set { _ApplicationID = value; }
        }
        #endregion
    }
}
