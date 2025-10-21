using System;

namespace Oxide.Ext.AllSQL
{
    public static class DB
    {
        public static IDatabaseProvider Create(string providerType)
        {
            return providerType.ToLower() switch
            {
                "sqlite" => new SQLiteProvider(),
                "sql" => new SqlServerProvider(),
                "mysql" => new MySqlProvider(),
                _ => throw new NotSupportedException($"Provider not supported: {providerType}")
            };
        }
    }
}
