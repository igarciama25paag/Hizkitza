using HizkitzaServer.util;

namespace HizkitzaServer.game.world.entity;

public class Player : Moving
{
    // Constructors //

    public Player(EntityMap entityMap, int x, int y) : base(entityMap, x, y, '@')
    {
    }

    // Functions //

    protected override void DeclareMapColliders()
    {
        Colliders.Add('-');
        Colliders.Add('|');
    }

    protected override void DeclareEntityColliders()
    {
        EntityColliders.Add(typeof(Entity));
    }

    protected override void DeclareDelays()
    {
        Delayer.Declare("move");
    }

    public override void Exe()
    {
        if (Delayer.Delay("move", 100))
        {
            if (KeyPress.Right()) MoveHrz(1);
            if (KeyPress.Left()) MoveHrz(-1);
            if (KeyPress.Up()) MoveVert(-1);
            if (KeyPress.Down()) MoveVert(1);
        }
    }
}
