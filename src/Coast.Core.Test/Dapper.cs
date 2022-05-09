using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Coast.Core.Test
{
    public class Dapper
    {
        private const string InsertSagaSql =
@"INSERT INTO ""Coast_Saga"" 
(""Id"", ""State"", ""CreationTime"") 
VALUES (@Id, @State, @CreationTime); ";
        [Fact(Skip = "For transaction test")]
        public async Task Test1Async()
        {
            using var conn = new NpgsqlConnection("Host=localhost;Port=5432;database=postgres;User Id=postgres;Password=root;");
            conn.Open();
            var tr = conn.BeginTransaction();
            await conn.ExecuteAsync(
                    InsertSagaSql,
                    new { Id = 5, State = SagaStateEnum.Started, CreationTime = DateTime.UtcNow },
                    transaction: tr).ConfigureAwait(false);
            await ChangeDb(conn, tr);

            throw new Exception();
            tr.Commit();
        }

        private async Task ChangeDb(IDbConnection db , IDbTransaction tx)
        {
            var conn2 = new NpgsqlConnection("Host=localhost;Port=5432;database=postgres;User Id=alean;Password=alean;");
            await db.ExecuteAsync(
                    InsertSagaSql,
                    new { Id = 6, State = SagaStateEnum.Started, CreationTime = DateTime.UtcNow },
                    transaction: tx).ConfigureAwait(false);


            conn2.Close();
        }
    }
}
