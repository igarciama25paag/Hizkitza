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
        );
}
