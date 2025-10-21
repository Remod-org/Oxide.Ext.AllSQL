# Oxide.Ext.AllSQL

An extension for Oxide/uMod providing an abstraction layer for SQLite, MSSQL, and MySQL.

This allows a plugin to provide more flexiblity for database choices.

NOTE: When using this extension with the intent of supporting more than one database type, your queries must be verified to work on all possible choices of database server.  For example, queries such as "INSERT OR UPDATE INTO ..." may only work on SQLite.  SQL is not universal, but if you keep it to simple SELECT, DELETE, INSERT you should be fine.  Since MySQL, SQLite, and even MSSQL (on Linux) are freely available, you should be able to write AND test.

### Configuration
  There is no configuration for this extension


### Available functions

The primary interface is of type IDatabaseProvider:

```cs
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
    }
```

For initial creation of the provider, you should use

```cs
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
```

Your main usage would be, at a minimum:

```cs
    IDatabaseProvider sqlConnection;
    sqlConnection = DB.Create("sql"); // or sqlite, mysql
    sqlConnection.ConnectionString("localhost", "DBName", "dbuser", "dbpass");  // SQL or MySQL
    //sqlConnection.ConnectionString("/path/to/folder", "DBFilename.db", "", ""); // SQLite
    sqlConnection.Open();
```

Once the connection is setup, you can perform the following types of queries:

Simple insert/update query requiring no return
```cs
    string query = "DELETE FROM table WHERE field='nope'";
    sqlConnection.ExecuteNonQuery(query);
```

Simple query containing a result set
```cs
    string query = "SELECT field FROM table";
    System.Data.IDataReader reader = sqlConnection.ExecuteReader(query);
    while (reader.Read())
    {
        string name = reader.GetString(0);
        // Do something with this value, e.g. add to an array, check the value and make a decision, etc.
    }
    reader.Close();
```

Simple query containing one result, e.g. for COUNT
```cs
    object? count = sqlConnection.ExecuteScalar("SELECT COUNT(*) FROM table");
```

### Bigger example

In one of my plugins, I added the following local function to setup my database.  This allows for creation of a master sqlConnection, or other connections for queries requiring a separate connection, e.g. within a loop.

This also shows how you might use configuration to allow the admin to choose the database type.


```cs
        IDatabaseProvider sqlConnection;

        private IDatabaseProvider SetupDB(bool newconn = false)
        {
            switch (newconn)
            {
                case true:
                    IDatabaseProvider newConnection;
                    switch (configData.Options.dbtype)
                    {
                        case "sql":
                            newConnection = DB.Create("sql");
                            newConnection.ConnectionString(configData.Options.dbhost, configData.Options.dbname, configData.Options.dbuser, configData.Options.dbpass);
                            break;
                        case "mysql":
                            newConnection = DB.Create("mysql");
                            newConnection.ConnectionString(configData.Options.dbhost, configData.Options.dbname, configData.Options.dbuser, configData.Options.dbpass);
                            break;
                        case "sqlite":
                        default:
                            newConnection = DB.Create("sqlite");
                            newConnection.ConnectionString(Interface.GetMod().DataDirectory, configData.Options.dbname, "", "");
                            break;
                    }
                    newConnection.Open();
                    return newConnection;
                case false:
                default:
                    switch (configData.Options.dbtype)
                    {
                        case "sql":
                            sqlConnection = DB.Create("sql");
                            sqlConnection.ConnectionString(configData.Options.dbhost, configData.Options.dbname, configData.Options.dbuser, configData.Options.dbpass);
                            break;
                        case "mysql":
                            sqlConnection = DB.Create("mysql");
                            sqlConnection.ConnectionString(configData.Options.dbhost, configData.Options.dbname, configData.Options.dbuser, configData.Options.dbpass);
                            break;
                        case "sqlite":
                        default:
                            sqlConnection = DB.Create("sqlite");
                            sqlConnection.ConnectionString(Interface.GetMod().DataDirectory, configData.Options.dbname, "", "");
                            break;
                    }
                    sqlConnection.Open();
                    return sqlConnection;
            }
        }
```

Note that you can ignore the return for sqlConnection for the master connection, if any.  However, in my case, I called this as follows:

```cs
    sqlConnection = SetupDB();
```

Then, for connections I needed outside of the master connection

```cs
    using IDatabaseProvider c = SetupDB(true);
    string query = $"SELECT * FROM table WHERE field={value}";
    if (c.ExecuteNonQuery(query) > 0)
    {
        // DO SOMETHING
    }
    c.Close();
```

Note that it is probably a good idea to close the connection when done as show above.  You can also close the master connection as follows:

```cs
    private void Unload()
    {
        sqlConnection?.Close();
    }
```
