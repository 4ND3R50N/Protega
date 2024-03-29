﻿using System;
using System.Data.SqlClient;
using Protega___Server.Classes.Entity;
using System.Data;

namespace Protega___Server.Classes.Data
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
                oData.ID = oReader.GetString(oReader.GetOrdinal("HardwareID"));
                oData.Application.ID = oReader.GetInt32(oReader.GetOrdinal("ApplicationID"));

                if (!oReader.IsDBNull(oReader.GetOrdinal("LatestIP")))
                    oData.IP = oReader.GetString(oReader.GetOrdinal("LatestIP"));
                if (!oReader.IsDBNull(oReader.GetOrdinal("isBanned")))
                    oData.isBanned = oReader.GetString(oReader.GetOrdinal("isBanned"))=="1";

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
            SqlParameter[] arParams = new SqlParameter[5];

            arParams[0] = new SqlParameter("@ComputerID", SqlDbType.NVarChar,50);
            arParams[1] = new SqlParameter("@IP", SqlDbType.NVarChar, 50);
            arParams[2] = new SqlParameter("@Language", SqlDbType.NVarChar, 50);
            arParams[3] = new SqlParameter("@OperatingSystem", SqlDbType.NVarChar, 50);
            arParams[4] = new SqlParameter("@ApplicationHash", SqlDbType.NVarChar, 50);

            arParams[0].Value = p_oData.ID;
            arParams[1].Value = p_oData.IP;
            arParams[2].Value = p_oData.Language;
            arParams[3].Value = p_oData.OperatingSystem;
            arParams[4].Value = p_oData.Application.Hash;

            return arParams;
        }
        #endregion

        #endregion

        #region Static public method

        #region Method select

        /*public static ECollectionPlayer GetList(EPlayer _pPlayer)
        {
            SqlDataReader oReader = null;
            try
            {
                //Initialize the return object
                ECollectionPlayer oCollData = new ECollectionPlayer();

                //Fill the request's parameters
                SqlParameter[] p_sqlParams = RegisterSqlParameter(_pPlayer);

                //Call the request
                oReader = CCstDatabase.DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_User_GetByName, p_sqlParams);

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

        }*/

        /*public static EPlayer GetByName(string Name)
        {
            SqlDataReader dataReader = null;

            try
            {
                EPlayer odata = new EPlayer();

                SqlParameter[] Param = new SqlParameter[1];
                Param[0] = new SqlParameter("@Name", SqlDbType.VarChar);
                Param[0].Value = Name;

                dataReader = CCstDatabase.DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_User_GetByName, Param);
                
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
        }*/

        public static EPlayer Authenticate(EPlayer _User)
        {
            SqlDataReader oReader = null;
            try
            {

                //Fill the request's parameters
                SqlParameter[] p_sqlParams = RegisterSqlParameter(_User);
                
                //Call the request
                oReader = CCstData.GetInstance(_User.Application.Hash).DatabaseEngine.ExecuteReader(CommandType.StoredProcedure, CCstDatabase.SP_User_Authenticate, p_sqlParams);

                //If there is a result (not null)
                if (oReader != null)
                {
                    EPlayer oData = new EPlayer();
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
