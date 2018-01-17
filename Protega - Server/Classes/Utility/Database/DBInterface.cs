/**
 * WhiteCode
 *
 * A selfmade Interface controller who shares global functions to subclasses
 *
 * @author		Anderson from WhiteCode
 * @copyright		Copyright (c) 2016
 * @link		http://white-code.org
 * @since		Version 1.0
 */
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Collections.Generic;using System;
using System.Linq;
using System.Data;
using System.Collections;

namespace Protega___Server.Classes
{

    public abstract class DBEngine
    {

        protected string host_ip;
        protected string sql_user;
        protected string sql_pass;
        protected short sql_port;
        protected string sql_db_default;
        
        
        public DBEngine(string host_ip, string sql_user, string sql_pass, short sql_port, string sql_db_default)
        {
            this.host_ip = host_ip;
            this.sql_user = sql_user;
            this.sql_pass = sql_pass;
            this.sql_port = sql_port;
            this.sql_db_default = sql_db_default;
        }

        //Support      
        protected abstract MySqlDataReader executeQuery(MySqlConnection MysqlConnection, string query);
        //protected abstract SqlDataReader executeQuery(SqlConnection MssqlConnection, string query);
        public abstract bool testDBConnection();

        /// <summary>
        /// Check the Sql Parameters if it fits to the stored procedure
        /// </summary>
        /// <param name="SqlComm"></param>
        /// <param name="SqlParams"></param>
        //protected abstract void pAttachParameters(SqlCommand SqlComm, SqlParameter[] SqlParams);

        /// <summary>
        /// Open and assign connection and all data
        /// </summary>
        /// <param name="sqlCommand">SqlCommand to be assigned</param>
        /// <param name="sqlConnection">Valid SQL connection</param>
        /// <param name="sqlTransaction">Valid SQL transaction or null</param>
        /// <param name="cmdType">Command's type to execute (stored procedure, text, ...)</param>
        /// <param name="cmdText">Stored procedure's name or text of request</param>
        /// <param name="CmdParameters">Table of parameters Sql associates to the SqlCommand object or null if not existing</param>
        //protected abstract void pPrepareCommand(SqlCommand sqlCommand, SqlConnection sqlConnection, SqlTransaction sqlTransaction, CommandType cmdType, string cmdText, SqlParameter[] CmdParameters);

        /// <summary>
        /// Assign the parameters to a table of params
        /// </summary>
        /// <param name="SqlParameters">Table of Params</param>
        /// <param name="ParamValues">Table of object values</param>
        //protected abstract void pAssignParameters(SqlParameter[] SqlParameters, object[] ParamValues);
        
        

        #region Public methods
        /// <summary>
        /// Execute a SQL Command and return results
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="cmdText">Procedure's name</param>
        /// <param name="CmdParameters">Parameters</param>
        /// <returns></returns>
        public abstract SqlDataReader ExecuteReader(CommandType cmdType, string ProcedureName, params SqlParameter[] Parameters);

        /// <summary>
        /// Execute a SQL Command without returning results
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="cmdText">Procedure's name</param>
        /// <param name="CmdParameters">Parameters</param>
        /// <returns></returns>
        public abstract int ExecuteNonQuery(CommandType cmdType, string ProcedureName, params SqlParameter[] Parameters);
        #endregion
    }
    

}
