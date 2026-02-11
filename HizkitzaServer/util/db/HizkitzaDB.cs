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
        public static string HOST = "localhost";
        public static string PORT = "5432";
        public static string USERNAME = "admin";
        public static string PASSWORD = "admin";
        public static string DATABASE = "hizkitza";

        private static string CONNECTION = "" +
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
        public static async Task<Erabiltzailea> LoginErabiltzailea(string user, string pass)
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
                        reader.GetDateTime(4).ToString(@"yyyy-MM-dd")
                        );
                }
            });
        }

        /**
         * TXOSTENAK
         * */

        // Erabiltzailea datu baseari eskaera
        public static async Task<Erabiltzailea> GetErabiltzailea(string user)
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT * FROM \"Erabiltzaileak\" " +
                    $"WHERE izena = '{user}'"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return new Erabiltzailea(
                        reader.GetInt16(0),
                        reader.GetString(1).Trim(),
                        reader.GetString(2).Trim(),
                        Enum.Parse<ConnectionType>(reader.GetString(3).Trim()),
                        reader.GetDateTime(4).ToString(@"yyyy-MM-dd")
                        );
                }
            });
        }

        // Erabiltzaile estatistikak datu baseari eskaera
        public static async Task<ErabiltzaileStats> ErabiltzaileStats(string user)
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT E.erabiltzaile_id, azken_itxura, azken_kolorea, partida_sartu_n, partida_t_max, izena " +
                    "FROM \"ErabiltzaileakStats\" E " +
                    "INNER JOIN \"PartidakStats\" " +
                    "  ON azken_sartu_partida = partida_id " +
                    "WHERE E.erabiltzaile_id = ( " +
                    "  SELECT id " +
                    "  FROM \"Erabiltzaileak\" " +
                    $"  WHERE izena = '{user}')"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return new ErabiltzaileStats(
                        reader.GetInt16(0),
                        reader.GetChar(1),
                        reader.GetString(2).Trim(),
                        reader.GetInt16(3),
                        reader.GetTimeSpan(4).ToString(@"hh\:mm\:ss"),
                        reader.GetString(5).Trim()
                        );
                }
            });
        }

        // Erabiltziale baten partida famatuena datu baseari eskaera
        public static async Task<PartidaStats> ErabiltzailePartidaFamatuena(string user)
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT partida_id, E.izena, P.izena, iraupena, erabiltzaile_max, mapa, P.sorkuntza_data " +
                    "FROM \"PartidakStats\" P " +
                    "INNER JOIN \"Erabiltzaileak\" E " +
                    "  ON P.erabiltzaile_id = E.id " +
                    $"WHERE E.izena = '{user}' " +
                    "ORDER BY erabiltzaile_max DESC LIMIT 1"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return new PartidaStats(
                        reader.GetInt16(0),
                        reader.GetString(1).Trim(),
                        reader.GetString(2).Trim(),
                        reader.GetTimeSpan(3).ToString(@"hh\:mm\:ss"),
                        reader.GetInt16(4),
                        reader.GetString(5).Trim(),
                        reader.GetDateTime(6).ToString(@"yyyy-MM-dd")
                        );
                }
            });
        }

        // Top 10 partida famatuenak datu baseari eskaera
        public static async Task<List<PartidaStats>> Top10Partidak()
        {
            return await DBRequest(async dataSource => {
                await using var cmd = dataSource.CreateCommand(
                    "SELECT partida_id, E.izena, P.izena, iraupena, erabiltzaile_max, mapa, P.sorkuntza_data " +
                    "FROM \"PartidakStats\" P " +
                    "INNER JOIN \"Erabiltzaileak\" E " +
                    "  ON P.erabiltzaile_id = E.id " +
                    "ORDER BY erabiltzaile_max DESC LIMIT 10"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    var list = new List<PartidaStats>();

                    while (await reader.ReadAsync())
                        list.Add(new(
                            reader.GetInt16(0),
                            reader.GetString(1).Trim(),
                            reader.GetString(2).Trim(),
                            reader.GetTimeSpan(3).ToString(@"hh\:mm\:ss"),
                            reader.GetInt16(4),
                            reader.GetString(5).Trim(),
                        reader.GetDateTime(6).ToString(@"yyyy-MM-dd")
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
                    "SELECT partida_id, E.izena, P.izena, iraupena, erabiltzaile_max, mapa, P.sorkuntza_data " +
                    "FROM \"PartidakStats\" P " +
                    "INNER JOIN \"Erabiltzaileak\" E " +
                    "  ON P.erabiltzaile_id = E.id " +
                    "ORDER BY iraupena DESC LIMIT 1"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return new PartidaStats(
                        reader.GetInt16(0),
                        reader.GetString(1).Trim(),
                        reader.GetString(2).Trim(),
                        reader.GetTimeSpan(3).ToString(@"hh\:mm\:ss"),
                        reader.GetInt16(4),
                        reader.GetString(5).Trim(),
                        reader.GetDateTime(6).ToString(@"yyyy-MM-dd")
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
                    "ORDER BY COUNT(sorkuntza_data) DESC LIMIT 1"
                    );
                await using var reader = await cmd.ExecuteReaderAsync();
                {
                    await reader.ReadAsync();
                    return reader.GetDateTime(0).ToString(@"yyyy-MM-dd");
                }
            });
        }
    }
}
