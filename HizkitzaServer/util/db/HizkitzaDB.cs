using HizkitzaServer.util.connection;
using HizkitzaServer.util.db.data;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaClient.util.db
{
    static class HizkitzaDB
    {
        // PostgreSQL datu base parametroak
        private const string HOST = "localhost";
        private const string PORT = "5432";
        private const string USERNAME = "admin";
        private const string PASSWORD = "admin";
        private const string DATABASE = "hizkitza";

        private const string CONNECTION = "" +
                $"Host={HOST};" +
                $"Port={PORT};" +
                $"Username={USERNAME};" +
                $"Password={PASSWORD};" +
                $"Database={DATABASE}";

        // Datu baseari komando bat bidali bueltan ezer itxaron gabe
        private static void DBDispatch(string query)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await using var dataSource = NpgsqlDataSource.Create(CONNECTION);
                    dataSource.CreateCommand(query).ExecuteNonQuery();
                }
                catch (NpgsqlException e)
                {
                    Server.LogBerria("SQL:" + e.Message, Server.LogType.ERROR);
                }
            });
        }

        // Eskaera prozesua atributu bezala
        private delegate Task<T> Request<T>(NpgsqlDataSource dataSource);

        // Datu baseari komando bat bidali eta bueltan emaitza itxaron
        private static async Task<T> DBRequest<T>(Request<T> request)
        {
            try
            {
                await using var dataSource = NpgsqlDataSource.Create(CONNECTION);
                return await request(dataSource);
            }
            catch (NpgsqlException e)
            {
                Server.LogBerria("SQL:" + e.Message, Server.LogType.ERROR);
                return default!;
            }
        }

        /**
         * LOGIN
         * **/

        // Datu baseari erabiltzailea eta pasahitza bidali eta erabiltzailea bueltatu
        public static async Task<Erabiltzailea> GetErabiltzailea(string user, string pass)
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT * FROM \"Erabiltzaileak\" " +
                    $"WHERE izena = '{user}' " +
                    $"AND pasahitza = '{pass}'"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();

                    return new Erabiltzailea(
                        reader.GetInt16(0),
                        reader.GetString(1).Trim(),
                        reader.GetString(2).Trim(),
                        Enum.Parse<ConnectionType>(reader.GetString(3).Trim())
                        );
                }
            });
        }
    }
}
