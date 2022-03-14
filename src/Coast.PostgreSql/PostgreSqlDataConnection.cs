﻿namespace Coast.PostgreSql
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using Npgsql;

    static class PostgreSqlDataConnection
    {
        /// <summary>
        /// get the db connection.
        /// </summary>
        /// <param name="connStr"></param>
        /// <returns>dataconnection.</returns>
        public static IDbConnection OpenConnection(string connStr)
        {
            var conn = new NpgsqlConnection(connStr);
            conn.Open();
            return conn;
        }
    }
}
