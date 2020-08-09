using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public struct CubeInput : ICommandData<CubeInput>
{
    public uint Tick => tick;
    public uint tick;
    public int horizontal;
    public int vertical;

    public float x;
    public float y;
    public float z;
    public float w;

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        horizontal = reader.ReadInt();
        vertical = reader.ReadInt();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(horizontal);
        writer.WriteInt(vertical);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, CubeInput baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    public void Serialize(ref DataStreamWriter writer, CubeInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }
}

public class NetCubeSendCommandSystem : CommandSendSystem<CubeInput> { }

public class NetCubeReceiveCommandSystem : CommandReceiveSystem<CubeInput> { }

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SampleCubeInput : ComponentSystem
{
    InputAction action;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableFPSGameGhostReceiveSystemComponent>();

        action = GameObject.FindObjectOfType<PlayerInput>().actions.actionMaps.Single(x => x.name == "Player").actions.Single(x => x.name == "Move");
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<CubeInput>().ForEach((Entity ent, ref MovableCubeComponent cube) =>
            {
                if (cube.Id == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<CubeInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            });
            return;
        }
        var input = default(CubeInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        input.horizontal = Mathf.RoundToInt(action.ReadValue<Vector2>().x);
        input.vertical = Mathf.RoundToInt(action.ReadValue<Vector2>().y);

        var inputBuffer = EntityManager.GetBuffer<CubeInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}