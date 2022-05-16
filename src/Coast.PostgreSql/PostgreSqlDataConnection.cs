namespace Coast.PostgreSql
{
    using Npgsql;
    using System.Data;

    static class PostgreSqlDataConnection
    {
        /// <summary>
        /// get the db connection.
        /// </summary>
        /// <param name="connStr">connection string.</param>
        /// <returns>dataconnection.</returns>
        public static IDbConnection OpenConnection(string connStr)
        {
            var conn = new NpgsqlConnection(connStr);
            conn.Open();
            return conn;
        }
    }
}
