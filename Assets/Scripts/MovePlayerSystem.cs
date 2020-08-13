using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class MovePlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref LocalToWorld localToWorld, ref PhysicsVelocity velocity, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            float3 forward = localToWorld.Forward;
            float3 right = localToWorld.Right;

            float3 direction = forward * input.vertical + right * input.horizontal;
            float3 vel = direction;
            vel.y = velocity.Linear.y;

            velocity.Linear = vel;
        });
    }
}
