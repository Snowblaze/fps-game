using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine.Jobs;

[UpdateInGroup(typeof(TransformSystemGroup))]
[UpdateAfter(typeof(EndFrameLocalToParentSystem))]
public class CameraFollowSystem : JobComponentSystem
{
    private EntityQuery localPlayer;
    private EntityQuery camTransform;

    protected override void OnCreate()
    {
        localPlayer = GetEntityQuery(ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadOnly<PlayerInput>());
        camTransform = GetEntityQuery(ComponentType.ReadWrite<LocalToWorld>(), typeof(UnityEngine.Transform), ComponentType.ReadOnly<CameraTargetComponent>());

        RequireSingletonForUpdate<CameraTargetComponent>();
        RequireSingletonForUpdate<PlayerInput>();
    }

    [BurstCompile]
    struct CameraFollowJob : IJobParallelForTransform
    {
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<LocalToWorld> target;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position = target[0].Position;
            transform.rotation = target[0].Rotation;
        }
    }

    //[BurstCompile]
    //struct CameraFollowJob : IJobForEachWithEntity<LocalToWorld>
    //{
    //    [DeallocateOnJobCompletion] public NativeArray<TransformStash> transformStashes;

    //    [DeallocateOnJobCompletion]
    //    [ReadOnly]
    //    public NativeArray<LocalToWorld> target;

    //    public void Execute(Entity entity, int index, ref LocalToWorld localToWorld)
    //    {
    //        var transformStash = transformStashes[index];

    //        localToWorld = new LocalToWorld
    //        {
    //            Value = float4x4.TRS(
    //                target[0].Position + new float3(0.8f, 1.14f, -3.79f),
    //                target[0].Rotation,
    //                new float3(1.0f, 1.0f, 1.0f))
    //        };
    //    }
    //}

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var transforms = camTransform.GetTransformAccessArray();

        var cameraFollowJob = new CameraFollowJob
        {
            target = localPlayer.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
        };

        return cameraFollowJob.Schedule(transforms, inputDeps);
    }
}
