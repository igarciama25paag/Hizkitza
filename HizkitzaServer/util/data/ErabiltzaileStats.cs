using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaServer.util.data
{
    public record class ErabiltzaileStats(
        int Erabiltzaile_id,
        char Azken_itxura,
        string Azken_kolorea,
        int Partida_sartu_n,
        string Partida_t_max,
        string Azken_sartu_partida
        )
    {
        public override string ToString()
        {
            return $"{Erabiltzaile_id} {Azken_itxura} {Azken_kolorea} {Partida_sartu_n} {Partida_t_max} {Azken_sartu_partida}";
        }
    }
}
