using System;
using System.Data.SqlClient;
using Protega___Server.Classes.Entity;
using System.Data;

namespace Protega___Server.Classes.Data
{
    class DHackVirtual
    {
        #region Static private method

        #region RegisterSQLParameter method
        /// <summary>
        /// Fill SqlParameter by reading a EPlayer object to execute stored procedure
        /// </summary>
        /// <param name="p_oData"></param>
        /// <returns></returns>
        static private SqlParameter[] RegisterSqlParameter(EHackVirtual p_oData)
        {
            SqlParameter[] arParams = new SqlParameter[6];

            arParams[0] = new SqlParameter("@ApplicationID", SqlDbType.Int);
            arParams[1] = new SqlParameter("@ComputerID", SqlDbType.NVarChar, 50);
            arParams[2] = new SqlParameter("@BaseAddress", SqlDbType.NVarChar, 50);
            arParams[3] = new SqlParameter("@Type", SqlDbType.NVarChar, 5);
            arParams[4] = new SqlParameter("@DetectedValue", SqlDbType.NVarChar, 50);
            arParams[5] = new SqlParameter("@AllowedValues", SqlDbType.NVarChar, 50);

            arParams[0].Value = p_oData.ApplicationID;
            arParams[1].Value = p_oData.User.ID;
            arParams[2].Value = p_oData.BaseAddress;
            arParams[3].Value = p_oData.Offset;
            arParams[4].Value = p_oData.DetectedValue;
            arParams[5].Value = p_oData.DefaultValue;

            return arParams;
        }
        #endregion

        #endregion

        #region Static public method

        public static bool Insert(EHackVirtual _pdata)
        {
            SqlDataReader oReader = null;
            try
            {
                //Initialize the return object
                EHackVirtual oData = new EHackVirtual();

                //Fill the request's parameters
                SqlParameter[] p_sqlParams = RegisterSqlParameter(_pdata);

                //Call the request
                oReader = CCstData.GetInstance(_pdata.ApplicationID).DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_HackDetection_Insert_VirtualMemory, p_sqlParams);

                //If there is a result (not null)
                if (oReader != null)
                {
                    while (oReader.Read())
                    {
                        //Read the data and convert the SqlDataReader in the waiting object
                        object Test = oReader.GetOrdinal("ID");
                        return oReader.GetString(oReader.GetOrdinal("ID")) == "0";
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (oReader != null && !oReader.IsClosed) oReader.Close();
            }

        }
        #endregion


    }
}
