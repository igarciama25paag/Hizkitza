namespace HizkitzaServer.game.util;

public sealed class Delayer(Clock clock)
{
    Dictionary<string, long> Delays { get; }

    // Defaults //

    public void Declare(string name) => Delays[name] = 0;

    public bool Delay(string name, int delay)
    {
        if (Delays[name] > clock.Milliseconds()) return false;
        Delays[name] = clock.Milliseconds() + delay;
        return true;
    }
}
