using HizkitzaServer.game.util;
using HizkitzaServer.game.world.entity;
using HizkitzaServer.util;
using HizkitzaServer.world;

namespace HizkitzaServer.game.world;

public class EntityMap(World world) : IExecutable
{
    public List<Entity> Entities { get; } = [];
    public World World { get; } = world;

    public List<string> GetMap() => World.GetMap();

    // Functions //

    public void AddEntity(Entity entity)
    {
        entity.EntityMap = this;
        Entities.Add(entity);
    }

    public bool AnyEntityIn(int x, int y)
    {
        foreach (Entity entity in Entities)
            if (entity.IsEntityIn(x, y))
                return true;
        return false;
    }

    public List<Entity> EntitiesIn(int x, int y)
    {
        List<Entity> ents = [];
        foreach (Entity entity in Entities)
            if (entity.IsEntityIn(x, y))
                ents.Add(entity);
        return ents;
    }

    public void Exe()
    {
        foreach (Entity entity in Entities)
            entity.Exe();
    }
}
