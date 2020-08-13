using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ShootPlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref LocalToWorld localToWorld, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            if(input.shoot == 1)
            {
                Entity entity = Raycast(localToWorld.Position + localToWorld.Forward * 0.6f, localToWorld.Forward * 10.0f);
                if(entity != Entity.Null)
                {
                    PostUpdateCommands.DestroyEntity(entity);
                }
            }
        });
    }

    public Entity Raycast(float3 RayFrom, float3 RayTo)
    {
        foreach (var world in World.All)
        {
            if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            {
                var physicsWorldSystem = world.GetExistingSystem<BuildPhysicsWorld>();
                var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
                RaycastInput input = new RaycastInput()
                {
                    Start = RayFrom,
                    End = RayTo,
                    Filter = new CollisionFilter()
                    {
                        BelongsTo = ~0u,
                        CollidesWith = ~0u, // all 1s, so all layers, collide with everything
                        GroupIndex = 0
                    }
                };

                RaycastHit hit = new RaycastHit();
                bool haveHit = collisionWorld.CastRay(input, out hit);
                if (haveHit)
                {
                    // see hit.Position
                    // see hit.SurfaceNormal
                    Entity e = physicsWorldSystem.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                    return e;
                }
            }
        }
        return Entity.Null;
    }
}
