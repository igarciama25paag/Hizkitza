using HizkitzaServer.util.connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaServer.util.data
{
    public record class Erabiltzailea(
        int Id,
        string Izena,
        string Pasahitza,
        ConnectionType Mota
        );
}
