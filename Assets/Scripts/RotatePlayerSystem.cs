using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
[UpdateBefore(typeof(MovePlayerSystem))]
public class RotatePlayerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Rotation rot, ref PredictedGhostComponent prediction) =>
        {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);

            rot.Value = MathExtensions.Concatenate(rot.Value, quaternion.Euler(0.0f, input.mouseDeltaX * GameConfig.MouseSensitivity, 0.0f));
        });
    }
}
