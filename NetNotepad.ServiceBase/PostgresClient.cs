using Serilog;
using Npgsql;

namespace NetNotepad.ServiceBase
{
    public static class PostgresClient
    {

        public static async Task<string> Connect(string hostVar = "POSTGRESDBHOST", string portVar = "POSTGRESDBPORT", string nameVar = "POSTGRESDBNAME",
            string userVar = "POSTGRESDBUSER", string passVar = "POSTGRESDBPASS")
        {
            string DB_Host = Environment.GetEnvironmentVariable(hostVar) ?? throw new Exception("No Postgres DB host string"); ;
            string DB_Port = Environment.GetEnvironmentVariable(portVar) ?? throw new Exception("No Postgres DB port string"); ;
            string DB_Name = Environment.GetEnvironmentVariable(nameVar) ?? throw new Exception("No Postgres DB name string"); ;
            string DB_User = Environment.GetEnvironmentVariable(userVar) ?? throw new Exception("No Postgres DB user string"); ;
            string DB_Pass = Environment.GetEnvironmentVariable(passVar) ?? throw new Exception("No Postgres DB password string"); ;
            Log.Information("Found postgres DB host, port, name, user, password strings");
            string postgresConnect = string.Format("Host={0};Port={1};Database={2};Username={3};Password={4}", DB_Host, DB_Port, DB_Name, DB_User, DB_Pass);
            await using NpgsqlConnection connection = new(postgresConnect);
            await connection.OpenAsync();
            await connection.CloseAsync();
            return postgresConnect;
        }
    }
}