/**
 * WhiteCode
 *
 * A selfmade subclass with an global database controller
 *
 * @author		Anderson from WhiteCode
 * @copyright		Copyright (c) 2016
 * @link		http://white-code.org
 * @since		Version 1.0
 */
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;


namespace Protega___Server.Classes
{
    class DBMssqlDataManager : DBEngine
    {

        private SqlConnection sqlConnection = null;
        public DBMssqlDataManager(string host_ip, string sql_user, string sql_pass, short sql_port, string sql_db_default)
             : base(host_ip, sql_user, sql_pass, sql_port, sql_db_default)
        {
            sqlConnection = new SqlConnection("Server=" + host_ip + ";Database=" + sql_db_default + ";User Id=" + sql_user + ";Password=" + sql_pass + ";MultipleActiveResultSets=True;");
        }

        #region Private functions
        private void pPrepareCommand(SqlCommand sqlCommand, SqlTransaction sqlTransaction, CommandType p_cmdType, string cmdText, SqlParameter[] cmdParameters)
        {
            if (sqlCommand == null) return;
            if (cmdText == null || cmdText.Length == 0) return;

            // Open connection if it isnt
            //if (sqlConnection.State != ConnectionState.Open)
            //{
            //    //sqlConnection.Open();
            //}

            // Connect the connection to the command
            sqlCommand.Connection = sqlConnection;

            // Connect the name of the procedure to the command
            sqlCommand.CommandText = cmdText;

            // Affectation de la transaction si il y en une de définit
            if (sqlTransaction != null)
            {
                if (sqlTransaction.Connection == null) throw new ArgumentException("DB Error 1: The transaction was rolled back or commited. An open transaction is needed", "transaction");
                sqlCommand.Transaction = sqlTransaction;
            }

            // Connect the CommandType to the Command
            sqlCommand.CommandType = p_cmdType;
            sqlCommand.CommandTimeout = 120;

            // Attach the parameters if existing
            if (cmdParameters != null)
            {
                pSAttacherParametres(sqlCommand, cmdParameters);
            }
            return;
        }

        //private static void pSAssignerParametres(SqlParameter[] p_aopSqlParameters, object[] p_aooParamValues)
        //{
        //    if ((p_aopSqlParameters == null) || (p_aooParamValues == null))
        //    {
        //        // Return if no parameters were given
        //        return;
        //    }

        //    // The amount of given parameters doesn't fit the amount of the needed ones
        //    if (p_aopSqlParameters.Length != p_aooParamValues.Length)
        //    {
        //        throw new ArgumentException("The amount of given parameters doesnt fit to the stored procedure!");
        //    }

        //    // The parameters are compared to the ones of the procedure
        //    for (int i = 0, j = p_aopSqlParameters.Length; i < j; i++)
        //    {
        //        if (p_aooParamValues[i] is IDbDataParameter)
        //        {
        //            IDbDataParameter paramInstance = (IDbDataParameter)p_aooParamValues[i];
        //            if (paramInstance.Value == null)
        //            {
        //                p_aopSqlParameters[i].Value = DBNull.Value;
        //            }
        //            else
        //            {
        //                p_aopSqlParameters[i].Value = paramInstance.Value;
        //            }
        //        }
        //        else if (p_aooParamValues[i] == null)
        //        {
        //            p_aopSqlParameters[i].Value = DBNull.Value;
        //        }
        //        else
        //        {
        //            p_aopSqlParameters[i].Value = p_aooParamValues[i];
        //        }
        //    }
        //}

        private static void pSAttacherParametres(SqlCommand sqlCommand, SqlParameter[] sqlParameters)
        {
            if (sqlCommand == null) throw new ArgumentNullException("sqlCommand","DB Error 2 SqlCommand was null");
            if (sqlParameters != null)
            {
                foreach (SqlParameter p in sqlParameters)
                {
                    if (p != null)
                    {
                        // Verify if the parameters fit to the according parameter of the procedure
                        if ((p.Direction == ParameterDirection.InputOutput ||
                            p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        sqlCommand.Parameters.Add(p);
                    }
                }
            }
        }

        private SqlDataReader pExecuteReader(CommandType cmdType, string ProcedureName, SqlParameter[] Parameters)
        {
            SqlCommand sqlCommand = new SqlCommand();
            try
            {
                //Put parameters to the procedure
                pPrepareCommand(sqlCommand, null, cmdType, ProcedureName, Parameters);
                
                if (sqlConnection.State != ConnectionState.Open)
                    sqlConnection.Open();

                SqlDataReader dataReader;
                dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);

                bool CanClear = true;
                foreach (SqlParameter Param in sqlCommand.Parameters)
                {
                    if (Param.Direction != ParameterDirection.Input)
                        CanClear = false;
                }

                if(CanClear)
                {
                    sqlCommand.Parameters.Clear();
                }

                    return dataReader;

            }
            catch (Exception e)
            {
                sqlConnection.Close();
                throw e;
            }
            finally
            {
            }
        }

        #endregion

        #region Public functions

        public override int ExecuteNonQuery(CommandType cmdType, string ProcedureName, params SqlParameter[] Parameters)
        {
            if (sqlConnection == null) throw new ArgumentNullException("sqlConnection", "DB Error 3: Object was null");

            SqlCommand sqlCommand = new SqlCommand();
            pPrepareCommand(sqlCommand, null, cmdType, ProcedureName, Parameters);

            try
            {
                if (sqlConnection.State != ConnectionState.Open)
                    sqlConnection.Open();

                return sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        /// <summary>
        /// Execute a query and get results
        /// </summary>
        /// <param name="sqlConnection">A valid SQL Connection</param>
        /// <param name="cmdType">Whether Stored Procedure</param>
        /// <param name="ProcedureName">Name of the procedure</param>
        /// <param name="Parameters">Parameters, null if none</param>
        /// <returns></returns>
        public override SqlDataReader ExecuteReader(CommandType cmdType, string ProcedureName, params SqlParameter[] Parameters)
        {
            if (sqlConnection == null) return null;

            //if Parameters were given, assign them
            if(Parameters != null && Parameters.Length>0)
            {
                return pExecuteReader(cmdType, ProcedureName, Parameters);
            }
            else
            {
                return pExecuteReader(cmdType, ProcedureName, (SqlParameter[])null);
            }
        }

        #endregion
        //Queries

        protected override MySql.Data.MySqlClient.MySqlDataReader executeQuery(MySql.Data.MySqlClient.MySqlConnection mysqlConnection, string query)
        {
            throw new NotImplementedException();
        }
        

        public override bool testDBConnection()
        {
            try
            {
                sqlConnection.Open();
            }
            catch (Exception e)
            {
                //dataHandler.writeInMainlog("MSSQL Connect failed. [testDBConnection]", true);
                return false;
            }
            finally
            {
                sqlConnection.Close();
            }
            
            return true;
        }
        
    }
}
