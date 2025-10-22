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
            EnsureOpen();
            query = NormalizeQuery(query);
            using SQLiteCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, params object[] parameters)
        {
            EnsureOpen();
            query = NormalizeQuery(query);
            using SQLiteCommand cmd = new(query, _connection);
            AddParameters(cmd, parameters);
            return cmd.ExecuteScalar();
        }

        public IDataReader ExecuteReader(string query, params object[] parameters)
        {
            EnsureOpen();
            query = NormalizeQuery(query);
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
            //destination.BackupDatabase(_connection, "main", "main", -1, null, 0);
            _connection.BackupDatabase(destination, "main", "main", -1, null, 0);
            destination.Close();
        }

        private void EnsureOpen()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Database not open.");
            }
        }

        private static string NormalizeQuery(string query)
        {
            // Only strip "main." if no ATTACH is in use
            // We'll check for "ATTACH" in the query to be safe
            if (query.IndexOf("main.", StringComparison.OrdinalIgnoreCase) >= 0 &&
                query.IndexOf("ATTACH", StringComparison.OrdinalIgnoreCase) < 0 &&
                query.IndexOf("other.", StringComparison.OrdinalIgnoreCase) < 0)
            {
                query = ReplaceInsensitive(query, "main.", "");
            }

            return query;
        }

        private static string ReplaceInsensitive(string source, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue))
            {
                return source;
            }
            int index = 0;
            while (true)
            {
                index = source.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
                if (index < 0) break;

                source = source.Remove(index, oldValue.Length).Insert(index, newValue);
                index += newValue.Length;
            }

            return source;
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
