using HizkitzaServer.util.connection;
using System.Timers;
using Timer = System.Timers.Timer;

namespace HizkitzaServer;

public class Game
{
    List<ServersideClient> players = [];
    private Timer timer;
    
    public Game()
    {
        SetUpTimer();
        new Thread(() =>
        {
            
        }).Start();
    }

    private void SetUpTimer()
    {
        timer = new Timer(50);
        timer.AutoReset = true;
        timer.Enabled = true;
        timer.Elapsed += TimerAction;
    }

    private void TimerAction(object sender, ElapsedEventArgs e)
    {

    }
}