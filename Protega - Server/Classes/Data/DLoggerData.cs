using System;
using System.Data.SqlClient;
using Protega___Server.Classes.Entity;
using System.Data;

namespace Protega___Server.Classes.Data
{
    public static class DLoggerData
    {
        #region Static private method

        #region Method ReadData

        /// <summary>
        /// Fill ELoggerData object by reading SqlDataReader returned by stored procedure
        /// </summary>
        /// <param name="oReader"></param>
        /// <returns></returns>
        static private ELoggerData ReadData(SqlDataReader oReader)
        {
            try
            {
                ELoggerData oData = new ELoggerData();

                //ELoggerData
                oData.ID = oReader.GetString( oReader.GetOrdinal("ID"));                
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

        #region RegisterSQLParameter method - Not needed
        /// <summary>
        /// Fill SqlParameter by reading a EPlayer object to execute stored procedure
        /// </summary>
        /// <param name="p_oData"></param>
        /// <returns></returns>
        static private SqlParameter[] RegisterSqlParameter(ELoggerData p_oData)
        {
            SqlParameter[] arParams = new SqlParameter[5];

            arParams[0] = new SqlParameter("@ApplicationID", SqlDbType.Int);
            arParams[1] = new SqlParameter("@Message", SqlDbType.NVarChar, 500);
            arParams[2] = new SqlParameter("@Importance", SqlDbType.Int);
            arParams[3] = new SqlParameter("@LoggerCategory", SqlDbType.NVarChar, 100);
            arParams[4] = new SqlParameter("@LoggerType", SqlDbType.NVarChar, 50);

            arParams[0].Value = p_oData.ApplicationID;
            arParams[1].Value = p_oData.Message;
            arParams[2].Value = p_oData.Importance;
            arParams[3].Value = p_oData.Category;
            arParams[4].Value = p_oData.Type;


            return arParams;
        }
        #endregion

        #endregion

        #region Static public method

        #region Method select

        public static ELoggerData Insert(ELoggerData iData)
        {
            SqlDataReader oReader = null;
            try
            {

                //Fill the request's parameters
                SqlParameter[] p_sqlParams = RegisterSqlParameter(iData);

                //Call the request
                oReader = CCstData.GetInstance(iData.ApplicationID).DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_LoggerData_Insert, p_sqlParams);

                //If there is a result (not null)
                if (oReader != null)
                {
                    ELoggerData oData = new ELoggerData();
                    while (oReader.Read())
                    {
                        oData = ReadData(oReader);
                    }

                    if (oData.ID == "-1")
                        //If an error occurs, ID -1 is given
                        return null;
                    return oData;

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

        #endregion


    }
}
