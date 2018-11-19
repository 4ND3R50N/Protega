using System;
using System.Data.SqlClient;
using Protega.ApplicationAdapter.Classes.Database.Entity;
using System.Data;
using System.Collections.Generic;

namespace Protega.ApplicationAdapter.Classes.Database.Data
{
    static class DPlayer
    {

        #region Static private method

        #region Method ReadData

        /// <summary>
        /// Fill EPlayer object by reading SqlDataReader returned by stored procedure
        /// </summary>
        /// <param name="oReader"></param>
        /// <returns></returns>
        static private EPlayer ReadData(SqlDataReader oReader)
        {
            try
            {
                EPlayer oData = new EPlayer();

                //Player
                oData.ID = oReader.GetString(oReader.GetOrdinal("CharacterIdx"));

                if (!oReader.IsDBNull(oReader.GetOrdinal("Name")))
                    oData.Name = oReader.GetString(oReader.GetOrdinal("Name"));
                if (!oReader.IsDBNull(oReader.GetOrdinal("LastIP")))
                {
                    System.Net.IPAddress IPtemp;
                    if (System.Net.IPAddress.TryParse(oReader.GetString(oReader.GetOrdinal("LastIP")), out IPtemp))
                        oData.IP = IPtemp;
                    else
                        return null;
                }
                if (!oReader.IsDBNull(oReader.GetOrdinal("isOnline")))
                    oData.isOnline = oReader.GetString(oReader.GetOrdinal("isOnline")) == "1";

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

        #region RegisterSQLParameter method
        /// <summary>
        /// Fill SqlParameter by reading a EPlayer object to execute stored procedure
        /// </summary>
        /// <param name="p_oData"></param>
        /// <returns></returns>
        static private SqlParameter[] RegisterSqlParameter(EPlayer p_oData)
        {
            SqlParameter[] arParams = new SqlParameter[5];

            arParams[0] = new SqlParameter("@ComputerID", SqlDbType.NVarChar, 50);
            arParams[1] = new SqlParameter("@IP", SqlDbType.NVarChar, 50);
            arParams[2] = new SqlParameter("@Language", SqlDbType.NVarChar, 50);
            arParams[3] = new SqlParameter("@OperatingSystem", SqlDbType.NVarChar, 50);
            arParams[4] = new SqlParameter("@ApplicationHash", SqlDbType.NVarChar, 50);

            arParams[0].Value = p_oData.ID;
            arParams[1].Value = p_oData.IP;

            return arParams;
        }
        #endregion

        #endregion

        #region Static public method

        #region Method select

        public static List<EPlayer> GetOnlineList()
        {
            SqlDataReader oReader = null;
            try
            {
                //Initialize the return object
                List<EPlayer> oCollData = new List<EPlayer>();

                //Call the request
                oReader = CCstDatabase.DatabaseEngine.ExecuteReader(CommandType.Text, CCstDatabase.OnlinePlayers_GetList, new SqlParameter[0]);

                //If there is a result (not null)
                if (oReader != null)
                {
                    while (oReader.Read())
                    {
                        EPlayer oData = new EPlayer();
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
