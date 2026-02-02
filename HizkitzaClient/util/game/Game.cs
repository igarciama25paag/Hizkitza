using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaClient.util.game
{
    public class Game(string izena, string mapa)
    {
        public string Izena = izena;
        public string Mapa = mapa;

        override public string ToString()
        {
            return Izena;
        }

        override public bool Equals(object? obj)
        {
            if (obj is Game g)
                return g.Izena == Izena;
            return false;
        }

        override public int GetHashCode()
        {
            return Izena.GetHashCode();
        }
    }
}
