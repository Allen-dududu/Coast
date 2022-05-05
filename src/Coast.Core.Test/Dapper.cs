using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
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
(""Id"", ""State"", ""CreateTime"") 
VALUES (@Id, @State, @CreateTime); ";
        [Fact]
        public async Task Test1Async()
        {
            //var conn = new NpgsqlConnection("Host=localhost;Port=5432;database=postgres;User Id=postgres;Password=root;");
            //conn.Open();
            //var tr = conn.BeginTransaction();
            //await conn.ExecuteAsync(
            //        InsertSagaSql,
            //        new { Id = 5, State = SagaStateEnum.Started, CreateTime = DateTime.UtcNow },
            //        transaction: tr).ConfigureAwait(false);
            //var conn2 = new NpgsqlConnection("Host=localhost;Port=5432;database=postgres;User Id=alean;Password=alean;");
            //await conn2.ExecuteAsync(
            //        InsertSagaSql,
            //        new {  Id = 6, State = SagaStateEnum.Started, CreateTime = DateTime.UtcNow },
            //        transaction: tr).ConfigureAwait(false);

            //throw new Exception();

            //conn2.Close();

            //tr.Commit();
        }
    }
}
