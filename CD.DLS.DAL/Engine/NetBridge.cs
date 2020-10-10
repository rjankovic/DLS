using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Engine
{
    public class NetBridge
    {
        string _connString = null;

        public string ConnString { get { return _connString; } }

        private SqlConnection _brokerConnection;

        public SqlConnection BrokerConnection { get { return _brokerConnection; } }

        public bool SuppressBrokerConnection { get; set; }


        public NetBridge(bool suppressBrokerConnection = false, bool setConnectionString = true)
        {
            if (setConnectionString)
            {
                if (ConfigManager.DeploymentMode == DeploymentModeEnum.OnPremises || ConfigManager.ApplicationClass == ApplicationClassEnum.Client || ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
                {                  
                        SetConnectionString(ConfigManager.CustomerDatabaseConnectionString);                             
               }
            }
            SuppressBrokerConnection = suppressBrokerConnection;
        }


        public event SqlInfoMessageEventHandler ConnectionInfoMessage;
        public void MessageHandler(object sender, SqlInfoMessageEventArgs e)
        {
            if (ConnectionInfoMessage != null)
            {
                ConnectionInfoMessage(sender, e);
            }
        }
        
        public void SetConnectionString(string connstring)
        {
            _connString = connstring;
            if (Configuration.ConfigManager.QueueMode == Configuration.QueueModeEnum.ServiceBroker && !SuppressBrokerConnection)
            {
                RefreshBrokerConnection();
            }
        }

        private void RefreshBrokerConnection()
        {
            // BROKER
            // just don't use the broker
            return;
            
            if (ConfigManager.QueueMode == QueueModeEnum.AzureTopic)
            {
                return;
            }
            if (string.IsNullOrEmpty(_connString))
            {              
                if (_brokerConnection != null)
                {
                    _brokerConnection.Close();
                    _brokerConnection.Dispose();
                    _brokerConnection = null;
                }
                return;
            }
            if (_brokerConnection != null)
            {
                SqlDependency.Stop(_brokerConnection.ConnectionString);
                _brokerConnection.Close();
                _brokerConnection.Dispose();
            }
            try
            {
                _brokerConnection = new SqlConnection(_connString);
                _brokerConnection.Open();
                SqlDependency.Start(_connString);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("CD Framework failed to connect to: \"{0}\", {1}", _connString, ex.Message));
            }
        }


        private string SetParams(SqlCommand cmd, Dictionary<string, object> parameters, bool explicitNames = false)
        {
            foreach (var p in parameters.Keys)
            {
                var pvalue = parameters[p];
                //if (typeof(IEnumerable).IsAssignableFrom(pvalue.GetType()))
                //{
                //    var type = pvalue.GetType().GetGenericArguments()[0];
                //    pvalue = MapToTable(pvalue, type);
                //}

                var pname = "@" + p;
                cmd.Parameters.AddWithValue(pname, pvalue);

                if (cmd.Parameters[pname].Value == null)
                {
                    cmd.Parameters[pname].Value = DBNull.Value;
                }

                if (cmd.Parameters[pname].Value == DBNull.Value)
                {
                    cmd.Parameters[pname].SqlDbType = SqlDbType.UniqueIdentifier;
                }
                if (cmd.Parameters[pname].Value is string || cmd.Parameters[pname].Value == DBNull.Value)
                {
                    cmd.Parameters[pname].SqlDbType = SqlDbType.NVarChar;
                    cmd.Parameters[pname].Size = 2000000000;
                }
                cmd.Parameters[pname].Direction = System.Data.ParameterDirection.InputOutput;

                if (pvalue is DataTable)
                {
                    cmd.Parameters[pname].SqlDbType = SqlDbType.Structured; // .TypeName = ((DataTable)(pvalue)).TableName;
                    cmd.Parameters[pname].TypeName = ((DataTable)(pvalue)).TableName;
                    cmd.Parameters[pname].Direction = System.Data.ParameterDirection.Input;
                }
            }
            return String.Join(", ", parameters.Keys.Select(
              x => (explicitNames ? "@" + x + " = " : "") + "@" + x));
        }

        private string SetOutputParams(SqlCommand cmd, Dictionary<string, SqlDbType> outputParams, bool explicitNames = false)
        {
            foreach (var p in outputParams.Keys)
            {
                var pname = "@" + p;
                cmd.Parameters.Add(pname, outputParams[p]).Direction = ParameterDirection.Output;
                var param = cmd.Parameters[pname];
                if (param.SqlDbType == SqlDbType.NVarChar || param.SqlDbType == SqlDbType.VarChar)
                {
                    param.Size = 4000;
                }
            }
            return String.Join(", ", outputParams.Keys.Select(
                x => (explicitNames ? "@" + x + " = " : "") + "@" + x + " OUTPUT"));
        }

        public object ExecuteScalarFunction(string name, Dictionary<string, object> parameters = null)
        {
            object res;

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "SELECT " + name + " (" + SetParams(cmd, parameters) + ")";
                cmd.CommandTimeout = 36000;
                conn.Open();
                res = cmd.ExecuteScalar();
            }
            return res;
        }

        public void ExecuteProcedure(string name, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "EXEC " + name + " " + SetParams(cmd, parameters, true);
                cmd.CommandTimeout = 36000;
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public async void ExecuteProcedureAsync(string name, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "EXEC " + name + " " + SetParams(cmd, parameters, true);
                cmd.CommandTimeout = 36000;
                conn.Open();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public Dictionary<string, object> ExecuteProcedureWithOutParams(string name, Dictionary<string, object> inputParameters, Dictionary<string, SqlDbType> outputParameters)
        {
            if (inputParameters == null)
            {
                inputParameters = new Dictionary<string, object>();
            }
            Dictionary<string, object> results = new Dictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "EXEC " + name + " " + SetParams(cmd, inputParameters, true);
                var outputParamStr = SetOutputParams(cmd, outputParameters, true);
                cmd.CommandText = cmd.CommandText + (inputParameters.Any() ? ", " : "") + outputParamStr;
                cmd.CommandTimeout = 36000;
                conn.Open();
                cmd.ExecuteNonQuery();
                foreach (var outParam in outputParameters.Keys)
                {
                    results.Add(outParam, cmd.Parameters["@" + outParam].Value);
                }
            }
            return results;
        }

        public object ExecuteProcedureScalar(string name, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            object result;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "EXEC " + name + " " + SetParams(cmd, parameters, true);
                cmd.CommandTimeout = 36000;
                conn.Open();
                result = cmd.ExecuteScalar();
            }
            return result;
        }

        public DataTable ExecuteProcedureTable(string name, Dictionary<string, object> parameters = null)
        {
            DataTable res = new DataTable();

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            object result;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = "EXEC " + name + " " + SetParams(cmd, parameters, true);
                cmd.CommandTimeout = 36000;
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.SelectCommand.CommandTimeout = 36000;
                da.Fill(res);
            }
            return res;
        }

        public DataTable ExecuteTableFunction(string name, Dictionary<string, object> parameters = null)
        {
            DataTable res = new DataTable();

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.CommandTimeout = 36000;
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.CommandText = "SELECT * FROM " + name + "(" + SetParams(cmd, parameters) + ")";
                
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.SelectCommand.CommandTimeout = 36000;
                conn.Open();
                da.Fill(res);             
            }
            return res;
        }

        /// <summary>
        /// Unsafe - do not use unless you have to and there are no dynamic parameters included.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataTable ExecuteSelectStatement(string sql, string connectionString = null)
        {
            DataTable res = new DataTable();

            if (connectionString == null)
            {
                connectionString = _connString;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.InfoMessage += MessageHandler;
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.SelectCommand.CommandTimeout = 36000;
                conn.Open();
                    da.Fill(res);               
            }
            return res;
        }

    }
}
