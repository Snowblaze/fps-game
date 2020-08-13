using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class EnemySpawnSystem : ComponentSystem
{
    private Random rnd;

    protected override void OnCreate()
    {
        rnd = new Random(1);
    }

    protected override void OnUpdate()
    {
        var query = EntityManager.CreateEntityQuery(typeof(EnemyComponent));
        int count = query.CalculateEntityCount();
        if (count == 0)
        {
            var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
            var ghostId = FPSGameGhostSerializerCollection.FindGhostType<EnemySnapshotData>();
            var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
            var enemy = EntityManager.Instantiate(prefab);

            EntityManager.SetComponentData(enemy, new Translation { Value = new float3(rnd.NextFloat(-4.5f, 4.5f), 0.0f, rnd.NextFloat(-4.5f, 4.5f)) });
        }
    }
}
