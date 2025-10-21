using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Oxide.Ext.AllSQL
{
    public class SQLiteProvider : IDatabaseProvider
    {
        private SQLiteConnection _connection;
        string connectionString;

        public string ConnectionString(string host, string database, string user = "", string password = "")
        {
            connectionString = $"Data Source={host}" + Path.DirectorySeparatorChar + database;
            return connectionString;
        }

        public void Open()
        {
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
        }

        public void Close() => _connection?.Close();

        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            using SQLiteCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, params object[] parameters)
        {
            using SQLiteCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteScalar();
        }

        public IDataReader ExecuteReader(string query, params object[] parameters)
        {
            using SQLiteCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteReader();
        }


        public IDbTransaction BeginTransaction() => _connection.BeginTransaction();

        private static void AddParameters(SQLiteCommand cmd, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@p{i}", parameters[i]);
            }
        }

        public void Backup(string destinationPath)
        {
            EnsureOpen();

            using SQLiteConnection destination = new($"Data Source={destinationPath};Version=3;");
            destination.Open();
            destination.BackupDatabase(_connection, "main", "main", -1, null, 0);
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
        //    using SQLiteCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteNonQueryAsync();
        //}

        //public async Task<object> ExecuteScalarAsync(string query, params object[] parameters)
        //{
        //    using SQLiteCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteScalarAsync();
        //}

        //public async Task<IDataReader> ExecuteReaderAsync(string query, params object[] parameters)
        //{
        //    SQLiteCommand cmd = new(query, _connection);
        //    AddParameters(cmd, parameters);
        //    return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        //}
    }
}
