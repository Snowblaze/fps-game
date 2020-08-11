using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class MovePlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref LocalToWorld localToWorld, ref Translation trans, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            float3 forward = localToWorld.Forward;
            float3 right = localToWorld.Right;
            trans.Value += right * deltaTime * input.horizontal;
            trans.Value += forward * deltaTime * input.vertical;
        });
    }
}
