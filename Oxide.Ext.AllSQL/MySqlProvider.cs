using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;

namespace Oxide.Ext.AllSQL
{
    public class MySqlProvider : IDatabaseProvider
    {
        private MySqlConnection _connection;
        string connectionString;

        public string ConnectionString(string host, string database, string user, string password)
        {
            MySqlConnectionStringBuilder builder = new()
            {
                Server = host,
                Database = database,
                UserID = user,
                Password = password
            };
            connectionString = builder.ConnectionString;
            return connectionString;
        }

        public void Open()
        {
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        public void Close() => _connection?.Close();

        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            using MySqlCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }


        public object ExecuteScalar(string query, params object[] parameters)
        {
            using MySqlCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteScalar();
        }

        public IDataReader ExecuteReader(string query, params object[] parameters)
        {
            MySqlCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            // Note: CommandBehavior.CloseConnection optional, if you want reader close to also close connection
            return cmd.ExecuteReader();
        }

        public IDbTransaction BeginTransaction() => _connection.BeginTransaction();

        private static void AddParameters(MySqlCommand cmd, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
                cmd.Parameters.AddWithValue($"@p{i}", parameters[i]);
        }

        public void Backup(string destinationPath)
        {
            MySqlConnectionStringBuilder builder = new(_connection.ConnectionString);

            ProcessStartInfo psi = new()
            {
                FileName = "mysqldump",
                Arguments = $"--user={builder.UserID} --password={builder.Password} --host={builder.Server} {builder.Database} > \"{destinationPath}\"",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            using Process proc = Process.Start(psi);
            proc.WaitForExit();
        }

        public void Dispose() => _connection?.Dispose();

        //public async Task OpenAsync()
        //{
        //    _connection = new MySqlConnection(connectionString);
        //    await _connection.OpenAsync();
        //}

        //public async Task<int> ExecuteNonQueryAsync(string query, params object[] parameters)
        //{
        //    using MySqlCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteNonQueryAsync();
        //}

        //public async Task<object> ExecuteScalarAsync(string query, params object[] parameters)
        //{
        //    using MySqlCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteScalarAsync();
        //}

        //public async Task<IDataReader> ExecuteReaderAsync(string query, params object[] parameters)
        //{
        //    MySqlCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        //}
    }
}
