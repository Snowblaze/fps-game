using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
public class Game : ComponentSystem
{
    struct InitGameComponent : IComponentData { }

    protected override void OnCreate()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked;

        RequireSingletonForUpdate<InitGameComponent>();
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    protected override void OnUpdate()
    {
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());
        foreach (var world in World.All)
        {
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null)
            {
                NetworkEndPoint ep = NetworkEndPoint.LoopbackIpv4;
                ep.Port = 7979;
                network.Connect(ep);
            }
#if UNITY_EDITOR
            else if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null)
            {
                NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
                ep.Port = 7979;
                network.Listen(ep);
            }
#endif
        }
    }
}
