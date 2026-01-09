using HizkitzaServer.game.util;
using HizkitzaServer.util;

namespace HizkitzaServer.game.world.entity;

public abstract class Entity : IExecutable
{
    public EntityMap EntityMap { get; set; }
    public int X { get; protected set; }
    public int Y { get; protected set; }
    public char Look { get; protected set; }
    protected Delayer Delayer;

    // Constructors //

    protected Entity(EntityMap entityMap, int x, int y, char look)
    {
        EntityMap = entityMap;
        X = x;
        Y = y;
        Look = look;
        Delayer = new Delayer(EntityMap.World.Clock);
        try { DeclareDelays(); } catch(NullReferenceException){}
    }

    // Functions //

    public bool IsEntityIn(int x, int y) => X == x && Y == y;

    // Abstract //

    public abstract void Exe();
    protected abstract void DeclareDelays();
}
