using System;
using System.Data;

namespace Oxide.Ext.AllSQL
{
    public interface IDatabaseProvider : IDisposable
    {
        string ConnectionString(string host, string database, string user, string password);
        void Open();
        void Close();

        int ExecuteNonQuery(string query, params object[] parameters);

        object ExecuteScalar(string query, params object[] parameters);

        IDataReader ExecuteReader(string query, params object[] parameters);

        IDbTransaction BeginTransaction();

        abstract void Backup(string destinationPath);

        //Task<int> ExecuteNonQueryAsync(string query, params object[] parameters);
        //Task<object> ExecuteScalarAsync(string query, params object[] parameters);
        //Task<IDataReader> ExecuteReaderAsync(string query, params object[] parameters);
    }
}
