using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct FPSGameGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "PlayerGhostSerializer",
            "EnemyGhostSerializer",
        };
        return arr;
    }

    public int Length => 2;
#endif
    public void Initialize(World world)
    {
        var curPlayerGhostSpawnSystem = world.GetOrCreateSystem<PlayerGhostSpawnSystem>();
        m_PlayerSnapshotDataNewGhostIds = curPlayerGhostSpawnSystem.NewGhostIds;
        m_PlayerSnapshotDataNewGhosts = curPlayerGhostSpawnSystem.NewGhosts;
        curPlayerGhostSpawnSystem.GhostType = 0;
        var curEnemyGhostSpawnSystem = world.GetOrCreateSystem<EnemyGhostSpawnSystem>();
        m_EnemySnapshotDataNewGhostIds = curEnemyGhostSpawnSystem.NewGhostIds;
        m_EnemySnapshotDataNewGhosts = curEnemyGhostSpawnSystem.NewGhosts;
        curEnemyGhostSpawnSystem.GhostType = 1;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_PlayerSnapshotDataFromEntity = system.GetBufferFromEntity<PlayerSnapshotData>();
        m_EnemySnapshotDataFromEntity = system.GetBufferFromEntity<EnemySnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<FPSGameGhostDeserializerCollection>.InvokeDeserialize(m_PlayerSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            case 1:
                return GhostReceiveSystem<FPSGameGhostDeserializerCollection>.InvokeDeserialize(m_EnemySnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_PlayerSnapshotDataNewGhostIds.Add(ghostId);
                m_PlayerSnapshotDataNewGhosts.Add(GhostReceiveSystem<FPSGameGhostDeserializerCollection>.InvokeSpawn<PlayerSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            case 1:
                m_EnemySnapshotDataNewGhostIds.Add(ghostId);
                m_EnemySnapshotDataNewGhosts.Add(GhostReceiveSystem<FPSGameGhostDeserializerCollection>.InvokeSpawn<EnemySnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<PlayerSnapshotData> m_PlayerSnapshotDataFromEntity;
    private NativeList<int> m_PlayerSnapshotDataNewGhostIds;
    private NativeList<PlayerSnapshotData> m_PlayerSnapshotDataNewGhosts;
    private BufferFromEntity<EnemySnapshotData> m_EnemySnapshotDataFromEntity;
    private NativeList<int> m_EnemySnapshotDataNewGhostIds;
    private NativeList<EnemySnapshotData> m_EnemySnapshotDataNewGhosts;
}
public struct EnableFPSGameGhostReceiveSystemComponent : IComponentData
{}
public class FPSGameGhostReceiveSystem : GhostReceiveSystem<FPSGameGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableFPSGameGhostReceiveSystemComponent>();
    }
}
