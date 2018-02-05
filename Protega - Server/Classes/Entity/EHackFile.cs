using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Entity
{
    [Serializable]
    class EHackFile
    {
        #region Declaration of values in the class
        private EPlayer _User;
        private int _CaseID;
        private string _Content;
        private int _ApplicationID;

        #endregion

        #region Constructor
        public EHackFile()
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
        /// Unique identifier of the case
        /// </summary>
        public int CaseID
        {
            get { return _CaseID; }
            set { _CaseID = value; }
        }

        /// <summary>
        /// Content of the detection
        /// </summary>
        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
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
