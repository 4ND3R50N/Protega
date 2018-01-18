using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protega___Server.Classes.Entity
{
    [Serializable]
    public class EApplication
    {
        #region Declaration of values in the class
        private int _ID;
        private string _Name;
        private string _Hash;
        private string _Description;

        #endregion

        #region Constructor
        public EApplication()
        {}
        #endregion

        #region Accessors functions
        /// <summary>
        /// Unique identifier of the player
        /// </summary>
        public int ID
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
        public string Hash
        {
            get { return _Hash; }
            set { _Hash = value; }
        }

        /// <summary>
        /// Language of the player
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        #endregion
    }
}
