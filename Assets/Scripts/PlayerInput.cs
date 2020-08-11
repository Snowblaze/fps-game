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

public struct PlayerInput : ICommandData<PlayerInput>
{
    public uint Tick => tick;
    public uint tick;
    public int horizontal;
    public int vertical;

    public int mouseDeltaX;
    public int mouseDeltaY;

    public void Deserialize(uint tick, ref DataStreamReader reader)
    {
        this.tick = tick;
        horizontal = reader.ReadInt();
        vertical = reader.ReadInt();
        mouseDeltaX = reader.ReadInt();
        mouseDeltaY = reader.ReadInt();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteInt(horizontal);
        writer.WriteInt(vertical);
        writer.WriteInt(mouseDeltaX);
        writer.WriteInt(mouseDeltaY);
    }

    public void Deserialize(uint tick, ref DataStreamReader reader, PlayerInput baseline, NetworkCompressionModel compressionModel)
    {
        Deserialize(tick, ref reader);
    }

    public void Serialize(ref DataStreamWriter writer, PlayerInput baseline, NetworkCompressionModel compressionModel)
    {
        Serialize(ref writer);
    }
}

public class PlayerSendCommandSystem : CommandSendSystem<PlayerInput> { }

public class PlayerReceiveCommandSystem : CommandReceiveSystem<PlayerInput> { }

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SamplePlayerInput : ComponentSystem
{
    InputAction moveAction;
    InputAction lookAction;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableFPSGameGhostReceiveSystemComponent>();

        moveAction = GameObject.FindObjectOfType<UnityEngine.InputSystem.PlayerInput>().actions.actionMaps.Single(x => x.name == "Player").actions.Single(x => x.name == "Move");
        lookAction = GameObject.FindObjectOfType<UnityEngine.InputSystem.PlayerInput>().actions.actionMaps.Single(x => x.name == "Player").actions.Single(x => x.name == "Look");
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<PlayerInput>().ForEach((Entity ent, ref MovePlayerComponent player) =>
            {
                if (player.Id == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<PlayerInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            });
            return;
        }
        var input = default(PlayerInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        input.horizontal = Mathf.RoundToInt(moveAction.ReadValue<Vector2>().x);
        input.vertical = Mathf.RoundToInt(moveAction.ReadValue<Vector2>().y);
        input.mouseDeltaX = Mathf.RoundToInt(lookAction.ReadValue<Vector2>().x);
        input.mouseDeltaY = Mathf.RoundToInt(lookAction.ReadValue<Vector2>().y);

        var inputBuffer = EntityManager.GetBuffer<PlayerInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}