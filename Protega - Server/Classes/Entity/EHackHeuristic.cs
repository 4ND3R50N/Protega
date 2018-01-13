using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Entity
{
    [Serializable]
    class EHackHeuristic
    {
        #region Declaration of values in the class
        private EPlayer _User;
        private string _ProcessName;
        private string _WindowName;
        private string _ClassName;
        private string _MD5Value;
        
        #endregion

        #region Constructor
        public EHackHeuristic()
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
        public string ProcessName
        {
            get { return _ProcessName; }
            set { _ProcessName = value; }
        }

        /// <summary>
        /// Name of the player
        /// </summary>
        public string WindowName
        {
            get { return _WindowName; }
            set { _WindowName = value; }
        }

        /// <summary>
        /// Latest IP of the player
        /// </summary>
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; }
        }

        /// <summary>
        /// Language of the player
        /// </summary>
        public string MD5Value
        {
            get { return _MD5Value; }
            set { _MD5Value = value; }
        }
        #endregion
    }
}
