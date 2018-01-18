using System;
using System.Data.SqlClient;
using Protega___Server.Classes.Entity;
using System.Data;

namespace Protega___Server.Classes.Data
{
    public class DApplication
    {
        #region Static private method

        #region RegisterSQLParameter method
        /// <summary>
        /// Fill SqlParameter by reading a EPlayer object to execute stored procedure
        /// </summary>
        /// <param name="p_oData"></param>
        /// <returns></returns>
        static private SqlParameter[] RegisterSqlParameter(EApplication p_oData)
        {
            SqlParameter[] arParams = new SqlParameter[1];

            arParams[0] = new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 50);

            arParams[0].Value = p_oData.Name;

            return arParams;
        }
        #endregion

        #region ReadData
        static private EApplication ReadData(SqlDataReader oReader)
        {
            try
            {
                EApplication oData = new EApplication();

                //EApplication
                oData.ID = oReader.GetInt32(oReader.GetOrdinal("ApplicationID"));

                if (!oReader.IsDBNull(oReader.GetOrdinal("Name")))
                    oData.Name = oReader.GetString(oReader.GetOrdinal("Name"));
                if (!oReader.IsDBNull(oReader.GetOrdinal("Hash")))
                    oData.Hash = oReader.GetString(oReader.GetOrdinal("Hash"));
                if (!oReader.IsDBNull(oReader.GetOrdinal("Description")))
                    oData.Description = oReader.GetString(oReader.GetOrdinal("Description"));

                return oData;
            }
            catch (SqlException e)
            {
                throw e;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }
        #endregion

        #endregion

        #region Static public method

        public static EApplication GetByName(EApplication _pdata, DBEngine DatabaseEngine)
        {
            SqlDataReader oReader = null;
            try
            {
                //Fill the request's parameters
                SqlParameter[] p_sqlParams = RegisterSqlParameter(_pdata);

                //Call the request
                oReader = DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_Application_GetByName, p_sqlParams);

                //If there is a result (not null)
                if (oReader != null)
                {
                    while (oReader.Read())
                    {
                        //Read the data and convert the SqlDataReader in the waiting object
                        return ReadData(oReader);
                    }
                }
                return null;
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
