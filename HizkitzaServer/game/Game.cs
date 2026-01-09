using System.Timers;
using Timer = System.Timers.Timer;

namespace HizkitzaServer;

public class Game
{
    public string gameShot { get; private set; } = "";
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
        foreach(var player in players)
            player.Send(gameShot);
    }
}