using HizkitzaServer.util.connection;
using System.Timers;
using Timer = System.Timers.Timer;

namespace HizkitzaServer;

public class Game
{
    public readonly string Izena;
    public readonly string Mapa;
    public readonly List<ServersideClient> Players = [];
    //private Timer timer;
    
    public Game(string izena, string mapa)
    {
        Izena = izena;
        Mapa = mapa;
        /*SetUpTimer();
        new Thread(() =>
        {
            
        }).Start();*/
    }

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

    /*private void SetUpTimer()
    {
        timer = new Timer(50);
        timer.AutoReset = true;
        timer.Enabled = true;
        timer.Elapsed += TimerAction;
    }

    private void TimerAction(object sender, ElapsedEventArgs e)
    {

    }*/
}