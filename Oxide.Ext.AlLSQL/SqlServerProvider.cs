using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Oxide.Ext.AllSQL
{
    public class SqlServerProvider : IDatabaseProvider
    {
        private SqlConnection _connection;
        string connectionString;

        public string ConnectionString(string host, string database, string user, string password)
        {
            SqlConnectionStringBuilder builder = new()
            {
                DataSource = host,
                InitialCatalog = database,
                UserID = user,
                Password = password
            };
            connectionString = builder.ConnectionString;
            return connectionString;
        }

        public void Open()
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public void Close() => _connection?.Close();

        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            using SqlCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, params object[] parameters)
        {
            using SqlCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteScalar();
        }

        public IDataReader ExecuteReader(string query, params object[] parameters)
        {
            using SqlCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteReader();
        }

        public IDbTransaction BeginTransaction() => _connection.BeginTransaction();

        private static void AddParameters(SqlCommand cmd, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", parameters[i]);
            }
        }

        public void Backup(string destinationPath)
        {
            EnsureOpen();

            // Validate path and build full backup SQL command
            string dbName = _connection.Database;
            string backupFile = Path.GetFullPath(destinationPath);

            // SQL Server uses single quotes and requires doubled-up backslashes for file paths
            string sql = $@"
                BACKUP DATABASE [{dbName}]
                TO DISK = N'{backupFile.Replace("'", "''")}'
                WITH FORMAT, INIT, NAME = N'{dbName}_FullBackup',
                SKIP, NOREWIND, NOUNLOAD, STATS = 10;
            ";

            using SqlCommand cmd = new(sql, _connection);
            cmd.CommandTimeout = 0; // Allow long backups
            cmd.ExecuteNonQuery();
        }

        private void EnsureOpen()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Database not open.");
            }
        }

        public void Dispose() => _connection?.Dispose();

        //public async Task OpenAsync()
        //{
        //    _connection = new(connectionString);
        //    await _connection.OpenAsync();
        //}

        //public async Task<int> ExecuteNonQueryAsync(string query, params object[] parameters)
        //{
        //    using SqlCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteNonQueryAsync();
        //}

        //public async Task<object> ExecuteScalarAsync(string query, params object[] parameters)
        //{
        //    using SqlCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteScalarAsync();
        //}

        //public async Task<IDataReader> ExecuteReaderAsync(string query, params object[] parameters)
        //{
        //    SqlCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        //}
    }
}
