using HizkitzaServer.util.connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HizkitzaServer.util.connection.ServersideClient;

namespace HizkitzaServer.util.data
{
    public record class Erabiltzailea(
        int Id,
        string Izena,
        string Pasahitza,
        ConnectionType Mota,
        string Sorkuntza_data
        )
    {
        public override string ToString()
        {
            return $"{Id} {Izena} {Pasahitza} {Mota} {Sorkuntza_data}";
        }
    }
}
