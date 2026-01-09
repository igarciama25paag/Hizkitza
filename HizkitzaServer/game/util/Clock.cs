namespace HizkitzaServer.game.util;

using System.Timers;
using Timer = System.Timers.Timer;

public class Clock
{
    private long Millis;
    public ElapsedEventHandler TimerAction;

    // Constructors //

    public Clock()
    {
        Millis = 0;
        var timer = new Timer(50)
        {
            AutoReset = true,
            Enabled = true
        };
        timer.Elapsed += TimerAction;
    }

    // Getters and Setters //

    public long Milliseconds() => Millis;
    
    public long Seconds() => Milliseconds() / 1000;

    public long Minutes() => Seconds() / 60;

    public long Hours() => Minutes() / 60;

    // Object //

    public override string ToString()
    {
        return (Hours() < 10 ? "0" + Hours() : Hours()) +
               (Minutes() - 60 * Hours() < 10 ? " : 0" : " : ") +
               (Minutes() - 60 * Hours()) +
               (Seconds() - 60 * Minutes() < 10 ? " : 0" : " : ") +
               (Seconds() - 60 * Minutes());
    }
}
