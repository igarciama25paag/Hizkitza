using HizkitzaServer.util.connection;
using HizkitzaServer.util.data;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaServer.util.db
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
                    Server.NewLog("SQL:" + e.Message, Server.LogType.ERROR);
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
                Server.NewLog("SQL:" + e.Message, Server.LogType.ERROR);
                return default!;
            }
        }

        /**
         * LOGIN
         * */

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
                        Enum.Parse<ConnectionType>(reader.GetString(3).Trim()),
                        reader.GetDateTime(5).ToString()
                        );
                }
            });
        }

        /**
         * TXOSTENAK
         * */

        // Erabiltzaile estatistikak datu baseari eskaera
        public static async Task<ErabiltzaileStats> ErabiltzaileStats(string user)
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT * FROM \"ErabiltzaileakStats\" " +
                    "WHERE erabiltzaile_id = ( " +
                    "  SELECT id " +
                    "  FROM \"Erabiltzaileak\" " +
                    $" WHERE izena = '{user}' )"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return new ErabiltzaileStats(
                        reader.GetInt16(0),
                        reader.GetChar(1),
                        reader.GetString(2).Trim(),
                        reader.GetInt16(3),
                        reader.GetString(4).Trim(),
                        reader.GetDateTime(5).ToString()
                        );
                }
            });
        }

        // Erabiltziale baten partida famatuena datu baseari eskaera
        public static async Task<PartidaStats> ErabiltzailePartidaFamatua(string user)
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT * FROM \"PartidakStats\" " +
                    "WHERE erabiltzaile_id = ( " +
                    "  SELECT id " +
                    "  FROM \"Erabiltzaileak\" " +
                    $" WHERE izena = '{user}' ) " +
                    "ORDER BY erabiltzaile_max DESC LIMIT 1"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return new PartidaStats(
                        reader.GetInt16(0),
                        reader.GetInt16(1),
                        reader.GetString(2).Trim(),
                        reader.GetString(3).Trim(),
                        reader.GetInt16(4),
                        reader.GetString(5).Trim(),
                        reader.GetString(6).Trim()
                        );
                }
            });
        }

        // Top 10 partida famatuenak datu baseari eskaera
        public static async Task<List<PartidaStats>> Top10Partidak()
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT * FROM \"PartidakStats\" " +
                    "ORDER BY erabiltzaile_max DESC LIMIT 10"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    var list = new List<PartidaStats>();

                    while (await reader.ReadAsync())
                        list.Add(new(
                            reader.GetInt16(0),
                            reader.GetInt16(1),
                            reader.GetString(2).Trim(),
                            reader.GetString(3).Trim(),
                            reader.GetInt16(4),
                            reader.GetString(5).Trim(),
                            reader.GetString(6).Trim()
                            ));

                    return list;
                }
            });
        }

        // Mapa famatuena datu baseari eskaera
        public static async Task<string> MapaFamatuena()
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT mapa FROM \"PartidakStats\" " +
                    "GROUP BY mapa " +
                    "ORDER BY SUM(erabiltzaile_max) DESC LIMIT 1"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return reader.GetString(0).Trim();
                }
            });
        }

        // Partida luzeena datu baseari eskaera
        public static async Task<PartidaStats> PartidaLuzeena()
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT * FROM \"PartidakStats\" " +
                    "ORDER BY iraupena DESC LIMIT 1"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return new PartidaStats(
                        reader.GetInt16(0),
                        reader.GetInt16(1),
                        reader.GetString(2).Trim(),
                        reader.GetString(3).Trim(),
                        reader.GetInt16(4),
                        reader.GetString(5).Trim(),
                        reader.GetString(6).Trim()
                        );
                }
            });
        }

        // Data aktiboena datu baseari eskaera
        public static async Task<string> DataAktiboena()
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT sorkuntza_data FROM \"PartidakStats\" " +
                    "GROUP BY sorkuntza_data " +
                    "ORDER BY COUNT(sortkuntza_data) DESC LIMIT 1"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return reader.GetString(0).Trim();
                }
            });
        }
    }
}
