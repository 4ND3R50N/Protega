using System;
using System.Data.SqlClient;
using Protega___Server.Classes.Entity;
using System.Data;

namespace Protega___Server.Classes.Data
{
    public static class DLoggerType
    {
        #region Static private method

        #region Method ReadData

        /// <summary>
        /// Fill ELoggerType object by reading SqlDataReader returned by stored procedure
        /// </summary>
        /// <param name="oReader"></param>
        /// <returns></returns>
        static private ELoggerType ReadData(SqlDataReader oReader)
        {
            try
            {
                ELoggerType oData = new ELoggerType();

                //ELoggerType
                oData.ID = oReader.GetOrdinal("ID");

                if (!oReader.IsDBNull(oReader.GetOrdinal("Name")))
                    oData.Name = oReader.GetString(oReader.GetOrdinal("Name"));

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
        static private SqlParameter[] RegisterSqlParameter(EPlayer p_oData)
        {
            SqlParameter[] arParams = new SqlParameter[3];

            arParams[0] = new SqlParameter("@idBench", SqlDbType.Int);
            arParams[1] = new SqlParameter("@Name", SqlDbType.NVarChar, 255);
            arParams[2] = new SqlParameter("@idEnduranceRoom", SqlDbType.Int);

            arParams[0].Value = p_oData.ID;
            arParams[1].Value = p_oData.Name;
            arParams[2].Value = p_oData.isBanned;


            return arParams;
        }
        #endregion

        #endregion

        #region Static public method

        #region Method select

        public static ECollectionLoggerType GetList()
        {
            SqlDataReader oReader = null;
            try
            {
                //Initialize the return object
                ECollectionLoggerType oCollData = new ECollectionLoggerType();

                //Call the request
                using (DBEngine DBInstance = CCstData.GetInstance("").DatabaseEngine)
                {
                    oReader = DBInstance.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_LoggerType_GetList);

                }

                

                //If there is a result (not null)
                if (oReader != null)
                {
                    while (oReader.Read())
                    {
                        ELoggerType oData = new ELoggerType();
                        //Read the data and convert the SqlDataReader in the waiting object
                        oData = ReadData(oReader);
                        //Add the data to the return list
                        oCollData.Add(oData);
                    }
                }
                return oCollData;
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
