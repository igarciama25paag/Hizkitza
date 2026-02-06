using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaServer.util.data
{
    public record class PartidaStats(
        int Partida_id,
        int Erabiltzaile_id,
        string Izena,
        string Iraupena,
        int Erabiltzaile_max,
        string Mapa,
        string Sorkuntza_data
        )
    {
        public override string ToString()
        {
            return $"{Partida_id} {Erabiltzaile_id} {Izena} {Iraupena} {Erabiltzaile_max} {Mapa} {Sorkuntza_data}";
        }
    }
}
