using HizkitzaServer.util.connection;
using System.Timers;
using Timer = System.Timers.Timer;

namespace HizkitzaServer;

public class Game
{
    public readonly string Izena;
    public readonly string Mapa;
    private readonly List<ServersideClient> Players = [];
    private readonly object playersLock = new();
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

    public List<ServersideClient> GetPlayers()
    {
        lock (playersLock) return Players;
    }

    public void AddPlayer(ServersideClient client)
    {
        lock (playersLock) Players.Add(client);
    }

    public void RemovePlayer(ServersideClient client)
    {
        lock (playersLock) Players.Remove(client);
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