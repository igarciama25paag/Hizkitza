namespace HizkitzaServer.game.world.entity;

public abstract class Moving : Entity
{
    protected List<char> Colliders { get; } = [];
    protected List<Type> EntityColliders { get; } = [];

    // Constructors //

    protected Moving(EntityMap entityMap, int x, int y, char look) : base(entityMap, x, y, look)
    {
        try { DeclareMapColliders(); } catch(NullReferenceException){}
        try { DeclareEntityColliders(); } catch(NullReferenceException){}
    }

    // Functions //

    protected void MoveHrz(int x)
    {
        if (!Collides(X + x, Y)) X += x;
    }

    protected void MoveVert(int y)
    {
        if (!Collides(X, Y + y)) Y += y;
    }

    private bool Collides(int x, int y)
    {
        var symbol = EntityMap.World.SymbolIn(x, y);

        if (symbol == null) return true;

        if (EntityColliders.Count > 0)
            foreach (Entity entity in EntityMap.EntitiesIn(x, y))
                foreach (Type entityCollider in EntityColliders)
                    if (entity.GetType() == entityCollider)
                        return true;

        foreach (char collider in Colliders)
            if (symbol == collider)
                return true;

        return false;
    }

    // Abstract //

    protected abstract void DeclareMapColliders();

    protected abstract void DeclareEntityColliders();
}
