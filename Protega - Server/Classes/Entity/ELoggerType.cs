using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Protega___Server.Classes.Entity
{
    [Serializable]
    public class ELoggerType
    {
        #region Declaration of values in the class
        /// <summary>
        /// Unique identifier of the LoggerType
        /// </summary>
        private int? _ID = null;

        /// <summary>
        /// Name of the LoggerType
        /// </summary>
        private string _Name;
        #endregion

        #region Constructor
        public ELoggerType()
        { }
        #endregion

        #region Accessors functions
        /// <summary>
        /// Unique identifier of the LoggerType
        /// </summary>
        public int? ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        /// <summary>
        /// Name of the LoggerType
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        #endregion
    }
    /// <summary>
    /// Description :
    /// Define a list of ELoggerTypes and provide functions to interact with
    /// </summary>
    [Serializable]
    public class ECollectionLoggerType : ArrayList
    {
        public List<ELoggerType> ToList()
        {
            List<ELoggerType> ListData = new List<ELoggerType>();
            ListData.AddRange(this.OfType<ELoggerType>());

            return ListData;
        }

    }
}
