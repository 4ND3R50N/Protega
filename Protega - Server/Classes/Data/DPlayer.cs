using System;
using System.Data.SqlClient;
using Protega___Server.Classes.Entity;
using System.Data;

namespace Protega___Server.Classes.Data
{
    public static class DPlayer
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
                oData.ID = oReader.GetOrdinal("ID");

                if (!oReader.IsDBNull(oReader.GetOrdinal("Name")))
                    oData.Name = oReader.GetString(oReader.GetOrdinal("Name"));
                if (!oReader.IsDBNull(oReader.GetOrdinal("IP")))
                    oData.IP = oReader.GetString(oReader.GetOrdinal("IP"));
                if (!oReader.IsDBNull(oReader.GetOrdinal("BanStatus")))
                    oData.isBanned = oReader.GetInt32(oReader.GetOrdinal("BanStatus")) != 0;

                return oData;
            }
            catch(SqlException e)
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

        public static ECollectionPlayer GetList(EPlayer _pPlayer)
        {
            SqlDataReader oReader = null;
            try
            {
                //Initialize the return object
                ECollectionPlayer oCollData = new ECollectionPlayer();

                //Fill the request's parameters
                SqlParameter[] p_sqlParams = RegisterSqlParameter(_pPlayer);

                //Call the request
                oReader = CCstDatabase.DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_Player_GetByName, p_sqlParams);

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
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                if (oReader != null && !oReader.IsClosed) oReader.Close();
            }

        }

        public static EPlayer GetByName(string Name)
        {
            SqlDataReader dataReader = null;

            try
            {
                EPlayer odata = new EPlayer();

                SqlParameter[] Param = new SqlParameter[1];
                Param[0] = new SqlParameter("@Name", SqlDbType.VarChar);
                Param[0].Value = Name;

                dataReader = CCstDatabase.DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_Player_GetByName, Param);
                
                if(dataReader != null)
                {
                    while(dataReader.Read())
                    {
                        odata = ReadData(dataReader);
                    }
                }
                return odata;
            }
            catch (SqlException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed) dataReader.Close();
            }
        }
        #endregion

        #endregion


    }
}
